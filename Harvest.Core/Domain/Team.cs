using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Team Slug")]
        [Required]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Slug must be between 3 and 40 characters")]
        [RegularExpression(SlugRegex,
            ErrorMessage = "Slug may only contain lowercase alphanumeric characters or single hyphens, and cannot begin or end with a hyphen")]
        public string Slug { get; set; }

        public const string SlugRegex = "^([a-z0-9]+[a-z0-9\\-]?)+[a-z0-9]$";

        public List<Permission> Permissions { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>()
                .HasOne(q => q.Team)
                .WithMany(t => t.Permissions)
                .HasForeignKey(p => p.TeamId);

            modelBuilder.Entity<Project>()
                .HasOne(q => q.Team)
                .WithMany()
                .HasForeignKey(p => p.TeamId);
        }
    }
}
