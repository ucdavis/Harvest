using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.Settings;
using Harvest.Core.Models.SlothModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace Harvest.Core.Services
{
    public interface ISlothService
    {
        Task<SlothResponseModel> MoveMoney(Transfer moneyTransfer);

        Task ProcessTransferUpdates();
    }

    public class SlothService : ISlothService
    {
        private readonly AppDbContext _dbContext;
        private readonly SlothSettings _slothSettings;
        private readonly IFinancialService _financialService;

        public SlothService(AppDbContext dbContext, IOptions<SlothSettings> slothSettings, IFinancialService financialService)
        {
            _dbContext = dbContext;
            _slothSettings = slothSettings.Value;
            _financialService = financialService;
        }



        //TODO: Add validation?
        public async Task<SlothResponseModel> MoveMoney(Transfer moneyTransfer)
        {
            var token = _slothSettings.ApiKey;
            var url = _slothSettings.ApiUrl;

            if (string.IsNullOrWhiteSpace(token))
            {
                Log.Error("Sloth Token missing");
            }

            var debit = await _financialService.IsValid(moneyTransfer.FromAccount.Number);
            if (!debit.IsValid)
            {
                throw new Exception($"Unable to validate debit account {moneyTransfer.FromAccount.Number}: {debit.Message}");
            }

            var credit = await _financialService.IsValid(moneyTransfer.ToAccount.Number);
            if (!credit.IsValid)
            {
                throw new Exception($"Unable to validate credit account {moneyTransfer.ToAccount.Number}: {credit.Message}");
            }

            var model = new TransactionViewModel
            {
                MerchantTrackingNumber = moneyTransfer.Id.ToString(),
                MerchantTrackingUrl = $"{_slothSettings.MerchantTrackingUrl}/{moneyTransfer.Id}"
            };

            model.Transfers.Add(new TransferViewModel
            {
                Account = debit.KfsAccount.AccountNumber,
                Amount = moneyTransfer.Amount,
                Chart = debit.KfsAccount.ChartOfAccountsCode, 
                SubAccount = debit.KfsAccount.SubAccount,
                Description = moneyTransfer.Description, 
                Direction = TransferViewModel.Directions.Debit,
                ObjectCode = _slothSettings.DebitObjectCode
            });

            model.Transfers.Add(new TransferViewModel
            {
                Account = credit.KfsAccount.AccountNumber,
                Amount = moneyTransfer.Amount,
                Chart = credit.KfsAccount.ChartOfAccountsCode,
                SubAccount = credit.KfsAccount.SubAccount,
                Description = moneyTransfer.Description,
                Direction = TransferViewModel.Directions.Credit,
                ObjectCode = _slothSettings.CreditObjectCode
            });

            using var client = new HttpClient {BaseAddress = new Uri(url)};
            client.DefaultRequestHeaders.Add("X-Auth-Token", token);

            Log.Information(JsonConvert.SerializeObject(model));

            var response = await client.PostAsync("Transactions", new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    Log.Information("Sloth Response Not Found for moneyTransfer id {moneyTransferId}", moneyTransfer.Id);
                    break;
                case HttpStatusCode.NoContent:
                    Log.Information("Sloth Response No Content for moneyTransfer id {moneyTransferId}", moneyTransfer.Id);
                    break;
                case HttpStatusCode.BadRequest:
                    Log.Error("Sloth Response Bad Request for moneyTransfer {id}", moneyTransfer.Id);
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

                return JsonConvert.DeserializeObject<SlothResponseModel>(content);
            }


            Log.Information("Sloth Response didn't have a success code for moneyTransfer {id}", moneyTransfer.Id);
            var badContent = await response.Content.ReadAsStringAsync();                    
            Log.ForContext("data", badContent, true).Information("Sloth message response");
            var rtValue = JsonConvert.DeserializeObject<SlothResponseModel>(badContent);
            rtValue.Success = false;

            return rtValue;
        }

        /// <summary>
        /// This is to see if the money has moved in Sloth. Similar to MoneyHasMoved in Anlab
        /// </summary>
        /// <returns></returns>
        public async Task ProcessTransferUpdates()
        {
            Log.Information("Beginning ProcessTransferUpdates");
            var transferRequests = await _dbContext.TransferRequests.Where(r => r.Status != TransferStatusCodes.Complete).ToListAsync();
            if (transferRequests.Count == 0)
            {
                Log.Information("No account transfers to process");
                return ;
            }

            using var client = new HttpClient {BaseAddress = new Uri($"{_slothSettings.ApiUrl}Transactions/")};
            client.DefaultRequestHeaders.Add("X-Auth-Token", _slothSettings.ApiKey);

            Log.Information("Processing {transferCount} transfers", transferRequests.Count);
            var updatedCount = 0;
            var rolledBackCount = 0;
            foreach (var transfer in transferRequests)
            {
                if (!transfer.SlothTransactionId.HasValue)
                {
                    Log.Information("MoneyTransfer {transferId} missing SlothTransactionId", transfer.Id); //TODO: Log it
                    continue;
                }
                var response = await client.GetAsync(transfer.SlothTransactionId.ToString());
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.Information("MoneyTransfer {transferId} NotFound. SlothTransactionId {transactionId}",
                        transfer.Id, transfer.SlothTransactionId); //TODO: Log it
                    continue;
                }
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    Log.Information("MoneyTransfer {transferId} NoContent. SlothTransactionId {transactionId}", 
                        transfer.Id, transfer.SlothTransactionId); //TODO: Log it
                    continue;
                }
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var slothResponse = JsonConvert.DeserializeObject<SlothResponseModel>(content);
                    Log.Information("MoneyTransfer {transferId} SlothResponseModel status {status}. SlothTransactionId {transactionId}",
                        transfer.Id, slothResponse.Status, transfer.SlothTransactionId);
                    if (slothResponse.Status == "Completed")
                    {
                        updatedCount++;
                        transfer.Status = TransferStatusCodes.Complete;
                        transfer.History.Add(new TransferHistory
                        {
                            Action = "Move UCD Money",
                            Status = transfer.Status,
                            ActorName = "Job",
                            Notes = "Money Moved",
                        });
                    }
                    if (slothResponse.Status == "Cancelled")
                    {
                        transfer.History.Add(new TransferHistory
                        {
                            Action = "Move UCD Money",
                            Status = transfer.Status,
                            ActorName = "Job",
                            Notes = "Money Movement Cancelled.",
                        });
                        Log.Information("Order {transferId} was cancelled. Setting back to unpaid", transfer.Id);
                        rolledBackCount++;
                        //TODO: Write to the notes field? Trigger off an email?
                    }                        
                }
                else
                {
                    Log.Information("Order {transferId} Not Successful. Response code {statusCode}. SlothTransactionId {transactionId}", 
                        transfer.Id, response.StatusCode, transfer.SlothTransactionId); //TODO: Log it
                }
            }

            await _dbContext.SaveChangesAsync();
            Log.Information("Updated {updatedCount} orders. Rolled back {rolledBackCount} orders.", updatedCount, rolledBackCount);
            return;
        }
    }
}
