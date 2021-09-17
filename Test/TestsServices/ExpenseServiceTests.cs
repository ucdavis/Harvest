using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvest.Core.Extensions;
using Test.Helpers;
using TestHelpers.Helpers;
using Xunit;


namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class ExpenseServiceTests
    {
        public Mock<AppDbContext> MockDbContext { get; set; }
        public Mock<IProjectHistoryService> MockProjectHistoryService { get; set; }
        public Mock<IUserService> MockUserService { get; set; }
        

        public List<Project> Projects { get; set; }

        public ExpenseServiceTests()
        {
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();
            MockUserService = new Mock<IUserService>();
        }

        #region Setups
        private void SetupData()
        {
            Projects = new List<Project>();
            for (int i = 0; i < 3; i++)
            {
                Projects.Add(CreateValidEntities.Project(i + 1));
            }
            Projects[1].IsActive = false;
        }

        private void MockData()
        {
            MockDbContext.Setup(a => a.Projects).Returns(Projects.AsQueryable().MockAsyncDbSet().Object);
            MockUserService.Setup(a => a.GetCurrentUser()).ReturnsAsync(() => null);
            //MockUserService.Setup(a => a.GetCurrentUser()).ReturnsAsync(CreateValidEntities.User(1));
        }

        #endregion


    }
}
