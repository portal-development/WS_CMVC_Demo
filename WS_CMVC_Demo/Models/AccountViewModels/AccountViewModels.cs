using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WS_CMVC_Demo.Models.ManageViewModels;

namespace WS_CMVC_Demo.Models.AccountViewModels
{
    public static class Constant
    {
        public const string PhoneRegularExpression = "^\\+?\\d[\\d\\- ]{9,15}\\d$";
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Display(Name = "Фамилия")]
        [StringLength(50, MinimumLength = 1)]
        public string SecondName { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Display(Name = "Имя")]
        [StringLength(128, MinimumLength = 1)]
        public string Name { get; set; }

        [StringLength(128)]
        [Display(Name = "Отчество")]
        public string MiddleName { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Display(Name = "Код")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Запомнить браузер?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [EmailAddress(ErrorMessage = "Адрес электронной почты имеет неверный формат")]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Display(Name = "Телефон / Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public class MemberViewModel : EditUserViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [EmailAddress(ErrorMessage = "Адрес электронной почты имеет неверный формат.")]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Phone(ErrorMessage = "Мобильный телефон имеет неверный формат.")]
        [RegularExpression(Constant.PhoneRegularExpression, ErrorMessage = "Мобильный телефон имеет неверный формат.")]
        [Display(Name = "Мобильный телефон")]
        public string PhoneNumber { get; set; } = "+7";

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Display(Name = "Подкатегория")]
        public int UserSubcategoryId { get; set; }

        [Display(Name = "Страна")]
        public int? CountryId { get; set; }

        [Display(Name = "Регион")]
        public int? RussiaSubjectId { get; set; }

        [Display(Name = "Компетенция")]
        public int? CompetenceId { get; set; }

        [Display(Name = "Организация")]
        public string CompanyName { get; set; }

        [BooleanMustBeTrue(ErrorMessage = "Вы должны дать согласие на обработку персональных данных")]
        [Display(Name = "Согласие на обработку персональных данных")]
        public bool Agreement { get; set; } = false;
    }

    public class GoldMemberViewModel
    {
        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        [Display(Name = "Фамилия")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "{0} должна содержать не более {1} символов.")]
        public string SecondName { get; set; }

        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        [Display(Name = "Имя")]
        [StringLength(128, MinimumLength = 1, ErrorMessage = "{0} должно содержать не более {1} символов.")]
        public string Name { get; set; }

        [StringLength(128, ErrorMessage = "{0} должно содержать не более {1} символов.")]
        [Display(Name = "Отчество")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Display(Name = "Подкатегория")]
        public int UserSubcategoryId { get; set; }

        [Display(Name = "Страна")]
        public int? CountryId { get; set; }

        [Display(Name = "Регион")]
        public int? RussiaSubjectId { get; set; }

        [Display(Name = "Компетенция")]
        public int? CompetenceId { get; set; }

        [Display(Name = "Организация")]
        public string CompanyName { get; set; }
    }

    public class RegisterViewModel : MemberViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        [Phone(ErrorMessage = "Мобильный телефон имеет неверный формат.")]
        [RegularExpression(Constant.PhoneRegularExpression, ErrorMessage = "Мобильный телефон имеет неверный формат.")]
        [Display(Name = "Мобильный телефон")]
        public string PhoneNumber { get; set; } = "+7";
    }

    public class ConfirmPhoneNumberViewModel
    {
        [Required(ErrorMessage = "Поле \"{0}\" обязятельно для заполнения.")]
        public string Code { get; set; }
    }

    public class BooleanMustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object propertyValue)
        {
            return propertyValue != null
                && propertyValue is bool
                && (bool)propertyValue;
        }
    }
}
