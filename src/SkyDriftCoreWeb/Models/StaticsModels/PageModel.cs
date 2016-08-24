using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.StaticsModels
{
    /// <summary>
    /// From http://www.cnblogs.com/ListenCode/p/4198258.html
    /// </summary>
    public class BasePageModel
    {
        public string SearchKeyWord { get; set; }

        /// <summary>
        ///点击分页是指向 Action 的名字 根据具体需要而定
        /// </summary>
        public virtual string ActionName
        {
            get
            {
                return "Index";
            }
        }

        public int TotalCount { get; set; }

        public int CurrentIndex { get; set; }

        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)TotalCount / (double)PageSize);
            }
        }

        /// <summary>
        /// 根据需要具体而定PageSize
        /// </summary>
        public virtual int PageSize
        {
            get { return 10; }
        }

        /// <summary>
        ///根据需要具体而定 分页显示最大的页数 
        /// </summary>
        public virtual int DisplayMaxPages
        {
            get
            {
                return 10;
            }
        }

        public bool IsHasPrePage
        {
            get
            {
                return CurrentIndex != 1;
            }
        }

        public bool IsHasNextPage
        {
            get
            {
                return CurrentIndex != TotalPages;
            }
        }
    }
}
