using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
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
        [JsonIgnore]
        public List<Permission> Parents { get; set; } = new();

        // Optional: Children navigation for easier traversal
        [JsonIgnore]
        public List<Permission> Children { get; set; } = new();

#nullable enable
        [MaxLength(32)]
        public byte[]? Hash { get; set; }
        [MaxLength(16)]
        public byte[]? Salt { get; set; }
        [MaxLength(32)]
        public byte[]? Lookup { get; set; }

        public Guid? Token { get; set; }

        public DateTime? TokenExpires { get; set; }

#nullable disable

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().HasIndex(a => a.RoleId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.TeamId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.Lookup);

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
