using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Crop
    {
        [Key] public int Id { get; set; }
        /// <summary>
        /// From Project.CropTypes
        /// </summary>
        [Required] [StringLength(50)] public string Type { get; set; }
        [Required] [StringLength(50)] public string Name { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Crop>().HasIndex(a => a.Type);
            modelBuilder.Entity<Crop>().HasIndex(a => a.Name);
        }
    }

}
