using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [StringLength(100)]
        public string Number { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [NotMapped]
        public string AccountManagerName { get; set; }

        [NotMapped]
        public string AccountManagerEmail { get; set; }

        public decimal Percentage { get; set; }

        public int? ApprovedById { get; set; }

        [Display(Name = "Approved On")]
        public DateTime? ApprovedOn { get; set; }

        [Display(Name = "Approved By")]
        public User ApprovedBy { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasIndex(a => a.Name);
            modelBuilder.Entity<Account>().HasIndex(a => a.Number);
            modelBuilder.Entity<Account>().HasIndex(a => a.ProjectId);

            modelBuilder.Entity<Account>().Property(a => a.Percentage).HasPrecision(18, 2);
        }
    }
}
