using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Harvest.Core.Domain
{
    public class ProjectPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        [StringLength(20)]
        public string Permission { get; set; } = Role.Codes.ProjectViewer;

        internal static void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectPermission>().HasIndex(a => a.ProjectId);
            modelBuilder.Entity<ProjectPermission>().HasIndex(a => a.UserId);
        }
    }
}
