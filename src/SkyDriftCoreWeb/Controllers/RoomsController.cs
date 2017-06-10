using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class RoomsController : Controller
    {
        private SkyDriftDbContext db = new SkyDriftDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public RoomsController(
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
        // POST api/rooms/send_guid
        [HttpPost]
        [ActionName("send_guid")]
        public async Task<IActionResult> RegisterGuid(RegisterGuidModel request)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(request.access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => !r.IsMatch && r.RoomId == request.room_id.Value);
            if (room == null) return BadRequestJson(HttpContext);
            if (room.HostId != user.Id && room.HostIp != HttpContext.GetClientIP())
            {
                return UnauthorizedJson(HttpContext);
            }
            room.HostGuid = request.not_use_guid == 1 ? null : request.guid;
            await Task.WhenAll(_userManager.UpdateAsync(user), db.SaveChangesAsyncLock());
            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        /// <summary>
        /// 搜索房间
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="cli_version"></param>
        /// <returns></returns>
        // POST api/rooms/search
        [HttpPost]
        public async Task<IActionResult> Search(string access_token, string cli_version)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var dt = DateTime.Now.AddHours(-Core.Config.MaximumRoomTime);
            var rooms = from room in db.Rooms
                        where !room.IsMatch
                              && room.PlayerNum > 0 && room.PlayerNum < 8
                              && room.CliVersion == cli_version && room.SetupTime > dt
                        select new
                        {
                            room_id = room.RoomId.ToString(),
                            room_name = room.RoomName,
                            password_flg = room.HasPassword ? "1" : "0",
                            deny_watching_flg = room.CanWatch ? "1" : "0",
                            racing_now = room.IsRacing ? "1" : "0",
                            players_num = room.PlayerNum.ToString(),
                            watchers_num = room.WatcherNum.ToString(),
                        };
            return Json(rooms);
        }

        /// <summary>
        /// 玩家加入
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="room_id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Join(string access_token, int room_id)
        {
            //-4 = room not found
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }

            HttpContext.AddHeader(SkyError.OK);
            var room = db.Rooms.FirstOrDefault(r => !r.IsMatch && r.RoomId == room_id);
            if (room == null)
            {
                return Json(new { status = ((int)SkyError.OldVersionOrRoomNotExists).ToString() });
            }
            int fulled = 1;
            user.CurrentRoomId = room_id;
            user.State = UserState.InRoom;
            room.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == room.RoomId);
            room.PlayerNum++;
            if (room.PlayerNum >= 7) //BUG: What is the code of full?
            {
                room.PlayerNum = 7;
                fulled = 2;
            }
            await Task.WhenAll(_userManager.UpdateAsync(user), db.SaveChangesAsyncLock());
            if (!Core.RoomTracer.ContainsKey(room.RoomId))
            {
                Core.RoomTracer[room.RoomId] = RoomJoin.UseNat;
            }
            if (string.IsNullOrEmpty(room.HostGuid) || Core.RoomTracer[room.RoomId] == RoomJoin.Direct)
            {
                return
                    Json(
                        new
                        {
                            status = fulled.ToString(),
                            host_user_id = room.HostId.ToString(),
                            host_ip = room.HostIp,
                            host_port = room.UsePort.ToString()
                        });
            }

            return
                Json(
                    new
                    {
                        status = fulled.ToString(),
                        host_user_id = room.HostId.ToString(),
                        host_ip = room.HostIp,
                        host_port = room.UsePort.ToString(),
                        guid = room.HostGuid
                    });
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="room_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Leave(int room_id, string access_token)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => !r.IsMatch && r.RoomId == room_id);
            if (room == null)
            {
                return BadRequestJson(HttpContext);
            }
            room.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == room.RoomId);
            room.PlayerNum--;
            if (room.PlayerNum < 0)
            {
                room.PlayerNum = 0;
            }
            user.CurrentRoomId = -1;
            user.State = UserState.Online;
            Core.RoomTracer[room.RoomId] = Core.RoomTracer[room.RoomId] == RoomJoin.UseNat
                ? RoomJoin.Direct
                : RoomJoin.UseNat;
            await Task.WhenAll(_userManager.UpdateAsync(user), db.SaveChangesAsyncLock());
            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="room_id"></param>
        /// <param name="access_token"></param>
        /// <param name="target_user_id"></param>
        /// <returns></returns>
            // POST api/rooms/add
        [HttpPost]
        public async Task<IActionResult> Add(int room_id, string access_token, int target_user_id)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var target = await GetUserByToken(access_token);
            if (target == null) return BadRequestJson(HttpContext);
            var room = db.Rooms.FirstOrDefault(r => !r.IsMatch && r.RoomId == room_id);
            if (room == null) return BadRequestJson(HttpContext);

            if (room.HostId != user.Id && room.HostIp != HttpContext.GetClientIP())
            {
                return UnauthorizedJson(HttpContext);
            }
            target.CurrentRoomId = room.RoomId;
            target.State = UserState.InRoom;
            room.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == room.RoomId);
            room.PlayerNum++;
            if (room.PlayerNum > 7)
            {
                room.PlayerNum = 7;
            }
            await Task.WhenAll(_userManager.UpdateAsync(user), _userManager.UpdateAsync(target), db.SaveChangesAsyncLock());
            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        // POST api/rooms/create
        [HttpPost]
        public async Task<IActionResult> Create(CreateRoomModel request)
        {
            if (ModelState.IsValid)
            {
                var user = await GetUserByToken(request.access_token);
                if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
                user.State = UserState.InRoom;

                SkyRoom room = new SkyRoom()
                {
                    CliVersion = request.cli_version,
                    RoomName = request.room_name,
                    UsePort = request.use_port,
                    HasPassword = request.password_flg == 1,
                    CanWatch = request.deny_watching_flg == 1,
                    HostIp = HttpContext.GetClientIP(),
                    IsMatch = false,
                    SetupTime = DateTime.Now,
                    HostId = user.Id,
                    PlayerNum = 1,
                    WatcherNum = request.deny_watching_flg == 1 ? 1 : 0
                };
                db.Rooms.Add(room);
                await Task.WhenAll(_userManager.UpdateAsync(user), db.SaveChangesAsyncLock());
                //MARK: 保存后会自动更新对象，nice！
                Core.RoomTracer[room.RoomId] = RoomJoin.UseNat;
                HttpContext.AddHeader(SkyError.OK);
                return new JsonResult(new { room_id = room.RoomId.ToString() });
            }
            return BadRequestJson(HttpContext);
        }

        /// <summary>
        /// 删除房间
        /// </summary>
        /// <param name="room_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete(int room_id, string access_token)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.RoomId == room_id);
            if (room == null) return BadRequestJson(HttpContext);
            HttpContext.AddHeader(SkyError.OK);
            if (room.HostId == user.Id || room.HostIp == HttpContext.GetClientIP())
            {
                var us = from u in _userManager.Users
                         where u.State != UserState.Offline && u.CurrentRoomId == room.RoomId
                         select u;
                foreach (var u in us.AsEnumerable())
                {
                    u.CurrentRoomId = -1;
                    u.State = UserState.Online;
                    await _userManager.UpdateAsync(u);
                }
                db.Rooms.Remove(room);
                await db.SaveChangesAsyncLock();
                return OkJson(HttpContext);
            }
            return UnauthorizedJson(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Kick(int room_id, string access_token,
             int target_user_id)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var target = await _userManager.GetUserByIdAsync(target_user_id);
            if (target == null) return UnauthorizedJson(HttpContext);
            var room = db.Rooms.FirstOrDefault(r => !r.IsMatch && r.RoomId == room_id);
            if (room != null)
            {
                if (room.HostId != user.Id && room.HostIp != HttpContext.GetClientIP())
                {
                    HttpContext.AddHeader(SkyError.AuthError);
                    return UnauthorizedJson(HttpContext);
                }
                room.PlayerNum = _userManager.Users.Count(u => u.CurrentRoomId == room.RoomId);
                room.PlayerNum--;
                if (room.PlayerNum < 0)
                {
                    room.PlayerNum = 0;
                }
                target.CurrentRoomId = -1;
                target.State = UserState.Online;
                await Task.WhenAll(_userManager.UpdateAsync(target), db.SaveChangesAsyncLock());
                HttpContext.AddHeader(SkyError.OK);
                return OkJson(HttpContext);
            }
            return BadRequestJson(HttpContext);
        }

        [HttpPost]
        [ActionName("race_start")]
        public async Task<IActionResult> RaceStart(int room_id, string access_token)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.RoomId == room_id && !r.IsMatch);
            if (room == null) return BadRequestJson(HttpContext);
            HttpContext.AddHeader(SkyError.OK);
            foreach (var us in (from u in _userManager.Users
                                where u.CurrentRoomId == room_id
                                select u).AsEnumerable())
            {
                us.State = UserState.InRace;
                await _userManager.UpdateAsyncLock(us);
            }

            //user.State = UserState.InRace;
            if (room.HostId == user.Id || room.HostIp == HttpContext.GetClientIP())
            {
                room.IsRacing = true;
            }
            await Task.WhenAll(_userManager.UpdateAsync(user), db.SaveChangesAsyncLock());
            return OkJson(HttpContext);
        }

        [HttpPost]
        [ActionName("race_end")]
        public async Task<IActionResult> RaceEnd(int room_id, string access_token)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            var room = db.Rooms.FirstOrDefault(r => r.RoomId == room_id && !r.IsMatch);
            if (room == null) return BadRequestJson(HttpContext);
            HttpContext.AddHeader(SkyError.OK);
            foreach (var us in (from u in _userManager.Users
                                where u.CurrentRoomId == room_id
                                select u).AsEnumerable())
            {
                us.State = UserState.InRoom;
                await _userManager.UpdateAsyncLock(us);
            }

            //user.State = UserState.InRoom;
            //user.LastRaceTime = DateTime.Now; //MARK: 会导致排位不能正常加分 取消
            if (room.HostId == user.Id || room.HostIp == HttpContext.GetClientIP())
            {
                room.IsRacing = false;
                room.RaceCount++;
            }
            await Task.WhenAll(_userManager.UpdateAsync(user), db.SaveChangesAsyncLock());
            return OkJson(HttpContext);
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
