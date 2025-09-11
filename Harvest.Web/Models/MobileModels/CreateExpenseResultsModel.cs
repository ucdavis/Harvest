using System;
using System.Collections.Generic;

namespace Harvest.Web.Models.MobileModels
{
    public class CreateExpenseResultsModel
    {
        public List<CreateExpenseResultItem> Results { get; set; } = new List<CreateExpenseResultItem>();

        public CreateExpenseSummaryModel Summary { get; set; } = new CreateExpenseSummaryModel();

    }

    public class CreateExpenseResultItem
    {
        public Guid? WorkerMobileId { get; set; }
        public int? ExpenseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Result { get; set; }
        public CreateExpenseErrors Errors { get; set; }

    }
    public class CreateExpenseSummaryModel
    {
        public int Created { get; set; } = 0;
        public int Duplicate { get; set; } = 0;
        public int Rejected { get; set; } = 0;
    }

    public class CreateExpenseErrors
    {
        public string Field { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
