using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Extensions
{
    public static class ModelExtensions
    {
        public static string Summarize(this Invoice invoice, string summaryHeader = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(summaryHeader))
            {
                sb.AppendLine(summaryHeader);
            }

            sb.AppendLine($"Status: {invoice.Status}, Total: {invoice.Total}, Invoice Id: {invoice.Id}, KFS Tracking #: {invoice.KfsTrackingNumber}, Sloth Trx Id: {invoice.SlothTransactionId}");
            
            sb.AppendLine("Transfers:");
            foreach (var transfer in invoice.Transfers)
            {
                sb.AppendLine($"- Account {transfer.Account}, Total {transfer.Total}, Type {transfer.Type}");
            }
            
            sb.AppendLine("Expenses:");
            foreach (var expense in invoice.Expenses)
            {
                sb.AppendLine($"- Account {expense.Account}, Total {expense.Total}, Type {expense.Type}");
            }

            return sb.ToString();
        }

        public static string Summarize(this Expense expense, string summaryHeader)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(summaryHeader))
            {
                sb.AppendLine(summaryHeader);
            }

            sb.AppendLine($"Expense Account {expense.Account}, Total {expense.Total}, Type {expense.Type}");

            return sb.ToString();
        }

    }
}
