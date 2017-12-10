using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyDriftCoreWeb.Controllers;
using SkyDriftCoreWeb.Data;
using SkyDriftCoreWeb.Models;

namespace SkyDriftCoreWeb
{
    public class GC : IDisposable
    {
        private bool _superClean = true;
        private Timer _roomCleaner;
        private Timer _userKicker;
        private Timer _statisticianShort;
        private readonly SkyDriftDbContext _db = new SkyDriftDbContext();

        public GC(double roomCleanDuration, double userKickDuration)
        {
            _roomCleaner = new Timer(CleanRoom, null, 0,
                (int)TimeSpan.FromHours(roomCleanDuration).Duration().TotalMilliseconds);
            //_userKicker = new Timer(KickUser, null, 10000,
            //    (int)TimeSpan.FromHours(userKickDuration).Duration().TotalMilliseconds);
            _userKicker = new Timer(KickUser, null, 15000, Timeout.Infinite);
            _statisticianShort = new Timer(ShortStatics, null, 10000, (int)TimeSpan.FromMinutes(5).TotalMilliseconds);
        }

        public void Dispose()
        {
            _roomCleaner.Dispose();
            _userKicker.Dispose();
            _statisticianShort.Dispose();
        }

        /// <summary>
        /// 五分钟更新一次
        /// </summary>
        /// <param name="o"></param>
        private async void ShortStatics(object o)
        {
            //lock (Core.RankLock)
            //{
            //    try
            //    {
            //        Core.RankList = (from u in Core.UserManager.Users
            //                         orderby u.Class descending
            //                         select u).AsEnumerable().ToList();
            //    }
            //    catch (Exception)
            //    {
            //    }
            //    if (Core.RankList == null)
            //    {
            //        Core.RankList = new List<ApplicationUser>();
            //    }
            //}
            var users = await Core.UserManager.Users.ToListAsync();
            foreach (var player in users)
            {
                PlayerInfo pi;
                if (Core.PlayerInfos.ContainsKey(player.Id))
                {
                    pi = Core.PlayerInfos[player.Id];
                }
                else
                {
                    pi = new PlayerInfo(player.Id);
                    Core.PlayerInfos[player.Id] = pi;
                }
                var matches = from match in _db.Matches
                              where match.UserId == player.Id
                              select match;
                if (!matches.Any())
                {
                    continue;
                }
                var count = await matches.CountAsync();
                //if (pi.TotalMatch == count)
                //{
                //    continue;
                //}
                pi.TotalMatch = count;
                await matches.ToAsyncEnumerable().ForEachAsync(m =>
                {
                    pi.CharacterTake[m.Character1]++;
                    pi.CharacterTake[m.Character2]++;
                    pi.CourseTake[m.CourseId]++;
                    if (m.Order == 0)
                    {
                        pi.CharacterWin[m.Character1]++;
                        pi.CharacterWin[m.Character2]++;
                        pi.CourseWin[m.CourseId]++;
                    }
                });
            }
        }

        private async void CleanRoom(object o)
        {
            try
            {
                var dt = DateTime.Now.AddHours(-Core.Config.MaximumRoomTime);
                IQueryable<SkyRoom> rooms;
                if (_superClean)
                {
                    _superClean = false;
                    rooms = _db.Rooms;
                }
                else
                {
                    rooms = from room in _db.Rooms
                            where room.SetupTime < dt && (room.RaceCount == 0 || room.PlayerNum == 0)
                            select room;
                }

                _db.Rooms.RemoveRange(rooms);
                await _db.SaveChangesAsyncLock();
            }
            catch (Exception)
            {
                return;
            }
        }

        private async void KickUser(object o)
        {
            if (Core.UserManager == null)
            {
                return;
            }
            var dt = DateTime.Now.AddHours(-Core.Config.MaximumUserOnlineTime);
            //var onlines = await (from u in Core.UserManager.Users
            //              where u.State != UserState.Offline && u.LastActiveTime.HasValue && u.LastActiveTime.Value < dt
            //              select u).ToListAsync();
            //foreach (var u in onlines)
            //{
            //    u.State = UserState.Offline;
            //    u.AccessToken = null;
            //    await Core.UserManager.UpdateAsyncLock(u);
            //}

            int topRank = 1;
            int secondRank = 5;
            int thirdRank = 10;

            IQueryable<int> us;
            int aRankCount;
            try
            {
                us = from u in Core.UserManager.Users
                     orderby u.Class descending
                     select u.Id;

                aRankCount = (int)((us.Count() - (topRank + secondRank + thirdRank)) * 0.33f);
            }
            catch (Exception)
            {
                return;
            }
            int aRank = aRankCount;
            int bRank = aRank;

            List<int> usList = await us.ToListAsync();

            foreach (var uid in usList)
            {
                try
                {
                    var user = await Core.UserManager.GetUserByIdAsync(uid);
                    if (user == null)
                    {
                        continue;
                    }
                    if (user.State != UserState.Offline && user.LastActiveTime < dt)
                    {
                        user.State = UserState.Offline;
                        user.AccessToken = null;
                    }
                    if (topRank > 0)
                    {
                        topRank--;
                        user.Rank = (int)Ranking.S;
                        await Core.UserManager.UpdateAsyncLock(user);
                        continue;
                    }
                    if (secondRank > 0)
                    {
                        secondRank--;
                        user.Rank = (int)Ranking.T;
                        await Core.UserManager.UpdateAsyncLock(user);
                        continue;
                    }
                    if (thirdRank > 0)
                    {
                        thirdRank--;
                        user.Rank = (int)Ranking.V;
                        await Core.UserManager.UpdateAsyncLock(user);
                        continue;
                    }
                    if (aRank > 0)
                    {
                        aRank--;
                        user.Rank = (int)Ranking.A_Red - (aRankCount - aRank) - 1;
                        await Core.UserManager.UpdateAsyncLock(user);
                        continue;
                    }
                    if (bRank > 0)
                    {
                        bRank--;
                        user.Rank = (int)Ranking.B_Green - (aRankCount - bRank) - 1;
                        await Core.UserManager.UpdateAsyncLock(user);
                        continue;
                    }
                    user.Rank = (int)Ranking.C_Blue - 1;
                    await Core.UserManager.UpdateAsyncLock(user); //FIXED:
                    await Task.Delay(20);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }
}
