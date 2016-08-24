using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyDriftCoreWeb.Controllers;

namespace SkyDriftCoreWeb.Models
{
    public class PlayerInfo
    {
        public readonly int Id;
        public int TotalMatch = 0;
        public int[] CharacterTake = new int[Helper.CharacterCount];
        public int[] CharacterWin = new int[Helper.CharacterCount];
        public int[] CourseWin = new int[Helper.CourseCount];
        public int[] CourseTake = new int[Helper.CourseCount];
        //public ConcurrentDictionary<int, int> CharacterUse = new ConcurrentDictionary<int, int>();
        //public ConcurrentDictionary<int, int> CharacterWin = new ConcurrentDictionary<int, int>();
        //public ConcurrentDictionary<int, int> CourseWin = new ConcurrentDictionary<int, int>();
        public PlayerInfo(int id)
        {
            Id = id;
        }

        
    }
}
