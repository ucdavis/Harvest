using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Models.History
{
    public class ProjectTotalHistoryModel
    {
        public ProjectTotalHistoryModel() { }

        public ProjectTotalHistoryModel(Project project)
        {
            Id = project.Id;
            ChargedTotal = project.ChargedTotal;
        }

        public int Id { get; set; }
        public decimal ChargedTotal { get; set; }
    }
}
