using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harvest.Core.Domain;
using Harvest.Core.Utilities;
using NetTopologySuite.Geometries;

namespace Harvest.Web.Models
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

        public InvoiceModel() { }

        public InvoiceModel(Invoice invoice)
        {
            Id = invoice.Id;
            Total = invoice.Total;
            CreatedOn = invoice.CreatedOn;
            Notes = invoice.Notes;
            Expenses = (invoice.Expenses?.Select(e => new ExpenseModel(e)) ?? Enumerable.Empty<ExpenseModel>()).ToList();
        }
    }

    public class ExpenseModel
    {
        public int Id { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal Quantity { get; set; }
        public Rate Rate { get; set; }
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
            Rate = expense.Rate;
            RateId = expense.RateId;
            Price = expense.Price;
            Total = expense.Total;
        }
    }
}
