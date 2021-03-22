using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class TransferRequest
    {
        public TransferRequest()
        {
            History = new List<TransferHistory>();
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Sloth Transaction Id")]
        public Guid? SlothTransactionId { get; set; }

        [Display(Name = "Kfs Tracking Number")]
        [StringLength(20)] //It is 10 in sloth, but just in case...
        public string KfsTrackingNumber { get; set; }

        [StringLength(40)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [Display(Name = "Requested On")]
        public DateTime RequestedOn { get; set; }

        public bool IsDeleted { get; set; }

        public int RequestedById { get; set; }

        [Display(Name = "Requested By")]
        public User RequestedBy { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public IList<TransferHistory> History { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransferRequest>().HasIndex(a => a.ProjectId);
            modelBuilder.Entity<TransferRequest>().HasIndex(a => a.RequestedById);

            modelBuilder.Entity<TransferRequest>()
                .HasQueryFilter(a => a.IsDeleted == false)
                .HasMany(b => b.History)
                .WithOne()
                .HasForeignKey(t => t.TransferId)
                .OnDelete(DeleteBehavior.Restrict);
        }

   }
}
