using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkyDriftCoreWeb.Models;

namespace SkyDriftCoreWeb.Controllers
{
    public enum RoomJoin
    {
        UseProxy = 3,
        UseNat = 2,
        Direct = 1,
        Unable = 0
    }

    public enum UpdateCheckResult
    {
        UpToDate = 1,
        UpdateAvailable = 2,
        Error = 3
    }

    public enum SerialCheckResult
    {
        OK = 1,
        Used = 2,
        Wrong = 3
    }

    public enum Ranking
    {
        /// <summary>
        /// 特（铜）
        /// </summary>
        V = 10003,

        /// <summary>
        /// 特（银）
        /// </summary>
        T = 10002,

        /// <summary>
        /// 特（金）
        /// </summary>
        S = 10001,

        /// <summary>
        /// 武
        /// </summary>
        A_Red = 9000,

        /// <summary>
        /// 中
        /// </summary>
        B_Green = 6000,

        /// <summary>
        /// 稳
        /// </summary>
        C_Blue = 3000,
    }

    public enum EKlassBonus
    {
        /// <summary>
        /// ぶっちぎり
        /// 遥遥领先
        /// </summary>
        TopSpeed,

        /// <summary>
        /// 今日もドリフト
        /// 今天也在漂移！
        /// </summary>
        Daily,

        /// <summary>
        /// スペルHIT
        /// 符卡打击
        /// </summary>
        ItemHit,

        /// <summary>
        /// 落下ナシ
        /// 脚不沾地
        /// </summary>
        NoFall,

        /// <summary>
        /// ゲージ使い切り
        /// 电量耗尽
        /// </summary>
        NoGuage,

        /// <summary>
        /// ノーダメージ
        /// 无损通关
        /// </summary>
        NoDamage,

        /// <summary>
        /// 大逆転
        /// 大逆转
        /// </summary>
        SuperReversal,

        /// <summary>
        /// 逆転
        /// 逆转
        /// </summary>
        Reversal,

        /// <summary>
        /// 大転落
        /// 飞得高摔得狠
        /// </summary>
        Tumble,

        /// <summary>
        /// ウィニングラン
        /// Winning Run
        /// </summary>
        WinningRun,

        /// <summary>
        /// マイスペルキープ
        /// 留有王牌
        /// </summary>
        HasSpell,

        /// <summary>
        /// ラストワード発動
        /// 发动Last Word
        /// </summary>
        LastWord,

        /// <summary>
        /// 最後でもめげない
        /// 永不言弃
        /// </summary>
        NeverGiveup,

        /// <summary>
        /// グッドタッグ
        /// 最佳CP
        /// </summary>
        GoodTag,

        /// <summary>
        ///
        /// </summary>
        Max
    }



    public enum EKlassName : long
    {
        /// <summary>
        /// 妖精级
        /// 10000f
        /// </summary>
        Fairy = 10000,

        /// <summary>
        /// 人类级
        /// 100000f
        /// </summary>
        Human = 100000,

        /// <summary>
        /// 妖怪级
        /// 300000f
        /// </summary>
        Youkai = 300000,

        /// <summary>
        /// 神级
        /// 1000000f
        /// </summary>
        God = 1000000,

        /// <summary>
        /// 贤者级
        /// 5000000f
        /// </summary>
        Magi = 5000000,

        /// <summary>
        /// 幻走级
        /// 1E+08f
        /// </summary>
        Phantasm = 100000000,
    }

    public enum Course
    {
        马里奥赛车 = 0,
        魔法之森 = 1,
        人类之村 = 2,
        红魔馆 = 3,
        迷途竹林 = 4,
        妖怪之山 = 5,
        圣辇船 = 6,
        白玉楼 = 7,
        灼热地狱遗迹 = 8,
        雾之湖 = 9,
        旧都 = 10,
        马里奥赛车2 = 11,
        外面的世界 = 12,
        巴瓦鲁魔法图书馆 = 13,
    }

    public enum Character
    {
        博丽灵梦 = 0,
        雾雨魔理沙 = 1,
        十六夜咲夜 = 2,
        蕾米莉亚斯卡雷特 = 3,
        东风谷早苗 = 4,
        洩矢诹访子 = 5,
        古明地恋 = 6,
        秦心 = 7,
        魂魄妖梦 = 8,
        铃仙优昙华院因幡 = 9,
        封兽鵺 = 10,
        物部布都 = 11,
        琪露诺 = 12,
        鬼人正邪 = 13,
        伊吹萃香 = 14,
        茨木华扇 = 15,
        比那名居天子 = 16,
        八云紫 = 17
    }

