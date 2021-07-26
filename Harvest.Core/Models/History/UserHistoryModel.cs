using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Models.History
{
    public class UserHistoryModel
    {
        public UserHistoryModel() { }

        public UserHistoryModel(User user)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            Iam = user.Iam;
            Kerberos = user.Kerberos;
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Iam { get; set; }
        public string Kerberos { get; set; }
    }
}
