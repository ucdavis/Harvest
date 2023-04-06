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
        /// <summary>
        /// When using this Authentication, the projectId must be in the parameters.
        /// </summary>
        public const string PrincipalInvestigator = "PrincipalInvestigator";
        /// <summary>
        /// When using this Authentication, the projectId must be in the parameters.
        /// </summary>
        public const string PrincipalInvestigatorOnly = "PrincipalInvestigatorOnly ";
    }
}
