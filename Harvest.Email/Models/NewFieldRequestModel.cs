using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Harvest.Email.Models
{
    public class NewFieldRequestModel
    {
        public string PI { get; set; }
        public string ProjectName { get; set; }
        public string ProjectEnd { get; set; }
        public string ProjectStart { get; set; }
        public string CropType { get; set; }
        public string Crops { get; set; }
        public string Requirements { get; set; }
        public string ButtonUrl { get; set; }

        /// <summary>
        /// Sets model values when running TestController.TestBody
        /// </summary>
        public void InitForMjml()
        {
            PI           = "@Model.PI";
            ProjectName  = "@Model.ProjectName";
            ProjectStart = "@Model.ProjectStart";
            ProjectEnd   = "@Model.ProjectEnd";
            CropType     = "@Model.CropType";
            Crops        = "@Model.Crops";
            Requirements = "@Model.Requirements";
            ButtonUrl    = "@Model.ButtonUrl";
        }
    }
}
