using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using NetTopologySuite.Geometries;

namespace Harvest.Core.Models.History
{
    public class FieldHistoryModel
    {
        public FieldHistoryModel() {}

        public FieldHistoryModel(Field field)
        {
            Id = field.Id;
            ProjectId = field.ProjectId;
            Crop = field.Crop;
            Details = field.Details;
            Location = field.Location;
            IsActive = field.IsActive;
        }

        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Crop { get; set; }
        public string Details { get; set; }
        public Polygon Location { get; set; }
        public bool IsActive { get; set; }
    }
}
