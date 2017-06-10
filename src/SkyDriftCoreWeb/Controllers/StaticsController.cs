using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyDriftCoreWeb.Data;
using SkyDriftCoreWeb.Models;
using SkyDriftCoreWeb.Models.StaticsModels;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SkyDriftCoreWeb.Controllers
{
    public class StaticsController : Controller
    {
        private SkyDriftDbContext db = new SkyDriftDbContext();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public StaticsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> State()
        {
            StateModel s = new StateModel();
            var users = _userManager.Users.ToAsyncEnumerable();
            s.OnlineUsersCount = await users.Count(u => u.State != UserState.Offline);
            s.MatchingUsersCount = await users.Count(u => u.State == UserState.InMatch);
            s.InRoomUsersCount = await users.Count(u => u.State == UserState.InRoom);
            s.RacingUsersCount = await users.Count(u => u.State == UserState.InRace);
            s.OfflineUsersCount = _userManager.Users.Count() - s.OnlineUsersCount;

            s.Rooms = await db.Rooms.ToListAsync();

            return View(s);
        }

        public IActionResult Record(RecordQueryModel model)
        {
            if (model == null)
            {
                model = new RecordQueryModel() {Course = Course.马里奥赛车,Records = new List<Record>()};
            }

            ViewBag.CourseName = Helper.GetCourseName(model.Course);

            BasePageModel page = new BasePageModel() { SearchKeyWord = "", CurrentIndex = 1, TotalCount = 10 };
            var list = (from r in db.Records
                where r.CourseId == (int)model.Course
                orderby r.TotalTime
                select r).Take(10).AsEnumerable().ToList();
            ViewData["PageModel"] = page;
            model.Records = list;
            return View(model);
        }

        public async Task<IActionResult> Info(int userId)
        {
            var user = await _userManager.GetUserByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            return View(user);
        }

        // GET: /<controller>/
        public IActionResult Index(string searchkey, string index)
        {
            if (string.IsNullOrEmpty(index))
                index = "1";
            if (string.IsNullOrEmpty(searchkey))
                searchkey = string.Empty;

            int idx;
            if (!int.TryParse(index, out idx))
            {
                idx = 1;
            }
            BasePageModel page = new BasePageModel() { SearchKeyWord = searchkey, CurrentIndex = idx, TotalCount = 10 };
            List<ApplicationUser> users = new List<ApplicationUser>();
            var all = (from u in _userManager.Users
                       orderby u.Class descending
                       select u);
            var allList = all.AsEnumerable().ToList();
            ViewBag.Rank = allList;
            List<ApplicationUser> list = allList;

            try
            {
                if (!string.IsNullOrWhiteSpace(searchkey))
                {
                    searchkey = searchkey.ToLowerInvariant();
                    if (searchkey == "@online")
                    {
                        list = (from user in all
                                where user.State != UserState.Offline
                                orderby user.Class descending
                                select user).ToList();
                    }
                    else
                    {
                        list = (from user in all
                                where
                                (!string.IsNullOrEmpty(user.NickName) &&
                                user.NickName.ToLowerInvariant().Contains(searchkey)) ||
                                user.UserName.ToLowerInvariant().Contains(searchkey) ||
                                (!string.IsNullOrEmpty(user.Sign) &&
                                user.Sign.ToLowerInvariant().Contains(searchkey))
                                orderby user.Class descending
                                select user).ToList();
                    }
                }
                users = list.Skip((page.CurrentIndex - 1) * page.PageSize).Take(page.PageSize).ToList();

            }
            catch (Exception)
            {
            }

            page.TotalCount = list.Count;
            ViewData["PageModel"] = page;
            return View(users);
        }

    }
}
