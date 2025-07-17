using Harvest.Core.Domain;
using System;
using System.Linq.Expressions;

namespace Harvest.Core.Models.ProjectModels
{
    public class ProjectChangeRequestModel
    {
        public int Id { get;set; }
        public string Status { get; set; } 
        public string Name { get; set; }

        public static Expression<Func<Project, ProjectChangeRequestModel>> Projection()
        {
            return a => new ProjectChangeRequestModel
            {
                Id = a.Id,
                Status = a.Status,
                Name = a.Name
            };
        }
    }
}
