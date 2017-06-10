using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.ApiModels
{
    public class RegisterGuidModel
    {
        public int? matching_id { get; set; }

        public int? room_id { get; set; }

        public string access_token { get; set; }

        public string guid { get; set; }

        public int not_use_guid { get; set; }

        public int? use_proxy { get; set; }
        public string proxy_ip { get; set; }
        public int? proxy_port { get; set; }
    }
}
