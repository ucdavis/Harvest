using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Harvest.Core.Domain;
using Harvest.Core.Models;

namespace Test.Helpers
{
    public static class CreateValidEntities
    {
        public static Project Project(int? counter, bool loadAll = false, bool active = true)
        {
            var rtValue = new Project();
            rtValue.Id = counter ?? 99;
            rtValue.Start = new DateTime(2021, 06, 01);
            rtValue.End = DateTime.UtcNow.AddYears(1);
            rtValue.CropType = Harvest.Core.Domain.Project.CropTypes.Tree;
            rtValue.Crop = $"Crop{counter}";
            rtValue.Requirements = $"Requirements{counter}";
            if (active)
            {
                rtValue.Acres = 2.0;
                rtValue.AcreageRate = Rate(3);
                rtValue.Status = Harvest.Core.Domain.Project.Statuses.Active;
                rtValue.QuoteId = 1;
                rtValue.Quote = Quote(1);
                rtValue.IsApproved = true;
            }
            else
            {
                rtValue.Status = Harvest.Core.Domain.Project.Statuses.Requested;
                rtValue.IsApproved = false;
            }
            rtValue.Name = $"Name{counter}";
            rtValue.PrincipalInvestigatorId = 1;
            rtValue.PrincipalInvestigator = CreateValidEntities.User(1);
            rtValue.QuoteTotal = 10000.0m;
            rtValue.ChargedTotal = 5000.0m;
            rtValue.CreatedById = 1;
            rtValue.CreatedBy = CreateValidEntities.User(1);
            rtValue.CreatedOn = new DateTime(2021, 06, 01);
            rtValue.CurrentAccountVersion = 1;
            rtValue.IsActive = true;
            rtValue.Accounts = new List<Account>();
            rtValue.Accounts.Add(CreateValidEntities.Account(1));

            if (loadAll)    
            {
                //TODO?
            }            

            return rtValue;
        }

        public static Rate Rate(int? counter, string type = Harvest.Core.Domain.Rate.Types.Acreage)
        {
            switch (type)
            {
                case Harvest.Core.Domain.Rate.Types.Acreage:
                    return new Rate
                    {
                        Id = counter ?? 99,
                        Type = Harvest.Core.Domain.Rate.Types.Acreage,
                        Description = $"Description{counter}",
                        BillingUnit = $"BillingUnit{counter}",
                        Account = "3-RRACRES--RAS5",
                        Price = 1150.00m,
                        Unit = $"Unit{counter}"
                    };
                case Harvest.Core.Domain.Rate.Types.Equipment:
                    return new Rate
                    {
                        Id = counter ?? 99,
                        Type = Harvest.Core.Domain.Rate.Types.Equipment,
                        Description = $"Description{counter}",
                        BillingUnit = $"BillingUnit{counter}",
                        Account = "3-RRACRES--RAS5", //Use different account?
                        Price = 1150.00m,
                        Unit = $"Unit{counter}"
                    };
                case Harvest.Core.Domain.Rate.Types.Labor:
                    return new Rate
                    {
                        Id = counter ?? 99,
                        Type = Harvest.Core.Domain.Rate.Types.Labor,
                        Description = $"Description{counter}",
                        BillingUnit = $"BillingUnit{counter}",
                        Account = "3-RRACRES--RAS5", //Use different account?
                        Price = 1150.00m,
                        Unit = $"Unit{counter}"
                    };
                case Harvest.Core.Domain.Rate.Types.Other:
                    return new Rate
                    {
                        Id = counter ?? 99,
                        Type = Harvest.Core.Domain.Rate.Types.Other,
                        Description = $"Description{counter}",
                        BillingUnit = $"BillingUnit{counter}",
                        Account = "3-RRACRES--RAS5", //Use different account?
                        Price = 1150.00m,
                        Unit = $"Unit{counter}"
                    };
                default:
                    return new Rate
                    {
                        Id = counter ?? 99,
                        Type = type,
                        Description = $"Description{counter}",
                        BillingUnit = $"BillingUnit{counter}",
                        Account = "3-RRACRES--RAS5", //Use different account?
                        Price = 1150.00m,
                        Unit = $"Unit{counter}"
                    };
            }
            
        }

        public static User User(int counter)
        {
            var rtValue = new User();
            rtValue.Id = counter;
            rtValue.FirstName = $"FirstName{counter}";
            rtValue.LastName = $"LastName{counter}";
            rtValue.Email = $"Email{counter}@fake.com";
            rtValue.Iam = $"Iam{counter}";
            rtValue.Kerberos = $"Kerberos{counter}";

            return rtValue;
        }
        public static Quote Quote(int? counter)
        {
            var rtValue = new Quote();
            rtValue.Id = counter ?? 99;
            rtValue.ProjectId = 1;
            rtValue.Text = $"Text{counter}";
            rtValue.Total = 1150.00m;
            rtValue.InitiatedById = 1;
            rtValue.InitiatedBy = CreateValidEntities.User(1);
            rtValue.CreatedDate = DateTime.UtcNow;

            return rtValue;
        }

        public static QuoteDetail QuoteDetail()
        {
            var rtValue = new QuoteDetail();
            rtValue.AcreageRateId = 1;
            rtValue.Acres = 3;

            //So far, don't need more info for tests

            return rtValue;
        }
        public static Account Account(int? counter)
        {
            var rtValue = new Account();
            rtValue.Id = counter ?? 99;
            rtValue.Name = $"Name{counter}";
            rtValue.ProjectId = 1;
            rtValue.Number = "3-CRU9033";
            rtValue.Percentage = 100.00m;

            return rtValue;
        }

        public static Expense Expense(int? counter, int projectId)
        {
            var rtValue = new Expense();
            rtValue.Id = counter ?? 99;
            rtValue.ProjectId = projectId;
            rtValue.Description = $"Description{counter}";
            rtValue.Rate = Rate(3);
            rtValue.RateId = 3;
            rtValue.Rate.Account = "3-FRMRATE--RAY9";
            rtValue.Quantity = 2.00m;
            rtValue.Total = rtValue.Quantity * rtValue.Rate.Price;
            rtValue.Price = rtValue.Rate.Price;
            rtValue.CreatedOn = DateTime.UtcNow.AddDays(-1);
            rtValue.CreatedById = 1;
            rtValue.CreatedBy = CreateValidEntities.User(1);
            rtValue.Account = rtValue.Rate.Account;

            return rtValue;       

        }

        public static Invoice Invoice(int? counter, int projectId)
        {
            var rtValue = new Invoice();
            rtValue.Id = counter ?? 99;
            rtValue.ProjectId = projectId;
            rtValue.Total = 1150.00m;
            rtValue.CreatedOn = DateTime.UtcNow.AddDays(-1);
            rtValue.Status = Harvest.Core.Domain.Invoice.Statuses.Pending;

            return rtValue;
        }
    }
}
