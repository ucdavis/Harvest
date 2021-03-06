﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")] 
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")] 
        public string LastName { get; set; }

        [Required]
        [StringLength(300)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(10)]
        public string Iam { get; set; }

        [StringLength(20)]
        public string Kerberos { get; set; }

        public List<Permission> Permissions { get; set; }

        [Display(Name = "Name")]
        public string Name => FirstName + " " + LastName;

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(a => a.Kerberos);
            modelBuilder.Entity<User>().HasIndex(a => a.Iam);
            modelBuilder.Entity<User>().HasIndex(a => a.Email);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
