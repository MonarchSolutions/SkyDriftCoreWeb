using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
    public class UsersController : Controller
    {
        private SkyDriftDbContext db = new SkyDriftDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        // POST api/users/add
        [HttpPost]
        public async Task<IActionResult> Add(AuthoriseSerialModel request)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            HttpContext.AddHeader(SkyError.OK);

            var sn = from serial in db.Serials
                     where serial.Available && !serial.IsCode
                     select serial;
            if (!sn.Any()) return Json(new { user_id = AuthoriseSerialModel.InvaildInput.ToString() });
            var s = sn.First();
            if (s.CurrentUse >= s.MaxUse) return Json(new { user_id = AuthoriseSerialModel.InvaildInput.ToString() });
            if (_userManager.Users.Any(u =>
            (u.UserName.ToUpperInvariant() == request.account.ToUpperInvariant()) || (u.IP == HttpContext.GetClientIP() && u.SerialNo == s.Sn)))
            {
                return Json(new { user_id = AuthoriseSerialModel.InvaildInputDetail.ToString() });
            }

            var user = new ApplicationUser
            {
                UserName = request.account,
                Email = request.account,
                SerialNo = request.serial_no,
                IP = HttpContext.GetClientIP(),
                RegisterTime = DateTime.Now,
                LastActiveTime = (DateTime)SqlDateTime.MinValue,
                LoginTime = DateTime.Now,
                LastRaceTime = (DateTime)SqlDateTime.MinValue
            };
            var result = await _userManager.CreateAsync(user, request.password);
            if (result.Succeeded)
            {
                s.CurrentUse++;
                await db.SaveChangesAsyncLock();
                return Json(new { user_id = user.Id.ToString() });
            }
            return Json(new { user_id = AuthoriseSerialModel.InvaildInput.ToString() });
        }

        /// <summary>
        /// 修改昵称
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        // POST api/users/edit
        [HttpPost]
        public async Task<IActionResult> Edit(string access_token,  string name)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) {            HttpContext.AddHeader(SkyError.AuthError);return UnauthorizedJson(HttpContext);}
            user.NickName = name;
            await _userManager.UpdateAsyncLock(user);
            HttpContext.AddHeader(SkyError.OK);
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
