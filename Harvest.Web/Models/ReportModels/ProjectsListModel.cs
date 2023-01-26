using Harvest.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class ProjectsListModel
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public List<ProjectInvoiceSummaryModel> ProjectInvoiceSummaries { get; set; }
    }
    
    public class ProjectInvoiceSummaryModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public string CropType { get; set; }
        public string Crop { get; set; }
        public double Acres { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateTime CreatedOn { get; set; }

        public string PrincipalInvestigatorName { get; set; }
        public string PrincipalInvestigatorEmail { get; set; }

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
                CreatedOn = project.CreatedOn,
                PrincipalInvestigatorName = project.PrincipalInvestigator.Name,
                PrincipalInvestigatorEmail = project.PrincipalInvestigator.Email,
                InvoiceTotal = project.Invoices.Sum(i => i.Total)
            };            
        }

       
    }
}
