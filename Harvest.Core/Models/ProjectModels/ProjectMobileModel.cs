using Harvest.Core.Domain;
using System;
using System.Linq.Expressions;

namespace Harvest.Core.Models.ProjectModels
{
    public class ProjectMobileModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string PiName { get; set; }

        public static Expression<Func<Project, ProjectMobileModel>> Projection()
        {
            return a => new ProjectMobileModel
            {
                Id = a.Id,
                Name = a.Name,
                PiName = a.PrincipalInvestigator != null ? a.PrincipalInvestigator.Name : string.Empty,
            };
        }
    }
}
