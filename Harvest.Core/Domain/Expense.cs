using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string Type { get; set; }
        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        [Display(Name = "Rate")] 
        [Range(0.01, Double.MaxValue, ErrorMessage = "Rate must be greater than 0.01")]
        public decimal Price { get; set; }

        [Range(0.01, Double.MaxValue, ErrorMessage = "Quantity must be greater than 0.01")]
        public decimal Quantity { get; set; }

        [Range(0.01, Double.MaxValue, ErrorMessage = "Total must be greater than 0.01")]
        public decimal Total { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [Required]
        public int RateId { get; set; }

        public Rate Rate { get; set; }

        public int? InvoiceId { get; set; }

        public Invoice Invoice { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? CreatedById { get; set; }

        public User CreatedBy { get; set; }

        [Required]
        [StringLength(50)]
        public string Account { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>().HasIndex(a => a.ProjectId);

            modelBuilder.Entity<Expense>().Property(a => a.Total).HasPrecision(18, 2);
        }
    }
}
