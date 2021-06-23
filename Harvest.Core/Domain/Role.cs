using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasIndex(a => a.Name).IsUnique();

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Role)
                .WithMany()
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public class Codes
        {
            public const string System = "System";
            public const string Admin = "Admin";
            public const string FieldManager = "FieldManager";
            public const string Supervisor = "Supervisor";
            public const string Worker = "Worker";
            public const string PI = "PI";
        }
    }
}
