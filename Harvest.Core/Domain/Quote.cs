using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Quote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [MaxLength]
        public string Text { get; set; }

        public decimal Total { get; set; }

        public int InitiatedById { get; set; }

        public int? CurrentDocumentId { get; set; }

        public int? ApprovedById { get; set; }

        [Display(Name = "Aproved On")]
        public DateTime? ApprovedOn { get; set; }

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }

        [Display(Name = "Initiated By")]
        public User InitiatedBy { get; set; }

        [Display(Name = "Approved By")]
        public User ApprovedBy { get; set; }

        public List<Document> Documents { get; set; }

        public Document CurrentDocument { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quote>().HasIndex(a => a.ProjectId);
            modelBuilder.Entity<Quote>().HasIndex(a => a.ApprovedById);
            modelBuilder.Entity<Quote>().HasIndex(a => a.CurrentDocumentId);
            modelBuilder.Entity<Quote>().HasIndex(a => a.InitiatedById);

            modelBuilder.Entity<Quote>().Property(a => a.Total).HasPrecision(18, 2);
        }

        public class Statuses
        {
            public const string Created = "Created";
            public const string Proposed = "Proposed";
            public const string Approved = "Approved";
            public const string Superseded = "Superseded"; // When a newer quote takes the place of an existing approved quote

            public static List<string> TypeList = new List<string>
            {
                Created,
                Proposed,
                Approved,
                Superseded
            }.ToList();
        }
    }
}
