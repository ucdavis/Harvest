namespace Harvest.Core.Models.Settings
{
    public class AggieEnterpriseOptions
    {
        public string GraphQlUrl { get; set; }
        public string Token { get; set; }

        public bool UseCoA { get; set; }
        public string CreditCoaNaturalAccount { get; set; }
        public string CreditPassthroughCoaNaturalAccount { get; set; }
    }
}

