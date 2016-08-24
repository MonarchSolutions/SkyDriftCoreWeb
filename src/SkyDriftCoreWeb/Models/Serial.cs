using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models
{
    public enum ActivateCodeError
    {
        NewItemGet = 1,
        AlreadyGot = 2,
        OverUsed = 3,
        Wrong = 4
    }

    public class Serial
    {
        [Key]
        public string Sn { get; set; }

        public bool IsCode { get; set; } = false;

        /// <summary>
        /// 用,分隔的数字编号
        /// </summary>
        public string UnlockColor { get; set; }

        public bool Available { get; set; } = true;

        public int MaxUse { get; set; } = 100;

        public int CurrentUse { get; set; } = 0;
    }
}
