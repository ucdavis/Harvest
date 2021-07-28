using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Utilities;

namespace Harvest.Core.Models.History
{
    public class ProjectHistoryModel
    {
        public ProjectHistoryModel() { }

        public ProjectHistoryModel(Project project)
        {
            Id = project.Id;
            Start = project.Start;
            End = project.End;
            Crop = project.Crop;
            CropType = project.CropType;
            Requirements = project.Requirements;
            Acres = project.Acres;
            AcreageRateId = project.AcreageRateId;
            AcreageRate = project.AcreageRate == null ? null : new RateHistoryModel(project.AcreageRate);
            Name = project.Name;
            PrincipalInvestigatorId = project.PrincipalInvestigatorId;
            PrincipalInvestigator = project.PrincipalInvestigator == null ? null : new UserHistoryModel(project.PrincipalInvestigator);
            QuoteId = project.QuoteId;
            Quote = project.Quote?.Text.Deserialize<QuoteDetail>();
            OriginalProjectId = project.OriginalProjectId;
            QuoteTotal = project.QuoteTotal;
            ChargedTotal = project.ChargedTotal;
            CreatedById = project.CreatedById;
            CreatedOn = project.CreatedOn;
            Status = project.Status;

            CreatedById = project.CreatedById;
            CreatedOn = project.CreatedOn;
            Status = project.Status;
            CurrentAccountVersion = project.CurrentAccountVersion;
            IsApproved = project.IsApproved;
            IsActive = project.IsActive;
            CreatedBy = project.CreatedBy == null ? null : new UserHistoryModel(project.CreatedBy);
            Accounts = project.Accounts?.Select(a => new AccountHistoryModel(a)).ToList();
            Fields = project.Fields?.Select(f => new FieldHistoryModel(f)).ToList();
        }

        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Crop { get; set; }
        public string CropType { get; set; }
        public string Requirements { get; set; }
        public double Acres { get; set; }
        public int? AcreageRateId { get; set; }
        public RateHistoryModel AcreageRate { get; set; }
        public string Name { get; set; }
        public int PrincipalInvestigatorId { get; set; }
        public UserHistoryModel PrincipalInvestigator { get; set; }
        public int? QuoteId { get; set; }
        public QuoteDetail Quote { get; set; }
        public int? OriginalProjectId { get; set; }
        public decimal QuoteTotal { get; set; }
        public decimal ChargedTotal { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public int CurrentAccountVersion { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsActive { get; set; }
        public UserHistoryModel CreatedBy { get; set; }
        public List<AccountHistoryModel> Accounts { get; set; }
        public List<FieldHistoryModel> Fields { get; set; }
    }
}
