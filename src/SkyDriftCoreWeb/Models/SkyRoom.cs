using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models
{
    public class SkyRoom
    {
        //[DataMember] 更名？

        /// <summary>
        /// room_id
        /// </summary>
        [Key]
        public int RoomId
        { get; set; }

        public bool IsMatch { get; set; }

        /// <summary>
        /// room_name
        /// </summary>
        public string RoomName
        { get; set; }
        //[ReadOnly(true)]
        /// <summary>
        /// racing_now
        /// </summary>
        public bool IsRacing
        { get; set; }

        /// <summary>
        /// password_flg
        /// </summary>
        public bool HasPassword
        { get; set; }

        /// <summary>
        /// !deny_watching_flg
        /// </summary>
        public bool CanWatch
        { get; set; }
        [StringLength(12, MinimumLength = 0)]
        public string Password
        { get; set; }
        //[ReadOnly(true)]
        public string HostIp
        { get; set; }

        /// <summary>
        /// use_port
        /// </summary>
        public int UsePort
        { get; set; } = 12900;

        public int HostId { get; set; }

        /// <summary>
        /// host_guid
        /// </summary>
        public string HostGuid
        { get; set; }

        /// <summary>
        /// cli_version
        /// </summary>
        public string CliVersion
        { get; set; }
        //[ReadOnly(true)]
        public int PlayerNum
        { get; set; }
        //[ReadOnly(true)]
        public int WatcherNum
        { get; set; }
        //[ReadOnly(true)]
        public DateTime SetupTime { get; set; } = (DateTime) SqlDateTime.MinValue;

        public int RaceCount { get; set; } = 0;

        public string Players { get; set; }

        //[NotMapped]
        //MARK: [NotMapped]标签指定EF不将此属性存入数据库
    }
}
