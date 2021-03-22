namespace Harvest.Core.Models.FinancialAccountModels
{
    public class AccountValidationModel
    {
        public bool IsValid { get; set; } = false;
        public string Field { get; set; }
        public string Message { get; set; }

        public KfsAccount KfsAccount { get; set; }
    }
}
