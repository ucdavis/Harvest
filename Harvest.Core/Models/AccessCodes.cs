namespace Harvest.Core.Models
{

    public static class AccessCodes
    {
        public const string SystemAccess = "SystemAccess";
        public const string FieldManagerAccess = "FieldManagerAccess";
        public const string SupervisorAccess = "SupervisorAccess";
        public const string WorkerAccess = "WorkerAccess";
        public const string RateAccess = "RateAccess";
        public const string FinanceAccess = "FinanceAccess";
        public const string ReportAccess = "ReportAccess";
        public const string InvoiceAccess = "InvoiceAccess"; //Basically the same as PI, but need to add FinanceRole to just the invoice page.
        public const string ProjectAccess = "ProjectAccess"; //Need to allow finance to see projects in addition to the workers, but not enter expenses.
        public const string ApiKey = "ApiKey"; //Access to the API via API key.
        /// <summary>
        /// When using this Authentication, the projectId must be in the parameters.
        /// </summary>
        public const string PrincipalInvestigator = "PrincipalInvestigator";
        /// <summary>
        /// When using this Authentication, the projectId must be in the parameters.
        /// </summary>
        public const string PrincipalInvestigatorOnly = "PrincipalInvestigatorOnly";
        /// <summary>
        /// When using this Authentication, the projectId must be in the parameters.
        /// </summary>
        public const string PrincipalInvestigatorandFinance = "PrincipalInvestigatorandFinance"; //Allow Finance to override some things if the PI doesn't do them.
    }
}
