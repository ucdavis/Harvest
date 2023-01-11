using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Rate
    {
        [Key] public int Id { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(15)]
        public string Type { get; set; }
        [Required]
        [StringLength(250)]
        public string Description { get; set; }
        /// <summary>
        /// For example, Per Day, Per Gallon, etc.
        /// </summary>
        [StringLength(50)]
        public string Unit { get; set; }

        [StringLength(50)]
        [Display(Name = "Billing Unit")]
        public string BillingUnit { get; set; }
        [Required]
        [StringLength(80)] //Probably only 69 characters, but just in case
        public string Account { get; set; }

        [Display(Name = "Rate")] 
        [Range(0.01, Double.MaxValue, ErrorMessage = "Rate must be greater than 0.01")]
        public decimal Price { get; set; }
        [Display(Name = "Effective Date")]
        public DateTime? EffectiveOn { get; set; } //Hiding from entry and display for now.

        [Required] 
        public int CreatedById { get; set; }

        [Display(Name = "Created By")]
        public User CreatedBy { get; set; }

        [Required] 
        public int UpdatedById { get; set; }

        [Display(Name = "Updated By")]
        public User UpdatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedOn { get; set; }
        [Display(Name = "Updated Date")]
        public DateTime UpdatedOn { get; set; }

        [Display(Name = "Pass Through")]
        public bool IsPassthrough { get; set; } = false;

        // projects using this rate
        [JsonIgnore]
        public List<Project> Projects { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rate>().HasIndex(a => a.Type);
            modelBuilder.Entity<Rate>().HasIndex(a => a.Description);
            modelBuilder.Entity<Rate>().HasIndex(a => a.CreatedById);
            modelBuilder.Entity<Rate>().HasIndex(a => a.UpdatedById);
            modelBuilder.Entity<Rate>().Property(a => a.IsPassthrough).HasDefaultValue(false);


            modelBuilder.Entity<Rate>().Property(a => a.Price).HasPrecision(18, 2);

            modelBuilder.Entity<Project>()
                .HasOne(a => a.AcreageRate)
                .WithMany(p => p.Projects)
                .HasForeignKey(a => a.AcreageRateId)
                .OnDelete(DeleteBehavior.Restrict);

        }

        public class Types
        {
            public const string Acreage    = "Acreage";
            public const string Labor      = "Labor";
            public const string Equipment  = "Equipment";
            public const string Other      = "Other";
            public const string Adjustment = "Adjustment"; //Don't include this in the TypeList below or it will get added toi the UI dropdown

            public static List<string> TypeList = new List<string>
            {
                Acreage,
                Labor,
                Equipment,
                Other,
            }.ToList();
        }
    }
}
