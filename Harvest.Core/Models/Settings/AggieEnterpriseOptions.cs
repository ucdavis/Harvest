namespace Harvest.Core.Models.Settings
{
    public class AggieEnterpriseOptions
    {
        public string GraphQlUrl { get; set; }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string TokenEndpoint { get; set; }
        public string ScopeApp { get; set; }
        public string ScopeEnv { get; set; }

        public bool UseCoA { get; set; }
        public string NormalCoaNaturalAccount { get; set; }
        public string PassthroughCoaNaturalAccount { get; set; }
        public string PpmSpecialNaturalAccounts { get; set; } //This MUST NOT contain the normal and passthrough natural accounts
    }
}

