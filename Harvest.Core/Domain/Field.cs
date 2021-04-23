using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Harvest.Core.Domain
{
    public class Field
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public string Crop { get; set; }
        public string Details { get; set; }
        public Polygon Location { get; set; }
        public bool IsActive { get; set; } = true;

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Field>().HasIndex(a => a.Crop);
            modelBuilder.Entity<Field>().HasIndex(a => a.ProjectId);
        }
    }

}
