using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.ApiModels
{
    public class RaceResultModel
    {
        public string access_token { get; set; }

        public int matching_id { get; set; }

        public int order { get; set; }

        public int course_id { get; set; }

        public int charcter1_id { get; set; }

        public int charcter2_id { get; set; }

        public int klass { get; set; }

    }
}