    public enum SkyError
    {
        /// <summary>
        /// OK
        /// </summary>
        OK = 200,
        Maintenance = 503,

        /// <summary>
        /// BadRequest
        /// </summary>
        InvaildInput = 400,

        /// <summary>
        /// Unauthorized
        /// </summary>
        AuthError = 401,
        ConnectionError = 470,
        OldVersionOrRoomNotExists = -4,
        SerialUsed = 2,
        SerialInvaildInput = 3,

    }

    public static class Helper
    {
        public const int CourseCount = 14;
        public const int CharacterCount = 18;

        public static string GetState(ApplicationUser user)
        {
            switch (user.State)
            {
                case UserState.Offline:
                    return "离线";
                case UserState.Online:
                    return "可能在线";
                case UserState.InRoom:
                    return "可能在房间";
                case UserState.InRace:
                    return "漂移中";
                case UserState.InMatch:
                    return "排位中";
                default:
                    return "失踪中";
            }
        }

        public static Task<int> SaveChangesAsyncLock(this DbContext context)
        {
#if SQLITE
            return context.SaveChangesAsync();
            //lock (Core.DatabaseLock)
            //{
            //    var r = context.SaveChanges();
            //    return Task.FromResult(r);
            //}
#else
            return context.SaveChangesAsync();
#endif
        }

        public static Task<IdentityResult> UpdateAsyncLock<T>(this UserManager<T> userManager, T user)
            where T : ApplicationUser
        {
#if SQLITE
            return userManager.UpdateAsync(user);
#else
            return userManager.UpdateAsync(user);
#endif
        }

        public static string TimeToString(int time)
        {
            if (time <= 0)
            {
                return "-";
            }
            return TimeSpan.FromMilliseconds(time).ToString("g");
        }

        public static List<SelectListItem> GetSelectList(Type enumType)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            //selectList.Add(new SelectListItem { Text = "--请选择--", Value = "",Selected = true});
            foreach (int e in Enum.GetValues(enumType))
            {
                selectList.Add(new SelectListItem { Text = GetCourseName(e), Value = e.ToString() });
            }
            return selectList;
        }

