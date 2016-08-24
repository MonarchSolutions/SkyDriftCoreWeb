using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDriftCoreWeb.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "请输入Email。")]
        [EmailAddress(ErrorMessage = "格式不正确")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "昵称")]
        [StringLength(30, ErrorMessage = "个性签名不能超过{2}字。")]
        public string NickName { get; set; } = "幻想乡游客";

        [Required(ErrorMessage = "请输入密码。")]
        [StringLength(26, ErrorMessage = "{0}需要在{2}位到{1}位之间。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "两次密码输入不匹配。")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "请输入有效的序列号。")]
        [Display(Name = "序列号")]
        [StringLength(19, ErrorMessage = "序列号应为{1}位。", MinimumLength = 19)]
        public string Serial { get; set; }

        [Display(Name = "个性签名")]
        [DataType(DataType.MultilineText)]
        [StringLength(150, ErrorMessage = "个性签名不能超过140字。")]
        public string Sign { get; set; }

    }
}
