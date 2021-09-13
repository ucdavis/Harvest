using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class CropLookup
    {
        [Key] public int Id { get; set; }
        /// <summary>
        /// From Project.CropTypes
        /// </summary>
        [Required] [StringLength(50)] public string Type { get; set; }
        [Required] [StringLength(50)] public string Crop { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CropLookup>().HasIndex(a => a.Type);
            modelBuilder.Entity<CropLookup>().HasIndex(a => a.Crop);
        }
    }

}
