using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "请输入Email。")]
        [EmailAddress(ErrorMessage = "格式不正确")]
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入密码。")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我（公共场合请勿选择）")]
        public bool RememberMe { get; set; }
    }
}
