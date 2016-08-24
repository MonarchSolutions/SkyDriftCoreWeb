using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.StaticsModels
{
    public class StateModel
    {
        public int OnlineUsersCount { get; set; }
        public int OfflineUsersCount { get; set; }
        public int MatchingUsersCount { get; set; }
        public int InRoomUsersCount { get; set; }
        public int RacingUsersCount { get; set; }
    }
}
