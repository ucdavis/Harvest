using Harvest.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class WeeklyHoursByWorkerReportModel
    {
        [Display(Name = "Week Of")]
        [DataType(DataType.Date)]
        public DateTime? SelectedDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime Start { get; set; }

        [DataType(DataType.Date)]
        public DateTime End { get; set; }

        [Display(Name = "Rate Type")]
        public string SelectedRateType { get; set; } = Rate.Types.Labor;

        public List<string> AvailableRateTypes { get; set; } = new();

        public string TeamName { get; set; }
        public string Slug { get; set; }
        public List<WeeklyHoursByWorkerWorkerGroupModel> Workers { get; set; } = new();
    }

    public class WeeklyHoursByWorkerWorkerGroupModel
    {
        public string WorkerName { get; set; }
        public List<WeeklyHoursByWorkerRowModel> Entries { get; set; } = new();
        public decimal TotalHours => System.Linq.Enumerable.Sum(Entries, a => a.Hours);
    }

    public class WeeklyHoursByWorkerRowModel
    {
        [Display(Name = "Worker")]
        public string WorkerName { get; set; }

        [Display(Name = "Entered On")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime EnteredOnLocal { get; set; }

        [Display(Name = "Project Id")]
        public int ProjectId { get; set; }

        [Display(Name = "Project")]
        public string ProjectName { get; set; }

        public string Activity { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Approved { get; set; }

        [Display(Name = "Hours")]
        public decimal Hours { get; set; }

        [Display(Name = "Quantity")]
        public decimal? NonHourlyQuantity { get; set; }

        [Display(Name = "Unit")]
        public string NonHourlyUnit { get; set; }

        public static Expression<Func<Expense, WeeklyHoursByWorkerRowModel>> Projection()
        {
            return expense => new WeeklyHoursByWorkerRowModel
            {
                WorkerName = expense.CreatedBy != null ? expense.CreatedBy.Name : string.Empty,
                EnteredOnLocal = expense.CreatedOn,
                ProjectId = expense.ProjectId,
                ProjectName = expense.Project != null ? expense.Project.Name : string.Empty,
                Activity = expense.Activity,
                Type = expense.Type,
                Description = expense.Description,
                Approved = expense.Approved,
                Hours = expense.Rate != null && expense.Rate.Unit == "Hourly" ? expense.Quantity : 0,
                NonHourlyQuantity = expense.Rate != null && expense.Rate.Unit == "Hourly" ? null : expense.Quantity,
                NonHourlyUnit = expense.Rate != null && expense.Rate.Unit == "Hourly" ? string.Empty : expense.Rate.Unit
            };
        }
    }
}
