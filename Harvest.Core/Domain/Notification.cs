using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(300)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(300)]
        public string Subject { get; set; }

        [Required]
        [MaxLength]
        public string Body { get; set; }

        [Required]
        public bool Sent { get; set; }

        public Project Project { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>().HasIndex(a => a.ProjectId);
        }
    }
}
