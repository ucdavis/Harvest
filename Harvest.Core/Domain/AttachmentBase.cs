using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harvest.Core.Domain
{
    public abstract class AttachmentBase
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(250)]
        public string FileName { get; set; }
        [StringLength(250)]
        public string ContentType { get; set; }
        public int FileSize { get; set; }
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [NotMapped]
        public string SasLink { get; set; }
        [Required]
        [StringLength(128)]
        public string Identifier { get; set; }
    }
}
