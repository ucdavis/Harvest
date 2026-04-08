using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Models.ReportModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Harvest.Core.Domain.Project;

namespace Harvest.Web.Controllers
{
    public class ReportController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public ReportController(AppDbContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        [Authorize(Policy = AccessCodes.ReportAccess)]
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
        [Authorize(Policy = AccessCodes.ReportAccess)]
        public async Task<IActionResult> AllProjects(DateTime? start, DateTime? end)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            if (!start.HasValue)
            {
                start = new(DateTime.Now.Year - 1, 1, 1);
            }

            if (!end.HasValue)
            {
                end = new DateTime(DateTime.Now.Year - 1, 12, 31);
            }

            var model = new ProjectsListModel
            {
                Start = start.Value,
                End = end.Value,
                TeamName = team.Name,
                ProjectInvoiceSummaries = await _dbContext.Projects.Include(a => a.Invoices).Include(a => a.PrincipalInvestigator)
                    .Where(a => a.TeamId == team.Id && a.CreatedOn >= start.FromPacificTime() && a.CreatedOn <= end.FromPacificTime())
                    .Select(ProjectInvoiceSummaryModel.ProjectInvoiceSummaryProjection())
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Policy = AccessCodes.ReportAccess)]
        public async Task<IActionResult> HistoricalRateActivity(DateTime? start, DateTime? end)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            if (!start.HasValue)
            {
                start = new(DateTime.Now.Year - 1, 1, 1);
            }

            if (!end.HasValue)
            {
                end = new DateTime(DateTime.Now.Year - 1, 12, 31);
            }

