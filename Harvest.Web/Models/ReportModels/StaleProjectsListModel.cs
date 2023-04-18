using Harvest.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class StaleProjectsListModel
    {
        public List<StaleProjectModel> StaleProjects { get; set; }
        public string TeamName { get; set;}

    }
    public class StaleProjectModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public string Slug { get; set; }
        [Display(Name = "PI Name")]
        public string PrincipalInvestigatorName { get; set; }
        [Display(Name = "PI Email")]
        public string PrincipalInvestigatorEmail { get; set; }
        [Display(Name ="Last Action Date")]
        public DateTime LastStatusUpdatedOn { get; set; }
        [Display(Name = "Days Old")]
        public int DaysOld { get; set; }

        public static Expression<Func<Project, StaleProjectModel>> Projection()
        {
            var now = DateTime.UtcNow;
            return proj => new StaleProjectModel
            {
                Id = proj.Id,
                ProjectName = proj.Name,
                Status = proj.Status,
                Slug = proj.Team.Slug,
                PrincipalInvestigatorName = proj.PrincipalInvestigator.Name,
                PrincipalInvestigatorEmail = proj.PrincipalInvestigator.Email,
                LastStatusUpdatedOn = proj.LastStatusUpdatedOn,
                DaysOld = (int)(now - proj.LastStatusUpdatedOn).TotalDays,
            };
        }
    }
}
