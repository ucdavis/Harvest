using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Harvest.Core.Models.SlothModels
{
    public class TransactionViewModel
    {
        public TransactionViewModel()
        {
            TransactionDate = DateTime.UtcNow; //DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            Transfers = new List<TransferViewModel>(2);
            AutoApprove = false; //This should be settable here. It defaults to false in sloth
        }
        public string MerchantTrackingNumber { get; set; }

        public string MerchantTrackingUrl { get; set; }
        public bool AutoApprove { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Source { get; set; } = "Harvest Recharge";
        public string SourceType { get; set; } = "Recharge";

        public IList<TransferViewModel> Transfers { get; set; }
    }

    public class TransferViewModel
    {
        public Decimal Amount { get; set; }
        [StringLength(1)]
        public string Chart { get; set; }
        [StringLength(7)]
        public string Account { get; set; }
        [StringLength(5)]
        public string SubAccount { get; set; }
        [StringLength(4)]
        public string ObjectCode { get; set; }
        [StringLength(40)]
        public string Description { get; set; }
        
        public string Direction { get; set; }// Debit or Credit Code associated with the transaction. = ['Credit', 'Debit'],

        public class Directions
        {
            public const string Debit = "Debit";
            public const string Credit = "Credit";
        }
    }
}
