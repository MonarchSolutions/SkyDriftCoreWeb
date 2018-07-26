using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyDriftCoreWeb.Data;
using SkyDriftCoreWeb.Models;
using SkyDriftCoreWeb.Models.ApiModels;
using static SkyDriftCoreWeb.Controllers.Results;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SkyDriftCoreWeb.Controllers
{
    [Route("api/[controller]/[action]")]
    public class MatchingsController : Controller
    {
        private SkyDriftDbContext db = new SkyDriftDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public MatchingsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// 发送GUID以使用穿透
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        // POST api/matchings/send_guid
        [HttpPost]
        [ActionName("send_guid")]
        public async Task<IActionResult> RegisterGuid(RegisterGuidModel request)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(request.access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.IsMatch && r.RoomId == request.matching_id.Value);
            if (room == null) return BadRequestJson(HttpContext);
            if (room.HostId != user.Id && room.HostIp != HttpContext.GetClientIP())
            {
                return UnauthorizedJson(HttpContext);
            }
            room.HostGuid = request.not_use_guid == 1 ? null : request.guid;
            //Proxy
            if (request.use_proxy != null && request.use_proxy == 1
                && !string.IsNullOrWhiteSpace(request.proxy_ip)
                && !request.proxy_ip.StartsWith("UNASSIGNED_SYSTEM_ADDRESS")
                && request.proxy_port.HasValue && request.proxy_port.Value < 65535)
            {
                room.HostIp = request.proxy_ip;
                room.UsePort = request.proxy_port.Value;
                Core.RoomTracer[room.RoomId] = RoomJoin.UseProxy;
            }
            await Task.WhenAll(_userManager.UpdateAsyncLock(user), db.SaveChangesAsyncLock());
            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="matching_id"></param>
        /// <param name="access_token"></param>
        /// <param name="target_user_id"></param>
        /// <returns></returns>
        // POST api/rooms/add
        [HttpPost]
        public async Task<IActionResult> Add(int matching_id, string access_token, int target_user_id)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.IsMatch && r.RoomId == matching_id);
            if (room == null) return BadRequestJson(HttpContext);
            //MARK: is this necessary?
            if (room.HostId != user.Id)
            {
                return UnauthorizedJson(HttpContext);
            }
            //room.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == room.RoomId);
            //room.PlayerNum++;
            if (room.PlayerNum > 7)
            {
                room.PlayerNum = 7;
            }
            user.CurrentRoomId = room.RoomId;
            user.State = UserState.InMatch;
            await _userManager.UpdateAsyncLock(user);
            await db.SaveChangesAsyncLock();

            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="matching_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Leave(int matching_id, string access_token)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.IsMatch && r.RoomId == matching_id);
            if (room == null)
            {
                return BadRequestJson(HttpContext);
            }
            //room.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == room.RoomId);
            //room.PlayerNum--;
            if (room.PlayerNum < 0)
            {
                room.PlayerNum = 0;
            }
            user.CurrentRoomId = -1;
            user.State = UserState.Online;
            await Task.WhenAll(_userManager.UpdateAsyncLock(user), db.SaveChangesAsyncLock());
            var rt = Core.RoomTracer[room.RoomId];
            Core.RoomTracer[room.RoomId] = rt == RoomJoin.UseNat
                ? RoomJoin.Direct
                : RoomJoin.Unable;
            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        /// <summary>
        /// 删除房间
        /// </summary>
        /// <param name="matching_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("delete")]
        [Route("")]
        [Route("~/api/matchings/delete_for_to")]
        public async Task<IActionResult> Delete(int matching_id, string access_token)
        {
            if (ModelState.IsValid)
            {
                var user = await GetUserByToken(access_token);
                if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
                var room = db.Rooms.FirstOrDefault(r => r.RoomId == matching_id);
                if (room == null)
                {
                    return BadRequestJson(HttpContext);
                }
                HttpContext.AddHeader(SkyError.OK);
                if (room.HostId == user.Id)
                {
                    var us = from u in _userManager.Users
                             where u.State != UserState.Offline && u.CurrentRoomId == room.RoomId
                             select u;

                    foreach (var u in us.AsEnumerable())
                    {
                        u.CurrentRoomId = -1;
                        u.State = UserState.Online;
                        await _userManager.UpdateAsyncLock(u);
                    }
                    db.Rooms.Remove(room);
                    await db.SaveChangesAsyncLock();
                    return OkJson(HttpContext);
                }
                return UnauthorizedJson(HttpContext);
            }
            return BadRequestJson(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Kick(int matching_id, string access_token,
     int target_user_id)
        {
            if (ModelState.IsValid)
            {
                var user = await GetUserByToken(access_token);
                if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }

                var target = await _userManager.GetUserByIdAsync(target_user_id);

                if (target == null)
                {
                    return UnauthorizedJson(HttpContext);
                }
                var room = db.Rooms.FirstOrDefault(r => r.IsMatch && r.RoomId == matching_id);
                if (room == null) return BadRequestJson(HttpContext);
                if (room.HostId != user.Id && room.HostIp != HttpContext.GetClientIP())
                {
                    HttpContext.AddHeader(SkyError.AuthError);
                    return UnauthorizedJson(HttpContext);
                }
                //room.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == room.RoomId);
                //room.PlayerNum--;
                if (room.PlayerNum < 0)
                {
                    room.PlayerNum = 0;
                }
                target.CurrentRoomId = -1;
                target.State = UserState.Online;
                await Task.WhenAll(_userManager.UpdateAsyncLock(target), db.SaveChangesAsyncLock());
                HttpContext.AddHeader(SkyError.OK);
                return OkJson(HttpContext);
            }
            return BadRequestJson(HttpContext);
        }

        [HttpPost]
        [ActionName("disconnect_host")]
        public async Task<IActionResult> DisconnectHost(int matching_id, string access_token)
        {
            if (ModelState.IsValid)
            {
                var user = await GetUserByToken(access_token);
                if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
                //var room = db.Rooms.FirstOrDefault(r => r.IsMatch && r.RoomId == matching_id);
                //if (room == null) return BadRequestJson(HttpContext);
                user.CurrentRoomId = -1;
                foreach (var us in (from u in _userManager.Users
                                    where u.CurrentRoomId == matching_id
                                    select u).AsEnumerable())
                {
                    us.State = UserState.InMatch;
                    await _userManager.UpdateAsyncLock(us);
                }
                //user.State = UserState.Online;
                await _userManager.UpdateAsyncLock(user);
                HttpContext.AddHeader(SkyError.OK);
                return OkJson(HttpContext);
            }
            return BadRequestJson(HttpContext);
        }

        [HttpPost]
        [ActionName("race_start")]
        public async Task<IActionResult> RaceStart(int matching_id, string access_token)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.RoomId == matching_id && r.IsMatch);
            if (room == null) return BadRequestJson(HttpContext);
            HttpContext.AddHeader(SkyError.OK);
            foreach (var us in (from u in _userManager.Users
                                where u.CurrentRoomId == matching_id
                                select u).AsEnumerable())
            {
                us.State = UserState.InRace;
                await _userManager.UpdateAsyncLock(us);
            }

            //user.State = UserState.InRace;
            if (room.HostId == user.Id)
            {
                room.IsRacing = true;
            }
            await Task.WhenAll(_userManager.UpdateAsyncLock(user), db.SaveChangesAsyncLock());
            return OkJson(HttpContext);
        }

        [HttpPost]
        [ActionName("race_end")]
        public async Task<IActionResult> RaceEnd(int matching_id, string access_token)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.RoomId == matching_id && r.IsMatch);
            if (room == null) return BadRequestJson(HttpContext);
            HttpContext.AddHeader(SkyError.OK);
            user.State = UserState.InMatch;
            user.LastRaceTime = DateTime.Now;
            if (room.HostId == user.Id)
            {
                room.IsRacing = false;
                room.RaceCount++;
            }
            await Task.WhenAll(_userManager.UpdateAsyncLock(user), db.SaveChangesAsyncLock());
            return OkJson(HttpContext);
        }

        [HttpPost]
        [ActionName("goal")]
        public async Task<IActionResult> SendRaceResult(RaceResultModel request)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(request.access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.RoomId == request.matching_id && r.IsMatch);
            if (room == null) return BadRequestJson(HttpContext);
            if (user.CurrentRoomId != room.RoomId) return UnauthorizedJson(HttpContext);
            var anti = db.Matches.Any(m => m.UserId == user.Id && m.Time.HasValue &&
                                           DateTime.Now.Subtract(m.Time.Value).Duration().TotalSeconds <
                                           Core.Config.MinimumBreakTime);
            if (anti)
            {
                HttpContext.AddHeader(SkyError.AuthError);
                return UnauthorizedJson(HttpContext);
            }
            Match match = new Match()
            {
                UserId = user.Id,
                CourseId = request.course_id,
                Character1 = request.charcter1_id,
                Character2 = request.charcter2_id,
                Class = request.klass,
                Order = request.order,
                RoomId = request.matching_id,
                Time = DateTime.Now
            };
            db.Matches.Add(match);
            user.Class += match.Class; //POINT UP
            await Task.WhenAll(_userManager.UpdateAsyncLock(user), db.SaveChangesAsyncLock());
            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        /// <summary>
        /// 匹配
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        // status : 3 = no match found; 4 = old version; 5 = not available
        // POST api/matching/match
        [HttpPost]
        public async Task<IActionResult> Match(MatchModel request)
        {
            int maxMatchPlayer = 7;
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(request.access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            HttpContext.AddHeader(SkyError.OK);
            if (!Core.Config.MatchAvailable)
            {
                return Json(new
                {
                    status = (-5).ToString()
                });
            }

            SkyRoom sRoom = null;
            SkyRoom sAlter = null;

            foreach (var skyRoom in db.Rooms)
            {
                skyRoom.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == skyRoom.RoomId);
            }

            await db.SaveChangesAsyncLock();

            for (int i = 0; i < Core.Config.MatchTime; i++)
            {
                //TODO: Better match mechanism
                var rs = from room in db.Rooms
                         where room.IsMatch && !room.IsRacing && !room.HasPassword && room.PlayerNum < maxMatchPlayer && room.PlayerNum > 0
                         orderby room.PlayerNum descending
                         select room;
                if (i >= Core.Config.MatchTime - 1)
                {
                    sRoom = rs.FirstOrDefault();
                    break;
                }
                if (!rs.Any())
                {
                    await Task.Delay(100);
                    continue;
                }

                sRoom = rs.FirstOrDefault();
                sAlter = rs.Skip(1).FirstOrDefault();
                if (!Core.RoomTracer.ContainsKey(sRoom.RoomId))
                {
                    Core.RoomTracer[sRoom.RoomId] = RoomJoin.UseNat;
                }
                if (sAlter != null && !Core.RoomTracer.ContainsKey(sAlter.RoomId))
                {
                    Core.RoomTracer[sAlter.RoomId] = RoomJoin.UseNat;
                }
                if (Core.RoomTracer[sRoom.RoomId] == RoomJoin.Unable)
                {
                    Core.RoomTracer[sRoom.RoomId] = RoomJoin.UseNat;
                    sRoom = sAlter;
                }
                break;
            }
            if (sRoom == null)
            {
                if (request.client_only.HasValue && request.client_only.Value == 1)
                {
                    return Json(new
                    {
                        status = (-3).ToString()
                    });
                }
                //bool canWatch = request.client_only.HasValue ? request.client_only.Value == 1 : false;
                sRoom = new SkyRoom()
                {
                    CanWatch = true,
                    CliVersion = request.cli_version,
                    HostId = user.Id,
                    HostIp = HttpContext.GetClientIP(),
                    UsePort = request.use_port,
                    SetupTime = DateTime.Now,
                    IsMatch = true,
                    PlayerNum = 0,
                    WatcherNum = 0//canWatch ? 1 : 0
                };
                db.Rooms.Add(sRoom);
                await db.SaveChangesAsyncLock();
                user.CurrentRoomId = sRoom.RoomId;
                user.State = UserState.InMatch;
                await _userManager.UpdateAsyncLock(user);
                return Json(new
                {
                    status = "1", //Host = 1, Client = 2
                    matching_id = sRoom.RoomId.ToString(),
                    host_user_id = sRoom.HostId.ToString(),
                    host_ip = sRoom.HostIp,
                    host_port = sRoom.UsePort.ToString(),
                });
            }
            else //Room found
            {
                user.CurrentRoomId = sRoom.RoomId;
                await _userManager.UpdateAsyncLock(user);
                var rt = Core.RoomTracer[sRoom.RoomId];
                if (rt == RoomJoin.UseNat && !string.IsNullOrEmpty(sRoom.HostGuid))
                {
                    return Json(new
                    {
                        status = "2", //Host = 1, Client = 2
                        matching_id = sRoom.RoomId.ToString(),
                        host_user_id = sRoom.HostId.ToString(),
                        host_ip = sRoom.HostIp,
                        host_port = sRoom.UsePort.ToString(),
                        guid = sRoom.HostGuid
                    });
                }
                return Json(new
                {
                    status = "2", //Host = 1, Client = 2
                    matching_id = sRoom.RoomId.ToString(),
                    host_user_id = sRoom.HostId.ToString(),
                    host_ip = sRoom.HostIp,
                    host_port = sRoom.UsePort.ToString(),
                });
            }
        }

        private async Task<ApplicationUser> GetUserByToken(string access_token)
        {
            var user = await _userManager.GetUserByTokenAsync(access_token);
            if (user != null)
            {
                user.LastActiveTime = DateTime.Now;
            }
            return user;
        }
    }
}
