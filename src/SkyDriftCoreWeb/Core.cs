using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SkyDriftCoreWeb.Controllers;
using SkyDriftCoreWeb.Models;

namespace SkyDriftCoreWeb
{
    public static class Core
    {
        //public static SemaphoreSlim DatabaseLock2 = new SemaphoreSlim(1);
        public static object DatabaseLock = new object();
        private const string ConfigPath = "SkyConfig.json";
        private static GC _gc;
        public static Config Config = new Config();
        public static UserManager<ApplicationUser> UserManager = null;
        public static ConcurrentDictionary<int,PlayerInfo> PlayerInfos = new ConcurrentDictionary<int, PlayerInfo>();
        public static ConcurrentDictionary<int,RoomJoin> RoomTracer = new ConcurrentDictionary<int, RoomJoin>();
        //public static object RankLock = new object();
        //public static List<ApplicationUser> RankList = new List<ApplicationUser>();
        public static readonly ConcurrentDictionary<string, int> OnlineUsers = new ConcurrentDictionary<string, int>();

        public static async Task<string> CheckUpdate(int version)
        {
            //TODO: Can implement update if needed
            return "";
        }
        
        public static void Init()
        {
            if (!File.Exists(ConfigPath))
            {
                Config = new Config();
                Config.Save(ConfigPath);
            }
            else
            {
                Config = Config.Load(ConfigPath);
            }
        }

        public static void StartTasks()
        {
            _gc = new GC(Config.MaximumRoomTime, 0.5);
        }

        public static void StopTasks()
        {
            _gc.Dispose();
        }
    }
}
