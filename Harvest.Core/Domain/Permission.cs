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

        // Self-referencing many-to-many for parent permissions
        public List<Permission> Parents { get; set; } = new();

        // Optional: Children navigation for easier traversal
        public List<Permission> Children { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().HasIndex(a => a.RoleId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.TeamId);

            // Self-referencing many-to-many relationship
            modelBuilder.Entity<Permission>()
                .HasMany(p => p.Parents)
                .WithMany(p => p.Children)
                .UsingEntity<Dictionary<string, object>>(
                    "PermissionParent",
                    j => j
                        .HasOne<Permission>()
                        .WithMany()
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j
                        .HasOne<Permission>()
                        .WithMany()
                        .HasForeignKey("ChildId")
                        .OnDelete(DeleteBehavior.Cascade)
                );
        }
    }
}
