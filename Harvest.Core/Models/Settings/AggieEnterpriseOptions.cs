namespace Harvest.Core.Models.Settings
{
    public class AggieEnterpriseOptions
    {
        public string GraphQlUrl { get; set; }
        public string Token { get; set; }

        public bool UseCoA { get; set; }
        public string NormalCoaNaturalAccount { get; set; }
        public string PassthroughCoaNaturalAccount { get; set; }
    }
}

