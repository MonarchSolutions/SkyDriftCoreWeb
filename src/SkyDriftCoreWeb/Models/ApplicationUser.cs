using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SkyDriftCoreWeb.Models
{
    public enum UserState
    {
        Offline = 0,
        Online = 1,
        InRoom = 2,
        InRace = 3,
        InMatch = 4,
    }

    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<int> //MARK: 主键变int
    {
        //Username=name

        //Email=account

        public string NickName { get; set; }

        public string Sign { get; set; }

        /// <summary>
        /// klass
        /// </summary>
        public long Class { get; set; } = 0;

        /// <summary>
        /// rank
        /// </summary>
        public int Rank { get; set; } = 0;

        /// <summary>
        /// color_unlock
        /// </summary>
        public string ColorUnlock { get; set; }

        /// <summary>
        /// serial_no, 无横线，16位
        /// </summary>
        public string SerialNo { get; set; }

        public DateTime? LoginTime { get; set; } = (DateTime) SqlDateTime.MinValue;

        public DateTime? RegisterTime { get; set; } = (DateTime)SqlDateTime.MinValue;


        public UserState State { get; set; } = UserState.Offline;

        public DateTime? LastActiveTime { get; set; } = (DateTime)SqlDateTime.MinValue;

        public DateTime? LastRaceTime { get; set; } = (DateTime)SqlDateTime.MinValue;

        public string AccessToken { get; set; }

        public int CurrentRoomId { get; set; } = -1;

        public string IP { get; set; }
    }
}
