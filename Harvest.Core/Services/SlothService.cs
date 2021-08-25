using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.InvoiceModels;
using Harvest.Core.Models.Settings;
using Harvest.Core.Models.SlothModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace Harvest.Core.Services
{
    public interface ISlothService
    {
        Task<Result<SlothResponseModel>> MoveMoney(int invoiceId);

        Task ProcessTransferUpdates();
    }

    public class SlothService : ISlothService
    {
        private readonly AppDbContext _dbContext;
        private readonly SlothSettings _slothSettings;
        private readonly IFinancialService _financialService;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly IProjectHistoryService _historyService;

        public SlothService(AppDbContext dbContext, IOptions<SlothSettings> slothSettings, IFinancialService financialService,
            JsonSerializerOptions serializerOptions, IProjectHistoryService historyService)
        {
            _dbContext = dbContext;
            _slothSettings = slothSettings.Value;
            _financialService = financialService;
            _serializerOptions = serializerOptions;
            _historyService = historyService;
        }


        public async Task<Result<SlothResponseModel>> MoveMoney(int invoiceId)
        {
            var token = _slothSettings.ApiKey;
            var url = _slothSettings.ApiUrl;

            if (string.IsNullOrWhiteSpace(token))
            {
                Log.Error("Sloth Token missing");
            }

            var invoice = await _dbContext.Invoices.Where(a => a.Id == invoiceId && a.Status == Invoice.Statuses.Created).Include(a => a.Expenses)
                .Include(a => a.Project).ThenInclude(a => a.Accounts).SingleOrDefaultAsync();
            if (invoice == null)
            {
                return Result.Error("Invoice not found: {invoiceId}", invoiceId);
            }

            if (invoice.Project.Accounts.Count == 0)
            {
                return Result.Error("No accounts found for invoice: {invoiceId}", invoiceId);
            }

            var model = new TransactionViewModel
            {
                MerchantTrackingNumber = invoiceId.ToString(),
                MerchantTrackingUrl = $"{_slothSettings.MerchantTrackingUrl}/{invoiceId}" //Invoice/Details/ but maybe instead an admin page view of the invoce
            };

            if (invoice.Expenses.Count == 0)
            {
                return Result.Error("No expenses found for invoice: {invoiceId}", invoiceId);
            }

            if (!invoice.Expenses.All(e => e.Total > 0))
            {
                return Result.Error("Expenses found with a Total of 0 or less for invoice: {invoiceId}", invoiceId);
            }

            var grandTotal = Math.Round(invoice.Expenses.Select(a => a.Total).Sum(),2);
            if (invoice.Total != grandTotal)
            {
                Log.Information("Invoice Total {invoiceTotal} != GrandTotal {grandTotal}", invoice.Total, grandTotal);
                invoice.Total = grandTotal;
            }
            foreach (var projectAccount in invoice.Project.Accounts)
            {
                //Debits
                //Validate accounts
                var debit = await _financialService.IsValid(projectAccount.Number);
                if (!debit.IsValid)
                {
                    return Result.Error("Unable to validate debit account {debitAccount}: {debitMessage}",
                        projectAccount.Number, debit.Message);
                }

                var tvm = new TransferViewModel
                {
                    Account = debit.KfsAccount.AccountNumber,
                    Amount = Math.Round(grandTotal * (projectAccount.Percentage / 100), 2),
                    Chart = debit.KfsAccount.ChartOfAccountsCode,
                    SubAccount = debit.KfsAccount.SubAccount,
                    Description = $"Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                    Direction = TransferViewModel.Directions.Debit,
                    ObjectCode = _slothSettings.DebitObjectCode
                };
                if (tvm.Amount >= 0.01m)
                {
                    //Only create this if the amount if 0.01 or greater (sloth requirement)
                    model.Transfers.Add(tvm);
                }
                else
                {
                    Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}", invoice.Id);
                }
            }
            //Go through them all and adjust the last record so the total of them matches the grandtotal (throw an exception if it is zero or negative)
            var debitTotal = model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit).Select(a => a.Amount).Sum();
            if (grandTotal != debitTotal)
            {
                var lastTransfer = model.Transfers.Last(a => a.Direction == TransferViewModel.Directions.Debit);
                lastTransfer.Amount = lastTransfer.Amount + (grandTotal - debitTotal);
                if (lastTransfer.Amount <= 0 || grandTotal != model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit)
                    .Select(a => a.Amount).Sum())
                {
                    return Result.Error("Couldn't get Debits to balance for invoice {invoiceId}", invoice.Id);
                }
            }

            var expenses = invoice.Expenses.GroupBy(a => a.Account);
            foreach (var expense in expenses)
            {
                //Credits
                //Validate Accounts.
                var credit = await _financialService.IsValid(expense.Key);
                if (!credit.IsValid)
                {
                    Log.Warning("Unable to validate credit account {creditAccount}: {creditMessage}", expense.Key, credit.Message);
                }
                var totalCost = Math.Round(expense.Sum(a => a.Total), 2); //Should already be to 2 decimals, but just in case...
                if(totalCost >= 0.01m)
                { 
                    model.Transfers.Add(new TransferViewModel
                    {
                        Account = credit.KfsAccount.AccountNumber,
                        Amount = totalCost,
                        Chart = credit.KfsAccount.ChartOfAccountsCode,
                        SubAccount = credit.KfsAccount.SubAccount,
                        Description = $"Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                        Direction = TransferViewModel.Directions.Credit,
                        ObjectCode = _slothSettings.CreditObjectCode
                    });
                }
                else
                {
                    Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}", invoice.Id);
                }
            }
            var creditTotal = model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Credit).Select(a => a.Amount).Sum();
            if (grandTotal != creditTotal)
            {
                var lastTransfer = model.Transfers.Last(a => a.Direction == TransferViewModel.Directions.Credit);
                lastTransfer.Amount = lastTransfer.Amount + (grandTotal - creditTotal);
                if (lastTransfer.Amount <= 0 || grandTotal != model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Credit)
                    .Select(a => a.Amount).Sum())
                {
                    return Result.Error("Couldn't get Credits to balance for invoice {invoiceId}", invoice.Id);
                }
            }
            
            using var client = new HttpClient { BaseAddress = new Uri(url) };
            client.DefaultRequestHeaders.Add("X-Auth-Token", token);

            Log.Information(JsonSerializer.Serialize(model, _serializerOptions));

            var response = await client.PostAsync("Transactions", new StringContent(JsonSerializer.Serialize(model, _serializerOptions), System.Text.Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Information("Sloth Success Response", content);
                var slothResponse = JsonSerializer.Deserialize<SlothResponseModel>(content, _serializerOptions);

                invoice.Transfers = new List<Transfer>();
                foreach (var transferViewModel in model.Transfers)
                {
                    var extraAccountInfo = string.Empty;
                    if (!string.IsNullOrWhiteSpace(transferViewModel.SubAccount))
                    {
                        extraAccountInfo = $"-{transferViewModel.SubAccount}";
                    }
                    var transfer = new Transfer();
                    transfer.Account = $"{transferViewModel.Chart}-{transferViewModel.Account}{extraAccountInfo}";
                    transfer.Total = transferViewModel.Amount;
                    transfer.Type = transferViewModel.Direction;

                    invoice.Transfers.Add(transfer);
                }

                invoice.KfsTrackingNumber = slothResponse.KfsTrackingNumber;
                invoice.SlothTransactionId = slothResponse.Id;

                invoice.Status = Invoice.Statuses.Pending;

                //Update running total
                invoice.Project.ChargedTotal += invoice.Total;

                await _historyService.MoveMoneyRequested(invoice.ProjectId, invoice);
                await _dbContext.SaveChangesAsync();


                return Result.Value(slothResponse);
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    Log.Information("Sloth Response Not Found for moneyTransfer id {transferId}", invoice.Id);
                    break;
                case HttpStatusCode.NoContent:
                    Log.Information("Sloth Response No Content for moneyTransfer id {transferId}", invoice.Id);
                    break;
                case HttpStatusCode.BadRequest:
                    var badrequest = await response.Content.ReadAsStringAsync();
                    return Result.Error("Sloth Response Bad Request for moneyTransfer id {transferId}: {data}", invoice.Id, badrequest);
            }

            var data = await response.Content.ReadAsStringAsync();
            return Result.Error("Sloth Response didn't have a success code for moneyTransfer id {transferId}: {data}", invoice.Id, data);
        }

        public async Task ProcessTransferUpdates()
        {
            Log.Information("Beginning ProcessTransferUpdates");
            var pendingInvoices = await _dbContext.Invoices.Include(a => a.Project).Where(a => a.Status == Invoice.Statuses.Pending).ToListAsync();
            if (pendingInvoices.Count == 0)
            {
                Log.Information("No pending invoices to process");
                return;
            }

            using var client = new HttpClient { BaseAddress = new Uri($"{_slothSettings.ApiUrl}Transactions/") };
            client.DefaultRequestHeaders.Add("X-Auth-Token", _slothSettings.ApiKey);

            Log.Information("Processing {invoiceCount} transfers", pendingInvoices.Count);
            var updatedCount = 0;
            var rolledBackCount = 0;
            foreach (var invoice in pendingInvoices)
            {
                if (string.IsNullOrWhiteSpace(invoice.SlothTransactionId))
                {
                    Log.Information("Invoice {transferId} missing SlothTransactionId", invoice.Id); //TODO: Log it
                    continue;
                }
                var response = await client.GetAsync(invoice.SlothTransactionId);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.Information("Invoice {transferId} NotFound. SlothTransactionId {transactionId}",
                        invoice.Id, invoice.SlothTransactionId); //TODO: Log it
                    continue;
                }
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    Log.Information("Invoice {transferId} NoContent. SlothTransactionId {transactionId}",
                        invoice.Id, invoice.SlothTransactionId); //TODO: Log it
                    continue;
                }
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var slothResponse = JsonSerializer.Deserialize<SlothResponseModel>(content, _serializerOptions);
                    Log.Information("Invoice {transferId} SlothResponseModel status {status}. SlothTransactionId {transactionId}",
                        invoice.Id, slothResponse.Status, invoice.SlothTransactionId);
                    switch (slothResponse.Status)
                    {
                        case SlothStatuses.Completed:
                            updatedCount++;

                            invoice.Status = Invoice.Statuses.Completed;
                            await _historyService.InvoiceCompleted(invoice.ProjectId, invoice);
                            if (invoice.Project.Status == Project.Statuses.FinalInvoicePending)
                            {
                                invoice.Project.Status = Project.Statuses.Completed;
                                await _historyService.ProjectCompleted(invoice.ProjectId, invoice.Project);
                            }
                            await _dbContext.SaveChangesAsync();
                            break;
                        case SlothStatuses.Cancelled:
                            Log.Information("Invoice {transferId} was cancelled. What do we do?!!!!", invoice.Id);
                            await _historyService.InvoiceCancelled(invoice.ProjectId, invoice);
                            rolledBackCount++;
                            //TODO: Write to the notes field? Trigger off an email?
                            break;
                    }
                }
                else
                {
                    Log.Information("Invoice {transferId} Not Successful. Response code {statusCode}. SlothTransactionId {transactionId}",
                        invoice.Id, response.StatusCode, invoice.SlothTransactionId); //TODO: Log it
                }
            }

            await _dbContext.SaveChangesAsync();
            Log.Information("Updated {updatedCount} orders. Rolled back {rolledBackCount} orders.", updatedCount, rolledBackCount);
        }

        
    }
}