            var model = new HistoricalRateActivityModelList
            {
                Start = start.Value,
                End = end.Value,
                TeamName = team.Name,
                HistoricalRates = await _dbContext.Rates.Where(a => a.TeamId == team.Id).Include(a => a.Expenses)
                    .Select(HistoricalRateActivityModel.Projection(start.Value.Date.FromPacificTime(), end.Value.Date.FromPacificTime()))
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Policy = AccessCodes.ReportAccess)]
        public async Task<IActionResult> StaleProjects()
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            var staleDate = DateTime.UtcNow.AddDays(-18);
            var statuses = new[] { Statuses.PendingApproval, Statuses.PendingCloseoutApproval };

            var model = new StaleProjectsListModel
            {
                TeamName = team.Name,
                StaleProjects = await _dbContext.Projects
                    .Include(a => a.PrincipalInvestigator)
                    .Include(a => a.Team)
                    .Where(a => a.TeamId == team.Id && statuses.Contains(a.Status) && a.LastStatusUpdatedOn <= staleDate)
                    .Select(StaleProjectModel.Projection())
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Policy = AccessCodes.ReportAccess)]
        public async Task<IActionResult> ProjectsUnbilledExpenses()
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            var model = new ProjectsUnbilledExpensesReportModel
            {
                TeamName = team.Name,
                Slug = team.Slug,
                Projects = await _dbContext.Projects
                    .AsNoTracking()
                    .Where(a => a.TeamId == team.Id && a.Expenses.Any(e => e.InvoiceId == null && e.Approved))
                    .OrderBy(a => a.Name)
                    .ThenBy(a => a.Id)
                    .Select(ProjectsUnbilledExpensesProjectRowModel.Projection())
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Policy = AccessCodes.ReportAccess)]
        public async Task<IActionResult> UnbilledExpenses()
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            var expenses = await _dbContext.Expenses
                .AsNoTracking()
                .Where(a => a.Project.TeamId == team.Id && a.InvoiceId == null && a.Approved)
                .OrderBy(a => a.Project.Name)
                .ThenBy(a => a.ProjectId)
                .ThenBy(a => a.CreatedOn)
                .ThenBy(a => a.Id)
                .Select(UnbilledExpenseRowModel.Projection())
                .ToListAsync();

            foreach (var expense in expenses)
            {
                expense.CreatedOnLocal = expense.CreatedOn.ToPacificTime();
                expense.ApprovedOnLocal = expense.ApprovedOn.ToPacificTime();
            }

            var model = new UnbilledExpensesReportModel
            {
                TeamName = team.Name,
                Slug = team.Slug,
                Expenses = expenses
            };

            return View(model);
        }

        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<IActionResult> WeeklyHoursByWorker(DateTime? selectedDate, string selectedRateType)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            var selectedWeek = selectedDate.HasValue
                ? GetWeekRangeForPacificDate(selectedDate.Value.Date)
                : GetMostRecentCompletedWeekRange();

            var currentUser = await _userService.GetCurrentUser();
            var isFieldManager = await _userService.HasAnyTeamRoles(TeamSlug, new[] { Role.Codes.FieldManager, Role.Codes.System });

            var workerPermissionsQuery = _dbContext.Permissions
                .AsNoTracking()
                .Where(a => a.TeamId == team.Id && a.Role.Name == Role.Codes.Worker);

            if (!isFieldManager)
            {
                workerPermissionsQuery = workerPermissionsQuery.Where(a =>
                    a.Parents.Any(p =>
                        p.UserId == currentUser.Id &&
                        p.TeamId == team.Id &&
                        (p.Role.Name == Role.Codes.Supervisor || p.Role.Name == Role.Codes.FieldManager)));
            }

            var workerIds = await workerPermissionsQuery
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            var availableRateTypes = Rate.Types.WorkerRateTypes.ToList();
            var useAllRateTypes = string.Equals(selectedRateType, "All", StringComparison.OrdinalIgnoreCase);
            var activeSelectedRateType = useAllRateTypes || availableRateTypes.Contains(selectedRateType)
                ? selectedRateType
                : Rate.Types.Labor;

            var startUtc = selectedWeek.Start.Date.FromPacificTime();
            var endUtcExclusive = selectedWeek.End.Date.AddDays(1).FromPacificTime();

            var entries = await _dbContext.Expenses
                .AsNoTracking()
                .Where(a =>
                    a.Project.TeamId == team.Id &&
                    a.CreatedById.HasValue &&
                    workerIds.Contains(a.CreatedById.Value) &&
                    a.CreatedOn >= startUtc &&
                    a.CreatedOn < endUtcExclusive &&
                    a.Rate.Unit == "Hourly" &&
                    (useAllRateTypes || a.Rate.Type == activeSelectedRateType))
                .OrderBy(a => a.CreatedBy.LastName)
                .ThenBy(a => a.CreatedBy.FirstName)
                .ThenBy(a => a.CreatedOn)
                .ThenBy(a => a.Project.Name)
                .ThenBy(a => a.Id)
                .Select(WeeklyHoursByWorkerRowModel.Projection())
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.EnteredOnLocal = entry.EnteredOnLocal.ToPacificTime();
            }

            var model = new WeeklyHoursByWorkerReportModel
            {
                SelectedDate = selectedWeek.Start,
                Start = selectedWeek.Start,
                End = selectedWeek.End,
                SelectedRateType = useAllRateTypes ? "All" : activeSelectedRateType,
                AvailableRateTypes = availableRateTypes,
                TeamName = team.Name,
                Slug = team.Slug,
                Workers = entries
                    .GroupBy(a => a.WorkerName)
                    .Select(a => new WeeklyHoursByWorkerWorkerGroupModel
                    {
                        WorkerName = a.Key,
                        Entries = a.ToList()
                    })
                    .ToList()
            };

            return View(model);
        }

        private static (DateTime Start, DateTime End) GetMostRecentCompletedWeekRange()
        {
            var today = DateTime.UtcNow.ToPacificTime().Date;
            var currentWeekStart = GetWeekRangeForPacificDate(today).Start;

            return (currentWeekStart.AddDays(-7), currentWeekStart.AddDays(-1));
        }

        private static (DateTime Start, DateTime End) GetWeekRangeForPacificDate(DateTime date)
        {
            var daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var start = date.AddDays(-daysSinceMonday);
            return (start, start.AddDays(6));
        }
    }
}
