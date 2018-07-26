using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [Route("api")]
    public class SpecialController : Controller
    {
        private SkyDriftDbContext db = new SkyDriftDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public SpecialController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // POST api/time_attacks/send_record
        [HttpPost]
        [Route("time_attacks/send_record")]
        public async Task<IActionResult> SendRecord(RecordModel request)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(request.access_token);
            if (user == null)
            {
                HttpContext.AddHeader(SkyError.OK);
                return Json(null);
            }
            var anti = db.Records.Any(m => m.UserId == user.Id && m.Time.HasValue &&
                               DateTime.Now.Subtract(m.Time.Value).Duration().TotalSeconds <
                               Core.Config.MinimumBreakTime);
            if (anti)
            {
                HttpContext.AddHeader(SkyError.OK);
                return Json(null);
            }
            Record r = new Record()
            {
                CourseId = request.course_id,
                Character1 = request.charcter1_id,
                Character2 = request.charcter2_id,
                TotalTime = request.totaltime,
                Time0 = request.time0,
                Time1 = request.time1,
                Time2 = request.time2,
                Time = DateTime.Now,
                UserId = user.Id
            };
            //FIXED:
            var oldR = db.Records.FirstOrDefault(rec => rec.UserId == user.Id && rec.CourseId == request.course_id);
            if (oldR != null)
            {
                if (oldR.TotalTime < r.TotalTime) //FIXED:
                {
                    HttpContext.AddHeader(SkyError.InvaildInput);
                    return Json(null);
                }
                oldR.Character1 = r.Character1;
                oldR.Character2 = r.Character2;
                oldR.TotalTime = r.TotalTime;
                oldR.Time0 = r.Time0;
                oldR.Time1 = r.Time1;
                oldR.Time2 = r.Time2;
                oldR.Time = r.Time;
                db.Update(oldR);
            }
            else
            {
                db.Records.Add(r);
            }
            await Task.WhenAll(_userManager.UpdateAsyncLock(user), db.SaveChangesAsyncLock());
            HttpContext.AddHeader(SkyError.OK);
            return Json(null);
        }

        //POST api/versions/check
        [HttpPost]
        [Route("versions/check")]
        public async Task<IActionResult> CheckUpdater(int version)
        {
            HttpContext.AddHeader(SkyError.OK);
            string updateUrl = null;
            try
            {
                updateUrl = await Core.CheckUpdate(version);
            }
            catch (Exception)
            {
                return Json(new { status = ((int)UpdateCheckResult.Error).ToString() });
            }
            if (string.IsNullOrWhiteSpace(updateUrl))
            {
                return Json(new { status = ((int)UpdateCheckResult.UpToDate).ToString() });
            }
            return Json(new { status = ((int)UpdateCheckResult.UpdateAvailable).ToString(), url = updateUrl });
        }
        // POST api/special_codes/activate_code
        [HttpPost]
        [Route("special_codes/activate_code")]
        public async Task<IActionResult> ActiviteCode(string access_token, string special_code)
        {
            if (!ModelState.IsValid) return BadRequestJson(HttpContext);
            var user = await GetUserByToken(access_token);
            if (user == null) { HttpContext.AddHeader(SkyError.AuthError); return UnauthorizedJson(HttpContext); }
            HttpContext.AddHeader(SkyError.OK);


            var cs = from code in db.Serials
                     where code.IsCode && code.Sn == special_code && code.Available && !string.IsNullOrEmpty(code.UnlockColor)
                     select code;
            if (cs.Any())
            {
                var c = cs.First();
                if (c.CurrentUse < c.MaxUse)
                {
                    Dictionary<int, bool> colors = new Dictionary<int, bool>();
                    if (!string.IsNullOrWhiteSpace(user.ColorUnlock))
                    {
                        foreach (var s in user.ColorUnlock.Split(','))
                        {
                            try
                            {
                                colors[int.Parse(s)] = true;
                            }
                            catch (Exception)
                            {
                                // ignored
                                continue;
                            }
                        }
                    }

                    foreach (var s in c.UnlockColor.Split(','))
                    {
                        try
                        {
                            colors[int.Parse(s)] = true;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    foreach (var color in colors)
                    {
                        if (color.Value)
                        {
                            sb.Append(color.Key).Append(",");
                        }
                    }
                    user.ColorUnlock = sb.ToString();
                    c.CurrentUse++;
                    await Task.WhenAll(db.SaveChangesAsyncLock(), _userManager.UpdateAsyncLock(user));
                    return Json(new
                    {
                        color_unlock = c.UnlockColor,
                        status = ((int)ActivateCodeError.NewItemGet).ToString()
                    });
                }
                return Json(new
                {
                    color_unlock = "",
                    status = ((int)ActivateCodeError.OverUsed).ToString()
                });
            }
            return Json(new
            {
                color_unlock = "",
                status = ((int)ActivateCodeError.Wrong).ToString()
            });
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
