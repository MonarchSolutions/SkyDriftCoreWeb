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
        /// �أ�ͭ��
        /// </summary>
        V = 10003,

        /// <summary>
        /// �أ�����
        /// </summary>
        T = 10002,

        /// <summary>
        /// �أ���
        /// </summary>
        S = 10001,

        /// <summary>
        /// ��
        /// </summary>
        A_Red = 9000,

        /// <summary>
        /// ��
        /// </summary>
        B_Green = 6000,

        /// <summary>
        /// ��
        /// </summary>
        C_Blue = 3000,
    }

    public enum EKlassBonus
    {
        /// <summary>
        /// �֤ä�����
        /// ңң����
        /// </summary>
        TopSpeed,

        /// <summary>
        /// ���դ�ɥ�ե�
        /// ����Ҳ��Ư�ƣ�
        /// </summary>
        Daily,

        /// <summary>
        /// ���ڥ�HIT
        /// �������
        /// </summary>
        ItemHit,

        /// <summary>
        /// ���¥ʥ�
        /// �Ų�մ��
        /// </summary>
        NoFall,

        /// <summary>
        /// ���`��ʹ���Ф�
        /// �����ľ�
        /// </summary>
        NoGuage,

        /// <summary>
        /// �Ω`����`��
        /// ����ͨ��
        /// </summary>
        NoDamage,

        /// <summary>
        /// ����ܞ
        /// ����ת
        /// </summary>
        SuperReversal,

        /// <summary>
        /// ��ܞ
        /// ��ת
        /// </summary>
        Reversal,

        /// <summary>
        /// ��ܞ��
        /// �ɵø�ˤ�ú�
        /// </summary>
        Tumble,

        /// <summary>
        /// �����˥󥰥��
        /// Winning Run
        /// </summary>
        WinningRun,

        /// <summary>
        /// �ޥ����ڥ륭�`��
        /// ��������
        /// </summary>
        HasSpell,

        /// <summary>
        /// �饹�ȥ�`�ɰk��
        /// ����Last Word
        /// </summary>
        LastWord,

        /// <summary>
        /// ����Ǥ�ᤲ�ʤ�
        /// ��������
        /// </summary>
        NeverGiveup,

        /// <summary>
        /// ���åɥ��å�
        /// ���CP
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
        /// ������
        /// 10000f
        /// </summary>
        Fairy = 10000,

        /// <summary>
        /// ���༶
        /// 100000f
        /// </summary>
        Human = 100000,

        /// <summary>
        /// ���ּ�
        /// 300000f
        /// </summary>
        Youkai = 300000,

        /// <summary>
        /// ��
        /// 1000000f
        /// </summary>
        God = 1000000,

        /// <summary>
        /// ���߼�
        /// 5000000f
        /// </summary>
        Magi = 5000000,

        /// <summary>
        /// ���߼�
        /// 1E+08f
        /// </summary>
        Phantasm = 100000000,
    }

    public enum Course
    {
        ��������� = 0,
        ħ��֮ɭ = 1,
        ����֮�� = 2,
        ��ħ�� = 3,
        ��;���� = 4,
        ����֮ɽ = 5,
        ʥ���� = 6,
        ����¥ = 7,
        ���ȵ����ż� = 8,
        ��֮�� = 9,
        �ɶ� = 10,
        ���������2 = 11,
        ��������� = 12,
        ����³ħ��ͼ��� = 13,
    }

    public enum Character
    {
        �������� = 0,
        ����ħ��ɳ = 1,
        ʮ��ҹ�Dҹ = 2,
        ��������˹������ = 3,
        ��������� = 4,
        ��ʸ������ = 5,
        �������� = 6,
        ���� = 7,
        �������� = 8,
        ������꼻�Ժ��� = 9,
        �����w = 10,
        �ﲿ���� = 11,
        ��¶ŵ = 12,
        ������а = 13,
        �������� = 14,
        ��ľ���� = 15,
        ������������ = 16,
        ������ = 17
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
                    return "����";
                case UserState.Online:
                    return "��������";
                case UserState.InRoom:
                    return "�����ڷ���";
                case UserState.InRace:
                    return "Ư����";
                case UserState.InMatch:
                    return "��λ��";
                default:
                    return "ʧ����";
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
            //selectList.Add(new SelectListItem { Text = "--��ѡ��--", Value = "",Selected = true});
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
                return "������";
            }
            var pi = Core.PlayerInfos[id];
            return GetCharacterName(pi.CharacterTake.GetMaxIndex());
        }

        public static string GetWinCharacter(int id)
        {
            if (!Core.PlayerInfos.ContainsKey(id))
            {
                return "������";
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
                return "������";
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
            return "������";
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
                    return "���� ����";
                case 1:
                    return "���� ħ��ɳ";
                case 2:
                    return "ʮ��ҹ �Dҹ";
                case 3:
                    return "�������ǡ�˹������";
                case 4:
                    return "����� ����";
                case 5:
                    return "��ʸ ������";
                case 6:
                    return "������ ��";
                case 7:
                    return "�� ��";
                case 8:
                    return "���� ����";
                case 9:
                    return "���ɡ���꼻�Ժ�����";
                case 10:
                    return "���� �w";
                case 11:
                    return "�ﲿ ����";
                case 12:
                    return "��¶ŵ";
                case 13:
                    return "���� ��а";
                case 14:
                    return "���� ����";
                case 15:
                    return "��ľ ����";
                case 16:
                    return "�������� ����";
                case 17:
                    return "���� ��";
                default:
                    return "������";
            }
        }

        public static string GetRankName(int rank)
        {
            if (rank == (int)Ranking.V)
            {
                return "ͭ";
            }
            if (rank == (int)Ranking.T)
            {
                return "��";
            }
            if (rank == (int)Ranking.S)
            {
                return "�����ߣ�";
            }
            if (rank >= (int)Ranking.A_Red)
            {
                return "��";
            }
            if (rank >= (int)Ranking.B_Green)
            {
                return "��";
            }

            return "�ȣ�";

        }

        public static string GetClassName(int klass)
        {
            if (klass < (int)EKlassName.Fairy)
            {
                return "������";
            }
            if (klass < (int)EKlassName.Human)
            {
                return "���༶";
            }
            if (klass < (int)EKlassName.Youkai)
            {
                return "���ּ�";
            }
            if (klass < (int)EKlassName.God)
            {
                return "��";
            }
            if (klass < (int)EKlassName.Magi)
            {
                return "���߼�";
            }
            return "���߼�";
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