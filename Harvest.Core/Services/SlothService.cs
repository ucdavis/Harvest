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
using Harvest.Core.Models.Settings;
using Harvest.Core.Models.SlothModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace Harvest.Core.Services
{
    public interface ISlothService
    {
        Task<SlothResponseModel> MoveMoney(int invoiceId);

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


        public async Task<SlothResponseModel> MoveMoney(int invoiceId)
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
                Log.Error("Invoice not found: {invoiceId}", invoiceId);
                return null;
            }

            var model = new TransactionViewModel
            {
                MerchantTrackingNumber = invoiceId.ToString(),
                MerchantTrackingUrl = $"{_slothSettings.MerchantTrackingUrl}/{invoiceId}" //Invoice/Details/ but maybe instead an admin page view of the invoce
            };

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
                   Log.Information("Invalid Project Account: {debitMessage}", debit.Message);
                   throw new Exception($"Unable to validate debit account {projectAccount.Number}: {debit.Message}");
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
                    throw new Exception($"Couldn't get Debits to balance for invoice {invoice.Id}");
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
                    Log.Information("Invalid Expense Account: {creditMessage}", credit.Message);
                    throw new Exception($"Unable to validate credit account {expense.Key}: {credit.Message}");
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
                    throw new Exception($"Couldn't get Credits to balance for invoice {invoice.Id}");
                }
            }
            
            using var client = new HttpClient { BaseAddress = new Uri(url) };
            client.DefaultRequestHeaders.Add("X-Auth-Token", token);

            Log.Information(JsonSerializer.Serialize(model, _serializerOptions));

            var response = await client.PostAsync("Transactions", new StringContent(JsonSerializer.Serialize(model, _serializerOptions), System.Text.Encoding.UTF8, "application/json"));
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    Log.Information("Sloth Response Not Found for moneyTransfer id {moneyTransferId}", invoice.Id);
                    break;
                case HttpStatusCode.NoContent:
                    Log.Information("Sloth Response No Content for moneyTransfer id {moneyTransferId}", invoice.Id);
                    break;
                case HttpStatusCode.BadRequest:
                    Log.Error("Sloth Response Bad Request for moneyTransfer {id}", invoice.Id);
                    var badrequest = await response.Content.ReadAsStringAsync();
                    Log.ForContext("data", badrequest, true).Information("Sloth message response");
                    var badRtValue = new SlothResponseModel
                    {
                        Success = false,
                        Message = badrequest
                    };

                    return badRtValue;
            }

            //TODO: Capture errors?

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
                    transfer.Account = $"{transferViewModel.Chart}-{transferViewModel.Account}{{extraAccountInfo}}";
                    transfer.Total = transferViewModel.Amount;
                    transfer.Type = transferViewModel.Direction;

                    invoice.Transfers.Add(transfer);
                }

                invoice.KfsTrackingNumber = slothResponse.KfsTrackingNumber;
                invoice.SlothTransactionId = slothResponse.Id;

                invoice.Status = Invoice.Statuses.Pending;

                await _historyService.AddProjectHistory(invoice.Project, nameof(MoveMoney), "Sloth money movement requested", invoice);
                await _dbContext.SaveChangesAsync();


                return slothResponse;
            }


            Log.Information("Sloth Response didn't have a success code for moneyTransfer {id}", invoice.Id);
            var badContent = await response.Content.ReadAsStringAsync();
            Log.ForContext("data", badContent, true).Information("Sloth message response");
            var rtValue = JsonSerializer.Deserialize<SlothResponseModel>(badContent, _serializerOptions);
            rtValue.Success = false;

            return rtValue;
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
                    if (slothResponse.Status == "Completed")
                    {
                        updatedCount++;
                        //Update Project Running total
                        invoice.Project.ChargedTotal += invoice.Total;

                        invoice.Status = Invoice.Statuses.Completed;
                        await _historyService.AddProjectHistory(invoice.Project, nameof(ProcessTransferUpdates), "Invoice completed", invoice);
                        await _dbContext.SaveChangesAsync();
                    }
                    if (slothResponse.Status == "Cancelled")
                    {

                        Log.Information("Invoice {transferId} was cancelled. What do we do?!!!!", invoice.Id);
                        await _historyService.AddProjectHistory(invoice.Project, nameof(ProcessTransferUpdates), "Invoice cancelled", invoice);
                        rolledBackCount++;
                        //TODO: Write to the notes field? Trigger off an email?
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
            return;
        }

        
    }
}
