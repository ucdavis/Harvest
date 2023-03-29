using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
namespace Harvest.Core.Domain
{
    public class Project
    {
        public Project()
        {
            Fields = new List<Field>();
            Accounts = new List<Account>();
            Quotes = new List<Quote>();
            Attachments = new List<ProjectAttachment>();
            LastStatusUpdatedOn = DateTime.UtcNow;
            CreatedOn = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        
        [Required] public int TeamId { get; set; } = 1;

        public Team Team { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        [StringLength(512)]
        [Required]
        public string Crop { get; set; }

        [StringLength(50)]
        [Required]
        public string CropType { get; set; }

        [MaxLength]
        [Required]
        public string Requirements { get; set; }

        public double Acres { get; set; }

        public int? AcreageRateId { get; set; }
        public Rate AcreageRate { get; set; }

        [StringLength(200)]
        public string Name { get; set; }


        public string NameAndId => $"{Name} ({Id})";

        public int PrincipalInvestigatorId { get; set; }

        public User PrincipalInvestigator { get; set; }

        public int? QuoteId { get; set; }

        public Quote Quote { get; set; }

        // Change request will refer back to original project
        public int? OriginalProjectId { get; set; }

        [JsonIgnore]
        public Project OriginalProject { get; set; }

        [Required]
        [Display(Name = "Quote Total")]
        public decimal QuoteTotal { get; set; }

        [Required]
        [Display(Name = "Charged Total")]
        public decimal ChargedTotal { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastStatusUpdatedOn { get; set; }

        [StringLength(50)]
        [Required]
        public string Status { get; set; }

        [Required]
        [Display(Name = "Current Account Version")]
        public int CurrentAccountVersion { get; set; }

        [Required]
        public bool IsApproved { get; set; } = false;

        [Required]
        // need or can we filter?
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created By")]
        public User CreatedBy { get; set; }

        public List<Account> Accounts { get; set; }

        [JsonIgnore]
        public List<Quote> Quotes { get; set; }

        [JsonIgnore]
        public List<Field> Fields { get; set; }

        [JsonIgnore]
        public List<Ticket> Tickets { get; set; }

        [JsonIgnore]
        public List<Expense> Expenses { get; set; }

        public List<ProjectAttachment> Attachments { get; set; }

        [JsonIgnore]
        public List<Invoice> Invoices { get; set; }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            LastStatusUpdatedOn = DateTime.UtcNow;
        }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: temporary for migrations
            modelBuilder.Entity<Rate>().Property(a => a.TeamId).HasDefaultValue(1);
            modelBuilder.Entity<Project>().HasIndex(a => a.TeamId);
            
            modelBuilder.Entity<Project>().HasIndex(a => a.Name);
            modelBuilder.Entity<Project>().HasIndex(a => a.CreatedById);
            modelBuilder.Entity<Project>().HasIndex(a => a.PrincipalInvestigatorId);
            modelBuilder.Entity<Project>().HasIndex(a => a.QuoteId);
            modelBuilder.Entity<Project>().HasIndex(a => a.IsActive);

            modelBuilder.Entity<Project>().Property(a => a.Requirements).HasDefaultValue("N/A");

            modelBuilder.Entity<Project>().Property(a => a.ChargedTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Project>().Property(a => a.QuoteTotal).HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Project)
                .WithMany(p => p.Accounts)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectAttachment>()
                .HasOne(a => a.Project)
                .WithMany(a => a.Attachments)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>()
                .HasOne(a => a.Project)
                .WithMany(a => a.Expenses)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(a => a.Project)
                .WithMany(a => a.Tickets)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Project)
                .WithMany()
                .HasForeignKey(n => n.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectHistory>()
                .HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Project)
                .WithMany(p => p.Quotes)
                .HasForeignKey(q => q.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Quote)
                .WithMany()
                .HasForeignKey(p => p.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Field>()
                .HasOne(f => f.Project)
                .WithMany(f => f.Fields)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public class Statuses
        {
            public const string Requested = "Requested";
            public const string PendingApproval = "PendingApproval";
            public const string QuoteRejected = "QuoteRejected";
            public const string PendingAccountApproval = "PendingAccountApproval";
            public const string Active = "Active";
            public const string ChangeRequested = "ChangeRequested";
            public const string AwaitingCloseout = "AwaitingCloseout";
            public const string PendingCloseoutApproval = "PendingCloseoutApproval";
            public const string FinalInvoicePending = "FinalInvoicePending";
            public const string Completed = "Completed";
            public const string Canceled = "Canceled";
            public const string ChangeApplied = "ChangeApplied"; //Change Request was approved and applied, this is no longer an active project.

            public static List<string> TypeList = new List<string>
            {
                Requested,
                PendingApproval,
                QuoteRejected,
                PendingAccountApproval,
                Active,
                ChangeRequested,
                AwaitingCloseout,
                PendingCloseoutApproval,
                FinalInvoicePending,
                Completed,
                Canceled,
                ChangeApplied,
            }.ToList();
        }

        public class CropTypes
        {
            public const string Row = "Row";
            public const string Tree = "Tree";
            public const string Other = "Other";

            public static List<string> TypeList = new List<string>
            {
                Row,
                Tree,
                Other
            };
        }
    }
}
