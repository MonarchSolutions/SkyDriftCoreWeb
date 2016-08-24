using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.ApiModels
{
    public class RecordModel
    {
        public string access_token { get; set; }

        public int course_id { get; set; }

        public int charcter1_id { get; set; }

        public int charcter2_id { get; set; }

        public int rep_version { get; set; }

        public int time0 { get; set; }

        public int time1 { get; set; }

        public int time2 { get; set; }

        public int totaltime { get; set; }
    }
}
