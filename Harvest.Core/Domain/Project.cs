using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Harvest.Core.Domain
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TeamId { get; set; }
        
        public DateTime Start { get; set; }
        
        public DateTime End { get; set; }
        
        [StringLength(50)]
        public string Crop { get; set; }
        
        [MaxLength]
        public string Requirements { get; set; }
        
        [StringLength(200)]
        public string Name { get; set; }
        
        public int PrincipalInvestigator { get; set; }
        
        public Geometry Location { get; set; }
        
        [StringLength(50)]
        public string LocationCode { get; set; }
        
        public int QuoteId { get; set; }
        
        // public decimal AcreagePerMonth  { get; set; }
        
        [Required]
        [Display(Name = "Quote Total")]
        public decimal QuoteTotal { get; set; }
        
        [Required]
        [Display(Name = "Charged Total")]
        public decimal ChargedTotal { get; set; }
        
        [Required]
        public int CreatedById { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; }
        
        [Required]
        [Display(Name = "Current Account Version")]
        public int CurrentAccountVersion { get; set; }
        
        [Required]
        // need or can we filter?
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public Team Team { get; set; }

        [Display(Name = "Created By")]
        public User CreatedBy { get; set; }

        public List<Account> Accounts { get; set; }

        public List<Quote> Quotes { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasIndex(a => a.Name);
            modelBuilder.Entity<Project>().HasIndex(a => a.TeamId);
            modelBuilder.Entity<Project>().HasIndex(a => a.CreatedById);
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

        }
    }
}
