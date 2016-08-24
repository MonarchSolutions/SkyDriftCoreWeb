using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyDriftCoreWeb.Data;
using SkyDriftCoreWeb.Models;
using static SkyDriftCoreWeb.Controllers.Results;
// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SkyDriftCoreWeb.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthController : Controller
    {
        private SkyDriftDbContext db = new SkyDriftDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        // POST api/auth/checkserial
        [HttpPost]
        public IActionResult CheckSerial(string serial_no)
        {
            var sn = db.Serials.FirstOrDefault(s => s.Sn == serial_no && !s.IsCode);
            return sn == null ?
                Json(new { status = ((int)SerialCheckResult.Wrong).ToString() })
                : Json(sn.CurrentUse > 0 ?
                new { status = ((int)SerialCheckResult.Used).ToString() }
                : new { status = ((int)SerialCheckResult.OK).ToString() });
        }

        [HttpPost]
        public async Task<IActionResult> Logout(string access_token)
        {
            try
            {
                var user = await GetUserByToken(access_token);
                if (user == null) {            HttpContext.AddHeader(SkyError.AuthError);return UnauthorizedJson(HttpContext);}
                user.State = UserState.Offline;
                db.Rooms.RemoveRange(from skyRoom in db.Rooms
                                     where skyRoom.HostId == user.Id
                                     select skyRoom);
                int id = user.Id;
                Core.OnlineUsers.TryRemove(user.AccessToken, out id);
                if (!Core.Config.TestUsers.Contains(user.Id))
                {
                    user.AccessToken = null;
                }
                await Task.WhenAll(_signInManager.SignOutAsync(), _userManager.UpdateAsyncLock(user), db.SaveChangesAsyncLock());
            }
            catch
            {
            }

            HttpContext.AddHeader(SkyError.OK);
            return OkJson(HttpContext);
        }

        // POST api/auth/login
        [HttpPost]

        public async Task<IActionResult> Login(string account, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(account, password, true, false);

            if (result.Succeeded)
            {
                var user = await _userManager.GetUserAsync(account);
                user.LoginTime = DateTime.Now;
                user.LastActiveTime = DateTime.Now;
                user.State = UserState.Online;
                if (!Core.Config.TestUsers.Contains(user.Id) || string.IsNullOrEmpty(user.AccessToken))
                {
                    user.AccessToken = Guid.NewGuid().ToString("D"); //重要
                }
                await _userManager.UpdateAsyncLock(user);
                Core.OnlineUsers[user.AccessToken] = user.Id;
                HttpContext.AddHeader(SkyError.OK);

                int bonus = 100;

                if (user.LastRaceTime.HasValue && user.LastRaceTime.Value.Date == DateTime.Today)
                {
                    bonus = 0;
                }

                return Json(
                    new
                    {
                        access_token = user.AccessToken,
                        today = DateTime.Now.Date.ToString("yyyy-MM-dd"),
                        name = string.IsNullOrWhiteSpace(user.NickName) ? "幻想乡游客" : user.NickName,
                        user_id = user.Id.ToString(),
                        klass = user.Class.ToString(),
                        rank = user.Rank.ToString(),
                        color_unlock = string.IsNullOrWhiteSpace(user.ColorUnlock) ? "0" : user.ColorUnlock,
                        news00 = Core.Config.ServerWelcome,
                        news01 = $"Online: {Core.OnlineUsers.Count}",
                        news02 = Core.Config.ServerNews,
                        daily_klass_bonus = bonus.ToString()
                    });
            }
            HttpContext.AddHeader(SkyError.AuthError);
            return UnauthorizedJson(HttpContext); //401
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
