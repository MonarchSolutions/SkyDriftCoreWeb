using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SkyDriftCoreWeb.Models.StaticsModels;

namespace SkyDriftCoreWeb
{
    public static class HtmlHelperExtensions
    {
        public static HtmlString CreatPageLiTag(this IUrlHelper urlHelper,
                                                   BasePageModel pageModel,
                                                   int index,
                                                   bool isCurrentIndex = false,
                                                   bool isDisable = true,
                                                   string content = "")
        {
            string url = urlHelper.Action(new UrlActionContext()
            {
                Action = pageModel.ActionName,
                Values = new {searchkey = pageModel.SearchKeyWord, index = index}
            });
            string activeClass = !isCurrentIndex ? string.Empty : "class='active'";
            string disableClass = isDisable ? string.Empty : "class='disabled'";
            url = isDisable ? "href='" + url + "'" : string.Empty;
            string contentString = string.IsNullOrEmpty(content) ? index.ToString() : content;

            return new HtmlString("<li " + activeClass + disableClass + "><a " + url + ">" + contentString + "</a></li>");
        }
    }
}
