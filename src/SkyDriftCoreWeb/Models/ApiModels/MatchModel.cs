using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.ApiModels
{
    public class MatchModel
    {
        public string access_token { get; set; }

        public int use_port { get; set; }

        public string cli_version { get; set; }

        public int? client_only { get; set; }
    }
}
