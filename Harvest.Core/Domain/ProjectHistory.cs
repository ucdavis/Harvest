using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class ProjectHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [StringLength(200)]
        public string Action { get; set; }

        [MaxLength]
        public string Description { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(20)]
        public int Actor { get; set; }

        [StringLength(50)]
        [Display(Name = "Actor Name")]
        public string ActorName { get; set; }

        [Required]
        [Display(Name = "Action Date")]
        public DateTime ActionDate { get; set; }

        public Project Project { get; set; }
    }
}
