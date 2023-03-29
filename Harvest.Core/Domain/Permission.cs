using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int UserId { get; set; }
        
        /// <summary>
        /// Optional for cross-team roles (currently only the "system" role)
        /// </summary>
        public int? TeamId { get; set; }

        public Role Role { get; set; }

        public User User { get; set; }

        public Team Team { get; set; }
        
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().HasIndex(a => a.RoleId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.TeamId);
        }
    }
}
