using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class ProjectAttachment : AttachmentBase
    {
        public int ProjectId { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectAttachment>().HasIndex(a => a.ProjectId);
            modelBuilder.Entity<ProjectAttachment>().HasIndex(a => a.CreatedOn);
            modelBuilder.Entity<ProjectAttachment>().HasIndex(a => a.CreatedById);
            modelBuilder.Entity<ProjectAttachment>().Property(a => a.FileName).HasMaxLength(250);
            modelBuilder.Entity<ProjectAttachment>().Property(a => a.ContentType).HasMaxLength(250);
            modelBuilder.Entity<ProjectAttachment>().Property(a => a.Identifier).HasMaxLength(128);
        }
    }
}
