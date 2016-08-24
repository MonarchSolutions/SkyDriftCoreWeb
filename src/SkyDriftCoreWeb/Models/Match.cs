using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models
{
    public class Match
    {
        [Key]
        public int Id { get; set; }

        public int RoomId { get; set; }

        public int UserId { get; set; }

        public int Order { get; set; }

        public int CourseId { get; set; }

        public int Character1 { get; set; }

        public int Character2 { get; set; }

        public int Class { get; set; }

        public DateTime? Time { get; set; } = (DateTime) SqlDateTime.MinValue;

    }
}
