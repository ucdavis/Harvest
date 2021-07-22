using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Models.History
{
    public class FileHistoryModel
    {
        public FileHistoryModel() { }

        public FileHistoryModel(AttachmentBase attachment)
        {
            Id = attachment.Id;
            FileName = attachment.FileName;
            ContentType = attachment.ContentType;
            FileSize = attachment.FileSize;
            CreatedById = attachment.CreatedById;
            CreatedBy = UserHistoryModel.CreateFrom(attachment.CreatedBy);
            CreatedOn = attachment.CreatedOn;
            Identifier = attachment.Identifier;
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int FileSize { get; set; }
        public int CreatedById { get; set; }
        public UserHistoryModel CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Identifier { get; set; }
    }
}
