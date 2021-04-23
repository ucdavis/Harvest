using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Harvest.Email.Models
{
    public class QuoteDecisionModel
    {
        public string PI { get; set; }
        public string ProjectName { get; set; }
        public string ProjectEnd { get; set; }
        public string ProjectStart { get; set; }
        public string Decision { get; set; }
        public string ButtonUrl { get; set; }
        public string DecisionColor { get; set; } = "#7aeb34";

        /// <summary>
        /// Sets model values when running TestController.TestBody
        /// </summary>
        public void InitForMjml()
        {
            PI            = "@Model.PI";
            ProjectName   = "@Model.ProjectName";
            ProjectStart  = "@Model.ProjectStart";
            ProjectEnd    = "@Model.ProjectEnd";
            Decision      = "@Model.Decision";
            DecisionColor = "@Model.DecisionColor";
            ButtonUrl     = "@Model.ButtonUrl";
        }
        public class Colors
        {
            public const string Approved = "#7aeb34";
            public const string Denied = "#cf3c3c";

        }
    }
}
