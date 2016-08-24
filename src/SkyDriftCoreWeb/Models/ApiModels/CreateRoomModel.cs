using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.ApiModels
{
    public class CreateRoomModel
    {
        public string access_token { get; set; }

        public string room_name { get; set; }

        public int use_port { get; set; }

        public string cli_version { get; set; }

        public int password_flg { get; set; }= 0;

        public int deny_watching_flg { get; set; } = 0;
    }
}
