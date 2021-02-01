using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.Settings
{
    public class AppSettings : BindableBase, ISerializableModel//, ISerializableModel<AppSettings>
    {
        public UserId UserId;

        public bool closeOnAoeExit;

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<AppSettings>(serialized);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
