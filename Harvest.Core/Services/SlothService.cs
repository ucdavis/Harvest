using AggieEnterpriseApi.Validation;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Models.Settings;
using Harvest.Core.Models.SlothModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _clientFactory;
        private IAggieEnterpriseService _aggieEnterpriseService;
        private readonly AggieEnterpriseOptions _aeSettings;

        public SlothService(AppDbContext dbContext, IOptions<SlothSettings> slothSettings, IFinancialService financialService,
            JsonSerializerOptions serializerOptions, IProjectHistoryService historyService, IEmailService emailService, IHttpClientFactory clientFactory, IOptions<AggieEnterpriseOptions> aeSettings, IAggieEnterpriseService aggieEnterpriseService)
        {
            _dbContext = dbContext;
            _slothSettings = slothSettings.Value;
            _financialService = financialService;
            _serializerOptions = serializerOptions;
            _historyService = historyService;
            _emailService = emailService;
            _clientFactory = clientFactory;
            _aggieEnterpriseService = aggieEnterpriseService;
            _aeSettings = aeSettings.Value;
        }


        public async Task<Result<SlothResponseModel>> MoveMoney(int invoiceId)
        {
            var invoice = await _dbContext.Invoices.Where(a => a.Id == invoiceId && a.Status == Invoice.Statuses.Created).Include(a => a.Expenses)
                .Include(a => a.Project).Include(a => a.Project.Accounts).Include(a => a.Project.Team.TeamDetail).SingleOrDefaultAsync();
            if (invoice == null)
            {
                return Result.Error("Invoice not found: {invoiceId}", invoiceId);
            }

            var token = invoice.Project.Team.TeamDetail.SlothApiKey;
            var sourceName = invoice.Project.Team.TeamDetail.SlothSource;

            var url = _slothSettings.ApiUrl;
            if (_aeSettings.UseCoA)
            {
                url = $"{url}v2/";
            }
            else
            {
                url = $"{url}v1/";
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                Log.Error("Sloth Token missing");
            }


            if (invoice == null)
            {
                return Result.Error("Invoice not found: {invoiceId}", invoiceId);
            }

            if (invoice.Project.Accounts.Count == 0)
            {
                return Result.Error("No accounts found for invoice: {invoiceId}", invoiceId);
            }

            if (_aeSettings.UseCoA)
            {
                //Do an early check to make sure project account(s) are valid
                foreach (var account in invoice.Project.Accounts)
                {
                    if (FinancialChartValidation.GetFinancialChartStringType(account.Number) == FinancialChartStringType.Invalid)
                    {
                        //Ok, it is an invalid COA format, so 99.99% chance this is still a KFS account. Lets try and convert it:
                        var coa = await _aggieEnterpriseService.ConvertKfsAccount(account.Number);
                        if (coa != null)
                        {
                            Log.Warning("Project Account updated to COA, using KFS Convert. Project: {id}, KFS Account: {kfs}, COA: {coa}", account.ProjectId, account.Number, coa);
                            account.Number = coa; // Assign it here so we can follow through with the validation. Will this get updated in the DB if everything else goes though I think.

                        }
                    }

                    var accountValidation = await _aggieEnterpriseService.IsAccountValid(account.Number);
                    if (!accountValidation.IsValid)
                    {
                        await _emailService.InvoiceError(invoice);
                        return Result.Error("Account {account} is not a valid Aggie Enterprise CoA", account.Number);
                    }
                }
            }

            if (invoice.Expenses.Count == 0)
            {
                return Result.Error("No expenses found for invoice: {invoiceId}", invoiceId);
            }

            var model = new TransactionViewModel
            {
                MerchantTrackingNumber = invoiceId.ToString(),
                MerchantTrackingUrl = $"{_slothSettings.MerchantTrackingUrl}/{invoice.Project.Team.Slug}/Invoice/Details/{invoice.ProjectId}/{invoiceId}", //Invoice/Details/ but maybe instead an admin page view of the invoice
                Description = $"Proj: {invoice.Project.Name} Inv: {invoiceId}",
                Source = sourceName,
                //TODO: Use the meta Data?
                //Maybe the PI? If so, need to include it.
            };
            model.AddMetadata("Team", invoice.Project.Team.Name);
            model.AddMetadata("Project", invoice.Project.Name);


            var grandTotal = Math.Round(invoice.Expenses.Select(a => a.Total).Sum(), 2);
            if (invoice.Total != grandTotal)
            {
                Log.Information("Invoice Total {invoiceTotal} != GrandTotal {grandTotal}", invoice.Total, grandTotal);
                invoice.Total = grandTotal;
            }
            var absGrandTotal = Math.Round(invoice.Expenses.Select(a => Math.Abs(a.Total)).Sum(), 2);


            if (!(ProcessRefunds(model, invoice)).Value)
            {
                return Result.Error("ProcessRefunds Failed");
            }

            if (!(await ProcessDebits(model, absGrandTotal, invoice)).Value)
            {
                return Result.Error("ProcessDebits Failed");
            }

            if (!(await ProcessCredits(model, absGrandTotal, invoice)).Value)
            {
                return Result.Error("ProcessCredits Failed");
            }

            if (model.Transfers.Count == 0)
            {
                return Result.Error("No Transfers Generated for invoice: {id}", invoice.Id);
            }


            using var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("X-Auth-Token", token);

            Log.Information(JsonSerializer.Serialize(model, _serializerOptions));

            var response = await client.PostAsync("Transactions", new StringContent(JsonSerializer.Serialize(model, _serializerOptions), System.Text.Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Information("Sloth Success Response", content);
                var slothResponse = JsonSerializer.Deserialize<SlothResponseModel>(content, _serializerOptions);

                invoice.Transfers = new List<Transfer>();
                foreach (var transferViewModel in model.Transfers)
                {
                    var transfer = new Transfer();
                    if (_aeSettings.UseCoA)
                    {
                        transfer.Account = transferViewModel.FinancialSegmentString;
                        transfer.IsProjectAccount =
                            invoice.Project.Accounts.Select(a => a.Number).Contains(_aggieEnterpriseService.ReplaceNaturalAccount(transfer.Account, _aeSettings.PassthroughCoaNaturalAccount, _aeSettings.PpmSpecialNaturalAccounts))
                            ||
                            invoice.Project.Accounts.Select(a => a.Number).Contains(_aggieEnterpriseService.ReplaceNaturalAccount(transfer.Account, _aeSettings.NormalCoaNaturalAccount, _aeSettings.PpmSpecialNaturalAccounts));
                    }
                    else
                    {
                        var account = new KfsAccount()
                        {
                            ChartOfAccountsCode = transferViewModel.Chart,
                            AccountNumber = transferViewModel.Account,
                            SubAccount = transferViewModel.SubAccount
                        }; //Don't put in object code. Project accounts don't have them, they are on the rate accounts

                        transfer.Account = account.ToString();
                        transfer.IsProjectAccount = invoice.Project.Accounts.Select(a => a.Number).Contains(transfer.Account);
                    }

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

        private Result<bool> ProcessRefunds(TransactionViewModel model, Invoice invoice)
        {
            var refundAmount = invoice.Expenses.Where(a => a.Total < 0).Sum(a => a.Total);
            refundAmount = Math.Abs(refundAmount);
            if (refundAmount <= 0)
            {
                //No refunds
                return Result.Value(true);
            }

            if (_aeSettings.UseCoA)
            {
                //Credits (Previously debits)
                var debExpenseGroups = invoice.Expenses.Where(a => a.Total < 0)
                    .GroupBy(a => a.IsPassthrough) //Just group by is passthrough
                    .Select(a => new { isPassthrough = a.Key, total = a.Sum(s => Math.Abs(s.Total)) })
                    .ToArray(); //This should only have 1 value

                var localTransfers = new List<TransferViewModel>();
                foreach (var debExpenseGroup in debExpenseGroups) //At most, this will contain 2 items
                {
                    foreach (var projectAccount in invoice.Project.Accounts)
                    {
                        //Replace the natural Account/Expenditure type with 
                        var tvm = new TransferViewModel
                        {
                            FinancialSegmentString = _aggieEnterpriseService.ReplaceNaturalAccount(projectAccount.Number, debExpenseGroup.isPassthrough ? _aeSettings.PassthroughCoaNaturalAccount : _aeSettings.NormalCoaNaturalAccount, _aeSettings.PpmSpecialNaturalAccounts),
                            Amount = Math.Round(debExpenseGroup.total * (projectAccount.Percentage / 100), 2),
                            Description = $"Rev Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                            Direction = TransferViewModel.Directions.Credit,
                        };
                        if (tvm.Amount >= 0.01m)
                        {
                            //Only create this if the amount if 0.01 or greater (sloth requirement)
                            model.Transfers.Add(tvm);
                            localTransfers.Add(tvm);
                        }
                        else
                        {
                            Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}",
                                invoice.Id);
                        }
                    }
                }

                if (localTransfers.Sum(a => a.Amount) != refundAmount)
                {
                    Log.Information("Refund debit total didn't match for invoice {id}", invoice.Id);
                }

                //Debit (Previously Credit)
                var expenseGroups = invoice.Expenses.Where(a => a.Total < 0).GroupBy(a => new { a.Account });
                foreach (var expenseGroup in expenseGroups)
                {
                    var amount = Math.Round(expenseGroup.Sum(a => a.Total), 2);
                    amount = Math.Abs(amount);
                    var tvm = new TransferViewModel
                    {
                        FinancialSegmentString = expenseGroup.Key.Account,
                        Amount = amount,
                        Description = $"Rev Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                        Direction = TransferViewModel.Directions.Debit,
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
            }
            //KFS Way
            else
            {
                //Credits (Previously debits)
                var debExpenseGroups = invoice.Expenses.Where(a => a.Total < 0)
                    .GroupBy(a => _financialService.Parse(a.Account).ObjectCode)
                    .Select(a => new { objectCode = a.Key, total = a.Sum(s => Math.Abs(s.Total)) })
                    .ToArray(); //This should only have 1 value
                var localTransfers = new List<TransferViewModel>();
                foreach (var debExpenseGroup in debExpenseGroups)
                {
                    foreach (var projectAccount in invoice.Project.Accounts)
                    {
                        var account = _financialService.Parse(projectAccount.Number);

                        var tvm = new TransferViewModel
                        {
                            Account = account.AccountNumber,
                            Amount = Math.Round(debExpenseGroup.total * (projectAccount.Percentage / 100), 2),
                            Chart = account.ChartOfAccountsCode,
                            SubAccount = account.SubAccount,
                            Description = $"Rev Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                            Direction = TransferViewModel.Directions.Credit,
                            ObjectCode = debExpenseGroup.objectCode,
                        };
                        if (tvm.Amount >= 0.01m)
                        {
                            //Only create this if the amount if 0.01 or greater (sloth requirement)
                            model.Transfers.Add(tvm);
                            localTransfers.Add(tvm);
                        }
                        else
                        {
                            Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}",
                                invoice.Id);
                        }
                    }
                }

                if (localTransfers.Sum(a => a.Amount) != refundAmount)
                {
                    Log.Information("Refund debit total didn't match for invoice {id}", invoice.Id);
                }

                //Debit (Previously Credit)
                var expenseGroups = invoice.Expenses.Where(a => a.Total < 0).GroupBy(a => new { a.Account });
                foreach (var expenseGroup in expenseGroups) //Probably only 1, but just in case
                {
                    var account = _financialService.Parse(expenseGroup.Key.Account);
                    var amount = Math.Round(expenseGroup.Sum(a => a.Total), 2);
                    amount = Math.Abs(amount);
                    var tvm = new TransferViewModel
                    {
                        Account = account.AccountNumber,
                        Amount = amount,
                        Chart = account.ChartOfAccountsCode,
                        SubAccount = account.SubAccount,
                        Description = $"Rev Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                        Direction = TransferViewModel.Directions.Debit,
                        ObjectCode = _slothSettings.CreditObjectCode,
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
            }


            //Ok, so this should happen before anything else...
            var debAmount = model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit).Sum(a => a.Amount);
            var credAmount = model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Credit).Sum(a => a.Amount);
            if (debAmount != credAmount)
            {
                Log.Information("Refunds don't balance invoice {id} Deb {deb} Cred {crd}", invoice.Id, debAmount, credAmount);
                var lastCredTmv = model.Transfers.Last(a => a.Direction == TransferViewModel.Directions.Credit);
                lastCredTmv.Amount = lastCredTmv.Amount + (debAmount - credAmount);
            }
            debAmount = model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit).Sum(a => a.Amount);
            credAmount = model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Credit).Sum(a => a.Amount);
            if (debAmount != credAmount)
            {
                return Result.Error("Couldn't get refund Credits to balance for invoice {invoiceId}", invoice.Id);
            }

            return Result.Value(true);
        }

        private async Task<Result<bool>> ProcessDebits(TransactionViewModel model, decimal grandTotal, Invoice invoice)
        {
            if (_aeSettings.UseCoA)
            {
                //Passthough flag tells us what to change the natural account to
                var expenseGroups = invoice.Expenses.Where(a => a.Total > 0)
                    .GroupBy(a => a.IsPassthrough)
                    .Select(a => new { isPassthrough = a.Key, total = a.Sum(s => s.Total) })
                    .ToArray();

                //Validate accounts. Do it here so we don't call this every time we go through the expense loop.
                var validatedProjectAccounts = new Dictionary<Account, AccountValidationModel>();
                foreach (var projectAccount in invoice.Project.Accounts)
                {
                    //This should actually be validated earlier up now.... but still need it to populate the account values....
                    var account = await _aggieEnterpriseService.IsAccountValid(projectAccount.Number);
                    if (!account.IsValid)
                    {
                        await _emailService.InvoiceError(invoice);

                        return Result.Error("Unable to validate debit account {debitAccount}: {debitMessage}", projectAccount.Number, account.Message);
                    }
                    validatedProjectAccounts.Add(projectAccount, account);
                }


                //Go through all the grouped expenses (by the expense account)
                foreach (var expenseGroup in expenseGroups)
                {
                    var localTransfers = new List<TransferViewModel>();
                    foreach (var projectAccount in validatedProjectAccounts)
                    {
                        //Debits
                        var debit = projectAccount.Value;
                        var tvm = new TransferViewModel
                        {
                            FinancialSegmentString = _aggieEnterpriseService.ReplaceNaturalAccount(debit.FinancialSegmentString, expenseGroup.isPassthrough ? _aeSettings.PassthroughCoaNaturalAccount : _aeSettings.NormalCoaNaturalAccount, _aeSettings.PpmSpecialNaturalAccounts),
                            Amount = Math.Round(expenseGroup.total * (projectAccount.Key.Percentage / 100), 2),
                            Description = $"Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                            Direction = TransferViewModel.Directions.Debit,
                        };
                        if (tvm.Amount >= 0.01m)
                        {
                            //Only create this if the amount if 0.01 or greater (sloth requirement)
                            //model.Transfers.Add(tvm); //Don't add this here, add them after the adjustment of the localTransfers
                            localTransfers.Add(tvm);
                        }
                        else
                        {
                            Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}", invoice.Id);
                        }
                    }


                    //Go through them all and adjust the last record so the total of them matches the grandtotal (throw an exception if it is zero or negative)
                    var debitTotal = localTransfers
                        .Select(a => a.Amount).Sum();
                    if (expenseGroup.total != debitTotal)
                    {
                        var lastAmount = localTransfers.Last().Amount;
                        Log.Information("Debit Total doesn't match. Attempting to fix. ExpenseTotal {expenseTotal} DebitTotal {debitTotal}", expenseGroup.total, debitTotal);
                        localTransfers.Last().Amount = lastAmount + (expenseGroup.total - debitTotal);
                        if (localTransfers.Last().Amount <= 0 || expenseGroup.total != localTransfers
                            .Select(a => a.Amount).Sum())
                        {
                            return Result.Error("Couldn't get Debits to balance for invoice {invoiceId}", invoice.Id);
                        }
                        Log.Information("Adjusted debit expense amount to get everything to balance. {exTotal} {dbTotal}", expenseGroup.total, debitTotal);
                    }
                    //Now add them to the model (This is a new way for COA and cleaner) 
                    foreach (var transfer in localTransfers)
                    {
                        model.Transfers.Add(transfer);
                    }
                }
            }
            // KFS Way
            /*
             * Writing Debits: Taking money from the PI's account
             * Takes all the expenses and groups them by the object code of the expense account and sums up the total
             * For each of these groups, it debits the PI's accounts splitting the sum into the PI's accounts based on the percentage of the account
             * The object code is replaced with the expense group's object code
             * We skip adding any amount that is less than 0.01 because that is the minimum amount that can be transferred
             * We total up all the debits for this group and if it doesn't match the total for the group, we try to adjust the last debit to make it match
            */
            else
            {
                //Don't need to group by IsPassthrough here because the account will have the expenseObject code in it.
                var expenseGroups = invoice.Expenses.Where(a => a.Total > 0)
                    .GroupBy(a => _financialService.Parse(a.Account).ObjectCode)
                    .Select(a => new { objectCode = a.Key, total = a.Sum(s => s.Total) })
                    .ToArray();

                //Validate accounts. Do it here so we don't call this every time we go through the expense loop.
                var validatedProjectAccounts = new Dictionary<Account, AccountValidationModel>();
                foreach (var projectAccount in invoice.Project.Accounts)
                {
                    var account = await _financialService.IsValid(projectAccount.Number);
                    if (!account.IsValid)
                    {
                        await _emailService.InvoiceError(invoice);

                        return Result.Error("Unable to validate debit account {debitAccount}: {debitMessage}", projectAccount.Number, account.Message);
                    }
                    validatedProjectAccounts.Add(projectAccount, account);
                }

                //Go through all the grouped expenses (by the expense account)
                foreach (var expenseGroup in expenseGroups)
                {
                    var localTransfers = new List<TransferViewModel>();
                    foreach (var projectAccount in validatedProjectAccounts)
                    {
                        //Debits
                        var debit = projectAccount.Value;
                        var tvm = new TransferViewModel
                        {
                            Account = debit.KfsAccount.AccountNumber,
                            Amount = Math.Round(expenseGroup.total * (projectAccount.Key.Percentage / 100), 2),
                            Chart = debit.KfsAccount.ChartOfAccountsCode,
                            SubAccount = debit.KfsAccount.SubAccount,
                            Description = $"Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                            Direction = TransferViewModel.Directions.Debit,
                            ObjectCode = expenseGroup.objectCode,
                        };
                        if (tvm.Amount >= 0.01m)
                        {
                            //Only create this if the amount if 0.01 or greater (sloth requirement)
                            model.Transfers.Add(tvm);
                            localTransfers.Add(tvm);
                        }
                        else
                        {
                            Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}", invoice.Id);
                        }
                    }

                    //Go through them all and adjust the last record so the total of them matches the grandtotal (throw an exception if it is zero or negative)
                    var debitTotal = localTransfers
                        .Where(a => a.Direction == TransferViewModel.Directions.Debit && a.ObjectCode == expenseGroup.objectCode)
                        .Select(a => a.Amount).Sum();
                    if (expenseGroup.total != debitTotal)
                    {
                        var lastAmount = localTransfers.Last().Amount;
                        Log.Information("Debit Total doesn't match. Attempting to fix. ExpenseTotal {expenseTotal} DebitTotal {debitTotal}", expenseGroup.total, debitTotal);
                        var lastTransfer = model.Transfers.Last(a => a.Direction == TransferViewModel.Directions.Debit && a.ObjectCode == expenseGroup.objectCode && a.Amount == lastAmount);
                        lastTransfer.Amount = lastTransfer.Amount + (expenseGroup.total - debitTotal);
                        if (lastTransfer.Amount <= 0 || expenseGroup.total != model.Transfers
                            .Where(a => a.Direction == TransferViewModel.Directions.Debit && a.ObjectCode == expenseGroup.objectCode)
                            .Select(a => a.Amount).Sum())
                        {
                            return Result.Error("Couldn't get Debits to balance for invoice {invoiceId} objectCode {objectCode}", invoice.Id, expenseGroup.objectCode);
                        }
                        Log.Information("Adjusted debit expense amount to get everything to balance. {exTotal} {dbTotal}", expenseGroup.total, debitTotal);
                    }
                }
            }

            var debitGrandTotal = model.Transfers.Where(a => a.Direction == TransferViewModel.Directions.Debit).Select(a => a.Amount).Sum();
            if (debitGrandTotal != grandTotal)
            {
                //This shouldn't happen, but maybe if there was a 1 cent last transaction... Extra check doesn't hurt.
                return Result.Error("Couldn't get Debits Total to balance with the Grand Total for invoice {invoiceId}", invoice.Id);
            }


            return Result.Value(true);
        }


        private async Task<Result<bool>> ProcessCredits(TransactionViewModel model, decimal grandTotal, Invoice invoice)
        {
            /*
             * We just need to group by the COA as this is a complete account and we will not be replacing anything here.
             */
            if (_aeSettings.UseCoA)
            {
                //For the Credits, we need to group by account 
                var expenseGroups = invoice.Expenses.Where(a => a.Total > 0).GroupBy(a => new { a.Account });
                foreach (var expenseGroup in expenseGroups)
                {
                    //Credits
                    //Validate Accounts.
                    var credit = await _aggieEnterpriseService.IsAccountValid(expenseGroup.Key.Account);
                    if (!credit.IsValid)
                    {
                        //Maybe this should prevent it?
                        Log.Warning("Unable to validate credit account {creditAccount}: {creditMessage}", expenseGroup.Key.Account, credit.Message);
                    }
                    var totalCost = Math.Round(expenseGroup.Sum(a => a.Total), 2); //Should already be to 2 decimals, but just in case...
                    if (totalCost >= 0.01m)
                    {
                        //We only replace the natural account on the PI's account. Not expenses
                        model.Transfers.Add(new TransferViewModel
                        {
                            FinancialSegmentString = credit.FinancialSegmentString,
                            Amount = totalCost,
                            Description = $"Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                            Direction = TransferViewModel.Directions.Credit,
                        });
                    }
                    else
                    {
                        Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}", invoice.Id);
                    }
                }
            }
            // KFS Way
            /*
             * Writing Credits: Putting money into the expense accounts
             * We group the expenses by the KFS account and if it is a passthrough or not
             * The object code is changed based on the passthrough flag
             */
            else
            {
                //For the Credits, we need to group by account and IsPassthrough... 
                var expenseGroups = invoice.Expenses.Where(a => a.Total > 0).GroupBy(a => new { a.Account, a.IsPassthrough });
                foreach (var expenseGroup in expenseGroups)
                {
                    //Credits
                    //Validate Accounts.
                    var credit = await _financialService.IsValid(expenseGroup.Key.Account);
                    if (!credit.IsValid)
                    {
                        //Maybe this should prevent it?
                        Log.Warning("Unable to validate credit account {creditAccount}: {creditMessage}", expenseGroup.Key.Account, credit.Message);
                    }
                    var totalCost = Math.Round(expenseGroup.Sum(a => a.Total), 2); //Should already be to 2 decimals, but just in case...
                    if (totalCost >= 0.01m)
                    {
                        model.Transfers.Add(new TransferViewModel
                        {
                            Account = credit.KfsAccount.AccountNumber,
                            Amount = totalCost,
                            Chart = credit.KfsAccount.ChartOfAccountsCode,
                            SubAccount = credit.KfsAccount.SubAccount,
                            Description = $"Proj: {invoice.Project.Name}".TruncateAndAppend($" Inv: {invoice.Id}", 40),
                            Direction = TransferViewModel.Directions.Credit,
                            ObjectCode = expenseGroup.Key.IsPassthrough ? _slothSettings.CreditPassthroughObjectCode : _slothSettings.CreditObjectCode,
                        });
                    }
                    else
                    {
                        Log.Information("Amount of zero detected. Skipping sloth transfer. Invoice {invoiceId}", invoice.Id);
                    }
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


            return Result.Value(true);
        }

        public async Task ProcessTransferUpdates()
        {
            var url = _slothSettings.ApiUrl;
            if (_aeSettings.UseCoA)
            {
                url = $"{url}v2/";
            }
            else
            {
                url = $"{url}v1/";
            }

            Log.Information("Beginning ProcessTransferUpdates");
            var pendingInvoices = await _dbContext.Invoices.Include(a => a.Project).Include(a => a.Project.Team.TeamDetail)                
                .Where(a => a.Status == Invoice.Statuses.Pending).ToListAsync();
            if (pendingInvoices.Count == 0)
            {
                Log.Information("No pending invoices to process");
                return;
            }

            var invoiceGroup = pendingInvoices.GroupBy(a => a.Project.Team);
            foreach (var invoices in invoiceGroup)
            {
                Log.Information("Processing {count} pending invoices for team {slug}", invoices.Count(), invoices.Key.Slug);
                using var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri($"{url}Transactions/");
                client.DefaultRequestHeaders.Add("X-Auth-Token", invoices.Key.TeamDetail.SlothApiKey);
                var updatedCount = 0;
                var rolledBackCount = 0;
                var pendingApprovalCount = 0;

                foreach (var invoice in invoices)
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
                                    invoice.Project.UpdateStatus(Project.Statuses.Completed);
                                    await _historyService.ProjectCompleted(invoice.ProjectId, invoice.Project);
                                }
                                await _dbContext.SaveChangesAsync();

                                await _emailService.InvoiceDone(invoice, invoice.Status);

                                break;
                            case SlothStatuses.Cancelled:
                                Log.Information("Invoice {transferId} was cancelled. What do we do?!!!!", invoice.Id);
                                await _historyService.InvoiceCancelled(invoice.ProjectId, invoice);

                                //await _emailService.InvoiceDone(invoice, SlothStatuses.Cancelled); //Email the PI that it was canceled? 
                                //Probably what we want to do is to set the expense invoiceIds to null, then delete the invoice
                                //Then send a notification to the FM and maybe us?

                                rolledBackCount++;
                                //TODO: Write to the notes field? Trigger off an email?
                                break;

                                //TODO: Check for rejected and email Brian? Us?
                            case SlothStatuses.PendingApproval:
                                pendingApprovalCount++;
                                break;
                        }
                    }
                    else
                    {
                        Log.Information("Invoice {transferId} Not Successful. Response code {statusCode}. SlothTransactionId {transactionId}",
                            invoice.Id, response.StatusCode, invoice.SlothTransactionId); //TODO: Log it
                    }
                }
                Log.Information("Pending Approval Count for team {slug}:{count}", invoices.Key.Slug, pendingApprovalCount);
                await _dbContext.SaveChangesAsync();
                Log.Information("Updated {updatedCount} orders. Rolled back {rolledBackCount} orders. for team {slug}", updatedCount, rolledBackCount, invoices.Key.Slug);

            }




        }


    }
}
