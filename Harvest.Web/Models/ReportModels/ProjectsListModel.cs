using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class ProjectsListModel
    {
        [DataType(DataType.Date)]       
        public DateTime? Start { get; set; }
        [DataType(DataType.Date)]
        public DateTime? End { get; set; }

        public List<ProjectInvoiceSummaryModel> ProjectInvoiceSummaries { get; set; }

        public string TeamName { get; set; }
    }
    
    public class ProjectInvoiceSummaryModel
    {
        [Display(Name = "Id")]
        public int ProjectId { get; set; }
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
        public string Status { get; set; }
        [Display(Name = "Crop Type")]
        public string CropType { get; set; }
        public string Crop { get; set; }
        public double Acres { get; set; }
        [DataType(DataType.Date)]
        public DateTime Start { get; set; }
        [DataType(DataType.Date)]
        public DateTime End { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Created")]
        public DateTime CreatedOn { get; set; }
        [Display(Name = "PI Name")]
        public string PrincipalInvestigatorName { get; set; }
        [Display(Name = "PI Email")]
        public string PrincipalInvestigatorEmail { get; set; }

        [Display(Name = "Invoice Total")]
        public decimal InvoiceTotal { get; set; }

        public static Expression<Func<Project, ProjectInvoiceSummaryModel>> ProjectInvoiceSummaryProjection()
        {
            //SELECT dbo.Projects.Id, dbo.Projects.Name, dbo.Users.FirstName, dbo.Users.LastName, dbo.Users.Email,
            //dbo.Projects.Status, dbo.Projects.CropType, SUM(dbo.Invoices.Total) AS [All Invoice Total],
            //dbo.Projects.Crop, dbo.Projects.Acres, dbo.Projects.Start, dbo.Projects.[End]
            //FROM dbo.Projects INNER JOIN
            //dbo.Invoices ON dbo.Projects.Id = dbo.Invoices.ProjectId LEFT OUTER JOIN
            //dbo.Users ON dbo.Projects.PrincipalInvestigatorId = dbo.Users.Id
            //GROUP BY dbo.Projects.Id, dbo.Projects.Name, dbo.Users.FirstName, dbo.Users.LastName, dbo.Users.Email, dbo.Projects.Status, dbo.Projects.Crop, dbo.Projects.CropType, dbo.Projects.Acres, dbo.Projects.Start, dbo.Projects.[End]


            return project => new ProjectInvoiceSummaryModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Status = project.Status,
                CropType = project.CropType,
                Crop = project.Crop,
                Acres = project.Acres,
                Start = project.Start,
                End = project.End,
                CreatedOn = project.CreatedOn.ToPacificTime(),
                PrincipalInvestigatorName = project.PrincipalInvestigator.Name,
                PrincipalInvestigatorEmail = project.PrincipalInvestigator.Email,
                InvoiceTotal = project.Invoices.Sum(i => i.Total)
            };            
        }

       
    }
}
