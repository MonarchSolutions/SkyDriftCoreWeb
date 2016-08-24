using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.ApiModels
{
    public class AuthoriseSerialModel
    {
        /// <summary>
        /// 输入不正确的错误码（mail，password），通过user_id返回
        /// </summary>
        public const int InvaildInputDetail = -460;
        /// <summary>
        /// 输入不正确的错误码，通过user_id返回
        /// </summary>
        public const int InvaildInput = -400;
        public string serial_no { get; set; }

        public string name { get; set; }

        public string account { get; set; }

        public string password { get; set; }
    }
}
