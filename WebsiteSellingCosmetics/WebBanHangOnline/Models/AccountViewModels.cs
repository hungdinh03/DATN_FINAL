using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBanHangOnline.Models
{
    public class LoginRegisterViewModel
    {
        public LoginViewModel Login { get; set; }
        public RegisterViewModel Register { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng điền tên đăng nhập!")]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng điền mật khẩu!")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Nhớ lần đăng nhập của tôi")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        [EmailAddress(ErrorMessage = "Sai định dạng email!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        [StringLength(100, ErrorMessage = "{0} phải có độ dài ít nhất bằng {2}!", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp. Vui lòng nhập lại!")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng điền email!")]
        [EmailAddress(ErrorMessage = "Sai định dạng email!")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ConfirmEmailViewModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }

        [Required(ErrorMessage = "Vui lòng điền mật khẩu mới!")]
        [StringLength(100, ErrorMessage = "{0} phải có độ dài ít nhất bằng {2}!", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp. Vui lòng nhập lại!")]
        public string ConfirmPassword { get; set; }
    }

    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string FullName { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        [EmailAddress(ErrorMessage = "Sai định dạng email!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        [StringLength(100, ErrorMessage = "{0} phải có độ dài ít nhất bằng {2}!", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp. Vui lòng nhập lại!")]
        public string ConfirmPassword { get; set; }

        public bool IsActive { get; set; }
    }

    public class EditAccountViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string FullName { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }

        public string Role { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        [EmailAddress(ErrorMessage = "Sai định dạng email!")]
        public string Email { get; set; }

        public bool IsActive { get; set; }
    }
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "Vui lòng điền vào mục này!")]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Nhớ lần đăng nhập của tôi")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

}
