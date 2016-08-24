using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.ManageViewModels
{
    public class EditViewModel
    {
        [Display(Name = "昵称")]
        [StringLength(30, ErrorMessage = "昵称不能超过{1}个字。")]
        public string NickName { get; set; }

        [StringLength(150, ErrorMessage = "个性签名不能超过140字。")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "个性签名")]
        public string Sign { get; set; }
    }
}
