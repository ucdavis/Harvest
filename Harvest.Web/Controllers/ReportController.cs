using Harvest.Core.Data;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Web.Models.ReportModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class ReportController : SuperController
    {
        private readonly AppDbContext _dbContext;

        public ReportController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">Filter for project creation date </param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IActionResult> AllProjects(DateTime? start, DateTime? end)
        {
            
            if (!start.HasValue)
            {
                start = new(DateTime.Now.Year - 1, 1, 1);
            }
            
            if (!end.HasValue)
            {
                end = new DateTime(DateTime.Now.Year - 1, 12, 31);
            }
            var model = new ProjectsListModel();
            model.Start = start.Value;
            model.End = end.Value;
            model.ProjectInvoiceSummaries = await _dbContext.Projects.Include(a => a.Invoices).Include(a => a.PrincipalInvestigator)
                .Where(a => a.CreatedOn >= start.FromPacificTime() && a.CreatedOn <= end.FromPacificTime())
                .Select(ProjectInvoiceSummaryModel.ProjectInvoiceSummaryProjection()).ToListAsync();

            return View(model);

        }
    }
}
