using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SkyDriftCoreWeb.Controllers
{
    public static class Results
    {
        public static JsonResult BadRequestJson(HttpContext http)
        {
            http.AddHeader(SkyError.InvaildInput);
            return new JsonResult(new { status = "400"});
        }

        public static JsonResult UnauthorizedJson(HttpContext http)
        {
            http.AddHeader(SkyError.AuthError);
            return new JsonResult(new { status = "401" });
        }

        public static JsonResult OkJson(HttpContext http)
        {
            http.AddHeader(SkyError.OK);
            return new JsonResult(new { status = "200" });
        }
    }
}
