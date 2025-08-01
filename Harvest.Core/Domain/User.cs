﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
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

        [JsonIgnore]
        public List<Permission> Permissions { get; set; }

        [JsonIgnore]
        public List<Project> CreatedProjects { get; set; }

        [JsonIgnore]
        public List<Rate> CreatedRates { get; set; }

        [JsonIgnore]
        public List<Rate> UpdatedRates { get; set; }

        [JsonIgnore]
        public List<Project> PrincipalInvestigatorProjects { get; set; }

        [JsonIgnore]
        public List<TicketAttachment> TicketAttachments { get; set; }
        [JsonIgnore]
        public List<TicketMessage> TicketMessages { get; set; }

        [JsonIgnore]
        public List<ProjectPermission> ProjectPermissions { get; set; }

        [Display(Name = "Name")]
        public string Name => FirstName + " " + LastName;

        public string NameAndEmail => $"{FirstName} {LastName} ({Email})";

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(a => a.Kerberos);
            modelBuilder.Entity<User>().HasIndex(a => a.Iam).IsUnique();
            modelBuilder.Entity<User>().HasIndex(a => a.Email);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.PrincipalInvestigator)
                .WithMany(u => u.PrincipalInvestigatorProjects)
                .HasForeignKey(p => p.PrincipalInvestigatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rate>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.CreatedRates)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rate>()
                .HasOne(p => p.UpdatedBy)
                .WithMany(u => u.UpdatedRates)
                .HasForeignKey(p => p.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketAttachment>()
                .HasOne(a => a.CreatedBy)
                .WithMany(a => a.TicketAttachments)
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TicketMessage>()
                .HasOne(a => a.CreatedBy)
                .WithMany(a => a.TicketMessages)
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProjectPermission>()
                .HasOne(a => a.User)
                .WithMany(a => a.ProjectPermissions)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
