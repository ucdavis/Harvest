using System;
using System.Text.Json;
using Harvest.Core.Domain;
using Harvest.Core.Utilities;
using NetTopologySuite.Geometries;

namespace Harvest.Core.Models
{
    public class QuoteModel
    {
        public Project Project { get; set; }
        public QuoteDetail Quote { get; set; }
    }


    // JSON quote detail stored in Quote.Text
    public class QuoteDetail
    {
        public QuoteDetail()
        {
            Fields = new QuoteField[0];
            Activities = new Activity[0];
        }

        public static QuoteDetail Deserialize(string text)
        {
            return JsonSerializer.Deserialize<QuoteDetail>(text, JsonOptions.Standard.WithGeoJson());
        }

        public static string Serialize(QuoteDetail value)
        {
            return JsonSerializer.Serialize<QuoteDetail>(value, JsonOptions.Standard.WithGeoJson());
        }

        public string ProjectName { get; set; }
        public double Acres { get; set; }
        public double AcreageRate { get; set; }
        public int? AcreageRateId { get; set; }
        public string AcreageRateDescription { get; set; }
        public double AcreageTotal { get; set; }
        public double ActivitiesTotal { get; set; }
        public double LaborTotal { get; set; }
        public double EquipmentTotal { get; set; }
        public double OtherTotal { get; set; }
        public double GrandTotal { get; set; }
        public int Years { get; set; }
        public QuoteField[] Fields { get; set; }
        public Activity[] Activities { get; set; }

        public User ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }

    }

    public class QuoteField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Crop { get; set; }
        public string Details { get; set; }
        public Polygon Geometry { get; set; }
    }

    public class Activity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Total { get; set; }
        public int Year { get; set; }
        public decimal Adjustment { get; set; }
        public WorkItem[] WorkItems { get; set; }
    }

    public partial class WorkItem
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int ActivityId { get; set; }
        public string Type { get; set; }
        public double Rate { get; set; }
        public int RateId { get; set; }
        public double Quantity { get; set; }
        public double Total { get; set; }
        public string Unit { get; set; }
        public bool Markup { get; set; }
        public bool IsPassthrough { get; set; }
    }
}
