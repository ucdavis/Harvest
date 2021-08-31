namespace Harvest.Core.Models.Settings
{
    public class DevSettings
    {
        public bool RecreateDb { get; set; }
        public bool UseSql { get; set; } = true;
        public bool NightlyInvoices { get; set; }
    }
}