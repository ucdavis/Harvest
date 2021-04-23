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
        [Key]
        public int Id { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        [StringLength(512)]
        public string Crop { get; set; }

        [StringLength(50)]
        public string CropType { get; set; }

        [MaxLength]
        public string Requirements { get; set; }

        public double Acres { get; set; }

        public int? AcreageRateId { get; set; }
        public Rate AcreageRate { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

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

        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [Display(Name = "Current Account Version")]
        public int CurrentAccountVersion { get; set; }

        [Required]
        public bool IsApproved { get; set; } = true;

        [Required]
        // need or can we filter?
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created By")]
        public User CreatedBy { get; set; }

        [JsonIgnore]
        public List<Account> Accounts { get; set; }

        [JsonIgnore]
        public List<Quote> Quotes { get; set; }

        [JsonIgnore]
        public List<Field> Fields { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasIndex(a => a.Name);
            modelBuilder.Entity<Project>().HasIndex(a => a.CreatedById);
            modelBuilder.Entity<Project>().HasIndex(a => a.PrincipalInvestigatorId);
            modelBuilder.Entity<Project>().HasIndex(a => a.QuoteId);

            modelBuilder.Entity<Project>().Property(a => a.ChargedTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Project>().Property(a => a.QuoteTotal).HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Project)
                .WithMany(p => p.Accounts)
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

            modelBuilder.Entity<Field>()
                .HasOne(f => f.Project)
                .WithMany(f => f.Fields)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public class Statuses
        {
            public const string Requested = "Requested";
            public const string PendingAccountApproval = "PendingAccountApproval";
            public const string Active = "Active";
            public const string ChangeRequested = "ChangeRequested";
            public const string Completed = "Completed";

            public static List<string> TypeList = new List<string>
            {
                Requested,
                PendingAccountApproval,
                Active,
                ChangeRequested,
                Completed,
            }.ToList();
        }

        public class CropTypes
        {
            public const string Row = "Row";
            public const string Tree = "Tree";

            public static List<string> TypeList = new List<string>
            {
                Row,
                Tree
            };
        }
    }
}
