using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Harvest.Core.Domain
{
    public class TeamDetail
    {
        [Key]
        public int Id { get; set; }

        [Required] 
        public int TeamId { get; set; } = 1;

        public Team Team { get; set; }

        [StringLength(128)] //Probably doesn't need to be this big...
        public string SlothApiKey { get; set; }
        [MaxLength(50)]
        public string SlothSource { get; set; } = "Harvest Recharge"; //This is configurable in sloth, but it can probably remain the same for all teams

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamDetail>().Property(a => a.TeamId).HasDefaultValue(1);
            //Team can only have 1 teamDetail and team detail can only have 1 team
            modelBuilder.Entity<TeamDetail>().HasOne(a => a.Team).WithOne(a => a.TeamDetail).HasForeignKey<TeamDetail>(a => a.TeamId);
        }
    }
}
