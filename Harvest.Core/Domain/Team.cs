using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Harvest.Core.Domain
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Display(Name = "Team Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Team Slug")]
        [Required]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Slug must be between 3 and 40 characters")]
        [RegularExpression(SlugRegex,
            ErrorMessage = "Slug may only contain lowercase alphanumeric characters or single hyphens, and cannot begin or end with a hyphen")]
        public string Slug { get; set; }

        public const string SlugRegex = "^([a-z0-9]+[a-z0-9\\-]?)+[a-z0-9]$";

        [StringLength(256)]
        public string Description { get; set; }

        public int TeamDetailId { get; set; }
        public TeamDetail TeamDetail { get; set; }

        [JsonIgnore]
        public IList<Permission> Permissions { get; set; }
        
        public void AddUserToRoleInTeam(User user, Role role, Team team)
        {
            Permissions.Add(new Permission()
            {
                Role = role,
                User = user,
                Team = team,
            });
        }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .ToTable("Teams")
                .HasIndex(t => t.Name).IsUnique();

            modelBuilder.Entity<Team>()
                .HasIndex(t => t.Slug).IsUnique();
        }
    }
}
