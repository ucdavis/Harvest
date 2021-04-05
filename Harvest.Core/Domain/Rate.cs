﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Rate
    {
        [Key] public int Id { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(15)]
        public string Type { get; set; }
        [Required]
        [StringLength(250)]
        public string Description { get; set; }
        [StringLength(50)]
        public string BillingUnit { get; set; }
        [Required]
        [StringLength(50)]
        public string Account { get; set; }

        [Display(Name = "Rate")] 
        public decimal Price { get; set; }

        public DateTime? EffectiveOn { get; set; }

        [Required] 
        public int CreatedById { get; set; }
        [Required] 
        public int UpdatedById { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rate>().HasIndex(a => a.Type);
            modelBuilder.Entity<Rate>().HasIndex(a => a.Description);
            modelBuilder.Entity<Rate>().HasIndex(a => a.CreatedById);
            modelBuilder.Entity<Rate>().HasIndex(a => a.UpdatedById);


            modelBuilder.Entity<Rate>().Property(a => a.Price).HasPrecision(18, 2);
        }

        public class Types
        {
            public const string Acreage = "Acreage";
            public const string Labor = "Labor";
            public const string Equipment = "Equipment";
            public const string Other = "Other";
        }
    }
}
