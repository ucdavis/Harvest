using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harvest.Core.Domain;

namespace Harvest.Web.Models
{
    public class QuoteModel
    {
        public Project Project { get; set; }
        public QuoteDetail Quote { get; set; }
    }


    // JSON quote detail stored in Quote.Text
    public class QuoteDetail
    {
        public static QuoteDetail Deserialize(string text)
        {
            return JsonSerializer.Deserialize<QuoteDetail>(text);
        }

        [JsonIgnore]
        public double Total
        {
            get
            {
                var acreage = Acres * AcreageRate;
                var activities = Activities.Sum(a => a.WorkItems.Sum(w => w.Quantity * w.Rate));
                return acreage + activities;
            }
        }

        public string ProjectName { get; set; }
        public double Acres { get; set; }
        public double AcreageRate { get; set; }
        public Activity[] Activities { get; set; }

    }

    public class Activity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public WorkItem[] WorkItems { get; set; }
    }

    public partial class WorkItem
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string Type { get; set; }
        public double Rate { get; set; }
        public int RateId { get; set; }
        public double Quantity { get; set; }
    }
}
