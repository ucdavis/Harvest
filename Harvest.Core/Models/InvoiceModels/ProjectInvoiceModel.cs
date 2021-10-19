using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harvest.Core.Domain;
using Harvest.Core.Utilities;
using NetTopologySuite.Geometries;

namespace Harvest.Core.Models.InvoiceModels
{
    public class ProjectInvoiceModel
    {
        public Project Project { get; set; }
        public InvoiceModel Invoice { get; set; }
    }

    public class InvoiceModel
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public List<ExpenseModel> Expenses { get; set; }
        public List<TransferModel> Transfers { get; set; }

        public InvoiceModel() { }

        public InvoiceModel(Invoice invoice)
        {
            Id = invoice.Id;
            Total = invoice.Total;
            CreatedOn = invoice.CreatedOn;
            Notes = invoice.Notes;
            Status = invoice.Status;
            Expenses = (invoice.Expenses?.Select(e => new ExpenseModel(e)) ?? Enumerable.Empty<ExpenseModel>()).ToList();
            Transfers = (invoice.Transfers?.Select(a => new TransferModel(a)) ?? Enumerable.Empty<TransferModel>()).ToList().GroupBy(a => a.Account).Select(a => new TransferModel
            {
                Account = a.Key,
                Total = a.Sum(s => s.Total),
                IsProjectAccount = a.First().IsProjectAccount,
                Id = a.First().Id,
                Type = a.First().Type
            }).ToList();

        }
    }

    public class TransferModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Account { get; set; }
        public decimal Total { get; set; }
        public bool IsProjectAccount { get; set; }

        public TransferModel()
        {
        }


        public TransferModel(Transfer transfer)
        {
            Id               = transfer.Id;
            Type             = transfer.Type;
            Account          = transfer.Account;
            Total            = transfer.Type == Transfer.Types.Credit ? transfer.Total * -1 : transfer.Total;
            IsProjectAccount = transfer.IsProjectAccount;
        }
    }

    public class ExpenseModel
    {
        public int Id { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public RateModel Rate { get; set; }
        public int RateId { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        public ExpenseModel() { }

        public ExpenseModel(Expense expense)
        {
            Id = expense.Id;
            Activity = expense.Activity;
            Description = expense.Description;
            Type = expense.Type;
            Quantity = expense.Quantity;
            Rate = expense.Rate == null ? null : new RateModel(expense.Rate);
            RateId = expense.RateId;
            Price = expense.Price;
            Total = expense.Total;
        }
    }

    public class RateModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public RateModel() { }

        public RateModel(Rate rate)
        {
            Id = rate.Id;
            Price = rate.Price;
            Unit = rate.Unit;
            Type = rate.Type;
            Description = rate.Description;
        }
    }
}
