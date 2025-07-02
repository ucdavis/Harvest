using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class ProjectHistory
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string Action { get; set; }

        [MaxLength]
        public string Description { get; set; }

        [MaxLength]
        public string Details { get; set; }

        [Required]
        [Display(Name = "Action Date")]
        public DateTime ActionDate { get; set; }

        public int? ActorId { get; set; }

        public User Actor { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public bool DisplayForPi { get; set; } = false;
        

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectHistory>().HasIndex(a => a.ProjectId);
            modelBuilder.Entity<ProjectHistory>().HasIndex(a => a.DisplayForPi);
        }
    }
}
