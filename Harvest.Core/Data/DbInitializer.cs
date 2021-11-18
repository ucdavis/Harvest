using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Data
{
    public class DbInitializer
    {
        private readonly AppDbContext _dbContext;

        public DbInitializer(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Initialize(bool recreateDb)
        {
            if (recreateDb)
            {
                //do what needs to be done?
            }

            //Make sure roles exist
            await CheckCreateRole(Role.Codes.System);
            await CheckCreateRole(Role.Codes.FieldManager);
            await CheckCreateRole(Role.Codes.Supervisor);
            await CheckCreateRole(Role.Codes.Worker);

            await _dbContext.SaveChangesAsync();

            var systemRole = await _dbContext.Roles.SingleAsync(a => a.Name == Role.Codes.System);
            var user = new User
            {
                Email = "jsylvestre@ucdavis.edu",
                Kerberos = "jsylvest",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Iam = "1000009309"
            };
            await CheckOrCreatePermission(systemRole, user);
            user = new User
            {
                Email = "srkirkland@ucdavis.edu",
                Kerberos = "postit",
                FirstName = "Scott",
                LastName = "Kirkland",
                Iam = "1000029584"
            };
            await CheckOrCreatePermission(systemRole, user);
            user = new User
            {
                Email = "swebermilne@ucdavis.edu",
                Kerberos = "sweber",
                FirstName = "Spruce",
                LastName = "Weber-Milne",
                Iam = "1000255034"
            };
            await CheckOrCreatePermission(systemRole, user);
            user = new User
            {
                Email = "cydoval@ucdavis.edu",
                Kerberos = "cydoval",
                FirstName = "Calvin",
                LastName = "Doval",
                Iam = "1000050298"
            };
            await CheckOrCreatePermission(systemRole, user);

            user = new User
            {
                Email = "bmceligot@ucdavis.edu",
                Kerberos = "mceligot",
                FirstName = "Brian",
                LastName = "McEligot",
                Iam = "1000007413"
            };
            await CheckOrCreatePermission(systemRole, user);

            await CheckCreateSampleRates();
            await _dbContext.SaveChangesAsync();

            await CheckCreateCropLookups();
            await _dbContext.SaveChangesAsync();

            return;
        }

        private async Task CheckCreateCropLookups()
        {
            if (await _dbContext.Crops.AnyAsync())
            {
                return;
            }

            var cropLookup = new Crop { Name = "Corn", Type = Project.CropTypes.Row };
            await _dbContext.Crops.AddAsync(cropLookup);

            cropLookup = new Crop { Name = "Cabbage", Type = Project.CropTypes.Row };
            await _dbContext.Crops.AddAsync(cropLookup);

            cropLookup = new Crop { Name = "Celery", Type = Project.CropTypes.Row };
            await _dbContext.Crops.AddAsync(cropLookup);

            cropLookup = new Crop { Name = "Potato", Type = Project.CropTypes.Row };
            await _dbContext.Crops.AddAsync(cropLookup);

            cropLookup = new Crop { Name = "Almond", Type = Project.CropTypes.Tree };
            await _dbContext.Crops.AddAsync(cropLookup);

            cropLookup = new Crop { Name = "Orange", Type = Project.CropTypes.Tree };
            await _dbContext.Crops.AddAsync(cropLookup);

            cropLookup = new Crop { Name = "Lemon", Type = Project.CropTypes.Tree };
            await _dbContext.Crops.AddAsync(cropLookup);

        }

        private async Task CheckCreateSampleRates()
        {

            if (await _dbContext.Rates.AnyAsync(a => a.IsActive))
            {
                return;
            }

            var createdBy = await _dbContext.Users.Where(a => a.Kerberos == "jsylvest").FirstAsync();

            await CreateAcreageRates(createdBy);

            await CreateLaborRates(createdBy);

            await CreateOtherRates(createdBy);

            await CreateEquipmentRates(createdBy);

        }

        private async Task CreateEquipmentRates(User createdBy)
        {
            var rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "PLS Row Planter";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS--RAPB";
            rate.Price = 34.91m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "PLS Cab Tractor";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS--RAPB";
            rate.Price = 132.01m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "PLS 15 Row Tractor";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS--RAPB";
            rate.Price = 34.44m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "PLS 4x4";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS--RAPB";
            rate.Price = 14.11m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "PLS Flail Mower";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS--RAPB";
            rate.Price = 90.02m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "PLS Thresher";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS--RAPB";
            rate.Price = 16.67m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Trencher (large)";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 685.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Excavator (small)";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 385.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Excavator (large)";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 1250.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Grader";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 50.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Front End Loader";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 50.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES 2000 gallon water truck";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 25.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Trencher (small)";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 250.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Skid Loader";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 190.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES 3-Point Sprayer";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 25.00m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Boom Sprayer Acre";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 25.00m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Boom Sprayer Hand Wand Gal";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 20.00m;
            rate.Unit = "Per Gallon";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES 5' Rototiller";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 90.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES 7' Rototiller";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 90.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES ATV";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 14.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Bobcat/Gator/Fourtrax";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 75.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Clamco Granular Fertilizer Applicators";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 50.00m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Closed System";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 500.00m;
            rate.Unit = "Each";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Dolly";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 125.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Drip Tape Install/Remove";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 55.00m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Forklift";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 16.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES GPS Leveller";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 57.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES 10' Tye Drill Planter";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 76.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES 20' Great Plains Drill Planter";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 18.00m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Monosem Planter";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 35.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Preplant Cultivator/In-season Cultivator";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 65.00m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Tractor - GPS";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 132.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Tractor John Deere 7810";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 114.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Tractor John Deere 5090M";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 34.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Trailer";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 16.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Truck";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 427.00m;
            rate.Unit = "Per Month";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Truck Special Service";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 105.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Truck 1997 Ford F-150";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 63.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Weld/Scraper";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 30.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES JD Booster Pump";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 15.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Lister";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 65.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES GT Cart Strip Weight";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE--RAY9";
            rate.Price = 184.00m;
            rate.Unit = "Per Strip Weighed";

            await _dbContext.Rates.AddAsync(rate);
        }

        private async Task CreateOtherRates(User createdBy)
        {
            var rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Crop Destruction Row";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLV--RAPB";
            rate.Price = 66.67m;
            rate.Unit = "Per acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Crop Destruction Tree";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLP--RAPB";
            rate.Price = 66.67m;
            rate.Unit = "Per acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Crop Destruction Env. Hort.";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLE--RAPB";
            rate.Price = 66.67m;
            rate.Unit = "Per acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Crop Destruction Wolfskill";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLW--RAPB";
            rate.Price = 66.67m;
            rate.Unit = "Per acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Century Project Soil Sample";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES-CNTRY-RAS5";
            rate.Price = 70.00m;
            rate.Unit = "Per sample";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Russell Ranch Barn Setup - Internal";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRBARN1--RAS5";
            rate.Price = 1200.00m;
            rate.Unit = "Per event";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Russell Ranch Barn Setup - External";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRBARN1--RAS5";
            rate.Price = 1605.00m;
            rate.Unit = "Per event";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Russell Ranch Barn Rental Half Day - Internal";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRBARN1--RAS5";
            rate.Price = 600.00m;
            rate.Unit = "Per 1/2 day";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Russell Ranch Barn Rental Half Day - External";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRBARN1--RAS5";
            rate.Price = 803.00m;
            rate.Unit = "Per 1/2 day";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Russell Ranch Barn Rental Day - Internal";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRBARN1--RAS5";
            rate.Price = 1000.00m;
            rate.Unit = "Per day";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Russell Ranch Barn Rental Day - External";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRBARN1--RAS5";
            rate.Price = 1337.00m;
            rate.Unit = "Per day";

            await _dbContext.Rates.AddAsync(rate);
        }

        private async Task CreateLaborRates(User createdBy)
        {
            var rate = DefaultRate(createdBy);
            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "RR Skilled Labor";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES--RAS5";
            rate.Price = 60.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "RR Mechanic";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRMSHOP--RAS5";
            rate.Price = 72.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "Century Project Skilled Labor";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES-CNTRY-RAS5";
            rate.Price = 91.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "Century Project Mechanic";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES-CNTRY-RAS5";
            rate.Price = 92.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Farm Labor";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFSA--RAPB";
            rate.Price = 34.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Row Crop";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLV--RAPB";
            rate.Price = 60.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Tree Crop";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLP--RAPB";
            rate.Price = 60.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Env. Hort.";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLE--RAPB";
            rate.Price = 60.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Wolfskill";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLW--RAPB";
            rate.Price = 60.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Overtime Row Crop";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLV--RAPB";
            rate.Price = 50.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Overtime Tree Crop";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLP--RAPB";
            rate.Price = 50.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Overtime Env. Hort.";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLE--RAPB";
            rate.Price = 50.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Skilled Labor Overtime Wolfskill";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLW--RAPB";
            rate.Price = 50.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Mechanic";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS--RAPB";
            rate.Price = 72.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);
        }

        private async Task CreateAcreageRates(User createdBy)
        {
            var rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Acreage;
            rate.Description = "Russell Ranch";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES--RAS5";
            rate.Price = 1150.00m;
            rate.Unit = "Acre per Year";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Acreage;
            rate.Description = "Century Project";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES-CNTRY-RAS5";
            rate.Price = 3281.00m;
            rate.Unit = "Acre per Year";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Acreage;
            rate.Description = "Plant Sciences Row Crop";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLV--RAPB";
            rate.Price = 1150.00m;
            rate.Unit = "Acre per Year";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Acreage;
            rate.Description = "Plant Sciences Tree Crop";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLP--RAPB";
            rate.Price = 1150.00m;
            rate.Unit = "Acre per Year";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Acreage;
            rate.Description = "Plant Sciences Env. Hort.";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLW--RAPB";
            rate.Price = 1150.00m;
            rate.Unit = "Acre per Year";

            await _dbContext.Rates.AddAsync(rate);
        }

        private static Rate DefaultRate(User createdBy)
        {
            var rate = new Rate {CreatedById = createdBy.Id, UpdatedById = createdBy.Id, CreatedOn = DateTime.UtcNow};
            rate.UpdatedOn = rate.CreatedOn;
            return rate;
        }

        private async Task CheckOrCreatePermission(Role systemRole, User user)
        {
            var userToCreate = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == user.Iam);
            if (userToCreate == null)
            {
                userToCreate = user;
                await _dbContext.Users.AddAsync(userToCreate);
            }

            var permission =
                await _dbContext.Permissions.SingleOrDefaultAsync(a =>
                    a.User.Id == userToCreate.Id && a.Role.Id == systemRole.Id);
            if (permission == null)
            {
                permission = new Permission {User = userToCreate, Role = systemRole};
                await _dbContext.Permissions.AddAsync(permission);
            }
        }

        private async Task CheckCreateRole(string role)
        {
            if (!await _dbContext.Roles.AnyAsync(a => a.Name == role))
            {
                var roleToCreate = new Role {Name = role};
                await _dbContext.Roles.AddAsync(roleToCreate);
            }
            
            return;
        }
    }
}
