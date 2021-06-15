namespace Harvest.Core.Models
{
    public static class AccessCodes
    {
        public const string SystemAccess = "SystemAccess";
        public const string AdminAccess = "AdminAccess";
        public const string DepartmentAdminAccess = "DepartmentAdminAccess";
        public const string FieldManagerAccess = "FieldManagerAccess";
        public const string SupervisorAccess = "SupervisorAccess";
        public const string WorkerAccess = "WorkerAccess";
        /// <summary>
        /// When using this Authentication, the project id must be in either the projectId or id parameters. It will look for the projectId first.
        /// </summary>
        public const string PrincipalInvestigatorProjectMustBeIdOrProjectId = "PrincipalInvestigator";
    }
}
