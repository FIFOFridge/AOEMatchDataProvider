using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.User
{
    public class UserGameProfileId
    {
        public string ProfileId { get; set; }

        public override string ToString()
        {
            return $"Profile ID: {ProfileId}";
        }
    }
}