        public static string GetFavoriteCharacter(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return "？？？";
            }
            var pi = Core.PlayerInfos[id];
            return GetCharacterName(pi.CharacterTake.GetMaxIndex());
        }

        public static string GetWinCharacter(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return "？？？";
            }
            var pi = Core.PlayerInfos[id];
            return GetCharacterName(pi.CharacterWin.GetMaxIndex());
        }

        //public static int[] GetCharacterUsage(int id)
        //{

        //}

        public static int GetWinCourseId(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return 1;
            }
            var pi = Core.PlayerInfos[id];
            int winCount = -1;
            int index = 0;
            for (int i = 0; i < pi.CourseTake.Length; i++)
            {
                if (pi.CourseTake[i] <= 0)
                {
                    continue;
                }

                if (pi.CourseWin[i] > winCount)
                {
                    index = i;
                    winCount = pi.CourseWin[i];
                }
            }
            return index + 1;
        }

        public static int GetLoseCourseId(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return 1;
            }
            var pi = Core.PlayerInfos[id];
            int winCount = pi.CourseTake.Length > 0 ? pi.CourseTake[0] : 99999;
            int index = 0;
            for (int i = 0; i < pi.CourseTake.Length; i++)
            {
                if (pi.CourseTake[i] <= 0)
                {
                    continue;
                }

                if (pi.CourseWin[i] < winCount)
                {
                    index = i;
                    winCount = pi.CourseWin[i];
                }
            }
            return index + 1;
        }

        public static double[] GetCharacterWinRate(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return new double[CharacterCount];
            }
            var pi = Core.PlayerInfos[id];
            double[] rate = new double[CharacterCount];
            for (int i = 0; i < CharacterCount; i++)
            {
                if (pi.CharacterTake[i] <= 0)
                {
                    rate[i] = 0.0;
                }
                else
                {
                    var r = pi.CharacterWin[i] / (double)pi.CharacterTake[i];
                    rate[i] = r;
                }
            }
            return rate;
        }

        public static string[] GetCourseWinRate(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return new string[CourseCount];
            }
            var pi = Core.PlayerInfos[id];
            string[] rate = new string[CourseCount];
            for (int i = 0; i < CourseCount; i++)
            {
                if (pi.CourseTake[i] <= 0)
                {
                    rate[i] = "0%";
                }
                else
                {
                    var r = pi.CourseWin[i] / (double)pi.CourseTake[i];
                    rate[i] = r.ToString("P2");
                }
            }
            return rate;
        }

        public static string GetWinCourse(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return "？？？";
            }
            var pi = Core.PlayerInfos[id];
            return GetCourseName(pi.CourseWin.GetMaxIndex());
        }

        public static int GetMaxIndex(this int[] array)
        {
            var m = array.Max();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == m)
                {
                    return i;
                }
            }
            return 0;
        }

        public static int GetMinIndex(this int[] array)
        {
            var m = array.Min();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == m)
                {
                    return i;
                }
            }
            return 0;
        }

        public static string GetCourseName(Course c)
        {
            return c.ToString();
        }

        public static string GetCourseName(int c)
        {
            if (Enum.IsDefined(typeof(Course), c))
            {
                return GetCourseName((Course)c);
            }
            return "？？？";
        }

        public static string GetCharacterName(Character c)
        {
            return GetCharacterName((int)c);
        }

        public static string GetCharacterName(int c)
        {
            switch (c)
            {
                case 0:
                    return "博丽 灵梦";
                case 1:
                    return "雾雨 魔理沙";
                case 2:
                    return "十六夜 咲夜";
                case 3:
                    return "蕾米莉亚·斯卡雷特";
                case 4:
                    return "东风谷 早苗";
                case 5:
                    return "洩矢 诹访子";
                case 6:
                    return "古明地 恋";
                case 7:
                    return "秦 心";
                case 8:
                    return "魂魄 妖梦";
                case 9:
                    return "铃仙·优昙华院·因幡";
                case 10:
                    return "封兽 鵺";
                case 11:
                    return "物部 布都";
                case 12:
                    return "琪露诺";
                case 13:
                    return "鬼人 正邪";
                case 14:
                    return "伊吹 萃香";
                case 15:
                    return "茨木 华扇";
                case 16:
                    return "比那名居 天子";
                case 17:
                    return "八云 紫";
                default:
                    return "？？？";
            }
        }

        public static string GetRankName(int rank)
        {
            if (rank == (int)Ranking.V)
            {
                return "铜";
            }
            if (rank == (int)Ranking.T)
            {
                return "银";
            }
            if (rank == (int)Ranking.S)
            {
                return "金（王者）";
            }
            if (rank >= (int)Ranking.A_Red)
            {
                return "武";
            }
            if (rank >= (int)Ranking.B_Green)
            {
                return "中";
            }

            return "稳！";

        }

        public static string GetClassName(int klass)
        {
            if (klass < (int)EKlassName.Fairy)
            {
                return "妖精级";
            }
            if (klass < (int)EKlassName.Human)
            {
                return "人类级";
            }
            if (klass < (int)EKlassName.Youkai)
            {
                return "妖怪级";
            }
            if (klass < (int)EKlassName.God)
            {
                return "神级";
            }
            if (klass < (int)EKlassName.Magi)
            {
                return "贤者级";
            }
            return "幻走级";
        }
        public static IQueryable<ApplicationUser> GetOnlineUsers(this UserManager<ApplicationUser> userManager)
        {
            return from u in userManager.Users
                   where u.State != UserState.Offline
                   select u;
        }

        public static Task<ApplicationUser> GetUserByIdAsync(this UserManager<ApplicationUser> userManager, int id)
        {
            return userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public static string GetClientIP(this HttpContext context)
        {
            return context != null ? context.Connection.RemoteIpAddress.ToString() : "";
        }

        public static Task<ApplicationUser> GetUserAsync(this UserManager<ApplicationUser> userManager, string account)
        {
            return userManager.FindByNameAsync(account);
        }

        public static Task<ApplicationUser> GetUserByTokenAsync(this UserManager<ApplicationUser> userManager, string token)
        {
            return userManager.Users.FirstOrDefaultAsync(u => u.AccessToken == token && !string.IsNullOrEmpty(u.AccessToken));
        }

        public static void AddHeader(this HttpContext context, string status)
        {
            context.Response.Headers["STATUS"] = status;
        }

        public static void AddHeader(this HttpContext context, SkyError status)
        {
            string code;
            switch (status)
            {
                case SkyError.OK:
                    code = "200 OK";
                    break;
                case SkyError.Maintenance:
                    code = "503";
                    break;
                case SkyError.InvaildInput:
                    code = "400";
                    break;
                case SkyError.AuthError:
                    code = "401";
                    break;
                case SkyError.ConnectionError:
                    code = "470";
                    break;
                case SkyError.OldVersionOrRoomNotExists:
                    code = "004";
                    break;
                case SkyError.SerialUsed:
                    code = "002";
                    break;
                case SkyError.SerialInvaildInput:
                    code = "003";
                    break;
                default:
                    code = "999";
                    break;
            }

            context.Response.Headers["STATUS"] = code;
        }
    }
}