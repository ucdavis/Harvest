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

        public Role Role { get; set; }

        public User User { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().HasIndex(a => a.RoleId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.UserId);
        }
    }
}
