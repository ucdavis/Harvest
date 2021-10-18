using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.InvoiceModels;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Harvest.Core.Services
{
    public interface IExpenseService
    {
        Task CreateYearlyAcreageExpense(Project project);
        Task CreateChangeRequestAdjustment(Project project, decimal extraAcres);
        Task CreateChangeRequestAdjustmentMaybe(Project project, QuoteDetail newQuote, QuoteDetail oldQuote);
    }

    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProjectHistoryService _historyService;
        private readonly IDateTimeService _dateTimeService;

        public ExpenseService(AppDbContext dbContext, IProjectHistoryService historyService, IDateTimeService dateTimeService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
            _dateTimeService = dateTimeService;
        }


        public async Task CreateYearlyAcreageExpense(Project project)
        {
            if (project.Acres <= 0)
            {
                return;
            }

            var now = _dateTimeService.DateTimeUtcNow();

            //Look for an acreage expense in the last year or so.
            //My thoughts are, invoices get created on the first business day of the month, so if I look back exactly 1 year, I may find one created on the 3rd
            //This wouldn't be horrible as I would catch it next month. But the year after that the fee could get moved another month back....
            //So, if I look back 1 year, less a week (or maybe 14 days), that will not find the one at the start of the month.
            //If it was created Jan 3, and this is running Dec 1-7, it will find it and not create the fee
            var compareDate = now.Date.AddYears(-1).AddDays(7); 
            if (await _dbContext.Expenses.AnyAsync(a => a.ProjectId == project.Id && a.Type != Rate.Types.Adjustment && a.CreatedOn >= compareDate && a.Rate.Type == Rate.Types.Acreage))
            {
                Log.Information("Project {projectId} found an acreage expense within the last year, skipping", project.Id);
                return;
            }

            var amountToCharge = Math.Round((decimal)project.Acres * project.AcreageRate.Price, 2, MidpointRounding.ToZero); //TODO: How to round this?
            if (amountToCharge < 0.01m)
            {
                Log.Error("Project {projectId} would have an acreage amount less than 0.01. Skipping.", project.Id);
                return;
            }

            var expense = CreateExpense(project, amountToCharge, (decimal)project.Acres, project.AcreageRate);

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AcreageExpenseCreated(project.Id, expense);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CreateChangeRequestAdjustment(Project project, decimal extraAcres)
        {
            if (extraAcres <= 0)
            {
                return;
            }

            if (project.AcreageRate == null) //I guess this is ok. Might want to call it later in the change request when it should have it?
            {
                var project1 = project;
                project = await _dbContext.Projects.Include(a => a.AcreageRate).SingleAsync(a => a.Id == project1.Id);
            }

            var amountToCharge = Math.Round(extraAcres * project.AcreageRate.Price, 2, MidpointRounding.ToZero); //TODO: How to round this?
            if (amountToCharge < 0.01m)
            {
                Log.Error("Adjustment Project {projectId} would have an acreage amount less than 0.01. Skipping.", project.Id);
                return;
            }

            var expense = CreateExpense(project, amountToCharge, extraAcres, project.AcreageRate);
            expense.Type = Rate.Types.Adjustment;
            expense.Description = $"Acreage Adjustment -- {expense.Description}".Truncate(250);

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AcreageExpenseCreated(project.Id, expense);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Did an invoice get created that we have to do anything?
        /// Did the rate Account change?
        /// Did the acreage decrease?
        /// Did the acreage increase?
        /// Do we need to create
        /// </summary>
        /// <param name="project"></param>
        /// <param name="newQuote"></param>
        /// <param name="oldQuote"></param>
        /// <returns></returns>
        public async Task CreateChangeRequestAdjustmentMaybe(Project project, QuoteDetail newQuote, QuoteDetail oldQuote)
        {
            var rateChanged = false;
            var partialRefund = false;

            var now = _dateTimeService.DateTimeUtcNow();
            var compareDate = now.Date.AddYears(-1);
            if (await _dbContext.Expenses.AnyAsync(a => a.ProjectId == project.Id && a.Type != Rate.Types.Adjustment && a.CreatedOn >= compareDate && a.Rate.Type == Rate.Types.Acreage))
            {
                //Ok, we found an acreage expense within the last year. Do we need to refund it?
                if (oldQuote.AcreageRateId != newQuote.AcreageRateId && oldQuote.AcreageRateId != null)
                {
                    //Create refund for old rate
                    rateChanged = true;
                    var oldRate = await _dbContext.Rates.SingleAsync(a => a.Id == oldQuote.AcreageRateId);
                    var amount = Math.Round((decimal)oldQuote.Acres * oldRate.Price, 2, MidpointRounding.ToZero) * -1; //Negative amount
                    var refundExpense = CreateExpense(project, amount, (decimal)oldQuote.Acres, oldRate);
                    refundExpense.Type = Rate.Types.Adjustment;
                    refundExpense.Description = $"Acreage Adjustment (Refund) -- {oldRate.Description}".Truncate(250);
                    await _dbContext.Expenses.AddAsync(refundExpense);
                    await _historyService.AcreageExpenseCreated(project.Id, refundExpense);
                }

                if (!rateChanged)
                {
                    //Do we need to do a partial refund?
                    if (oldQuote.Acres > newQuote.Acres)
                    {
                        var rate = await _dbContext.Rates.SingleAsync(a => a.Id == newQuote.AcreageRateId);
                        var acres = (decimal) (newQuote.Acres - oldQuote.Acres);
                        var amount = Math.Round(acres * rate.Price, 2, MidpointRounding.ToZero); //Negative amount
                        if (amount < 0) //Hey maybe some rounding issue
                        {
                            partialRefund = true;
                            var refundExpense = CreateExpense(project, amount, acres, rate);
                            refundExpense.Type = Rate.Types.Adjustment;
                            refundExpense.Description = $"Acreage Adjustment (Partial Refund) -- {rate.Description}".Truncate(250);
                            await _dbContext.Expenses.AddAsync(refundExpense);
                            await _historyService.AcreageExpenseCreated(project.Id, refundExpense);
                        }
                    }
                }
            }




            throw new NotImplementedException();
        }


        private Expense CreateExpense(Project project, decimal amountToCharge, decimal acres, Rate rate)
        {
            //If we populate the user, then this will have the PI's user because it is done in the request approval for change requests.
            var now = _dateTimeService.DateTimeUtcNow();

            var expense = new Expense
            {
                Type = project.AcreageRate.Type, //This gets changed for adjustments
                Description = project.AcreageRate.Description,
                Price = project.AcreageRate.Price,
                Quantity = acres,
                Total = Math.Round(amountToCharge,2, MidpointRounding.ToZero),
                ProjectId = project.Id,
                RateId = project.AcreageRate.Id,
                InvoiceId = null,
                CreatedOn = now,
                CreatedBy = null,
                Account = project.AcreageRate.Account,
            };
            return expense;
        }
    }
}
