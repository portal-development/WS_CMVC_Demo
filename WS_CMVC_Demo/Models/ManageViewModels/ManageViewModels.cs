using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WS_CMVC_Demo.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public ApplicationUser User { get; set; }

        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationScheme> OtherLogins { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationScheme> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать символов не менее: {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать символов не менее: {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Номер телефона")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
    }

    public class EditUserViewModel
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

        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        [Display(Name = "Серия и номер паспорта (слитно без пробелов)")]
        public string PassportNumber { get; set; }
    }

    public class UserArrivalViewModel
    {
        /// <summary>
        /// Дата, время прилета/приезда
        /// </summary>
        [Display(Name = "Дата, время прилета/приезда")]
        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        public DateTime? ArrivalDateTime { get; set; }

        /// <summary>
        /// Номер самолета/поезда приезда
        /// </summary>
        [Display(Name = "Номер самолета/поезда")]
        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        public string ArrivalDetails { get; set; }

        /// <summary>
        /// Дата, время вылета/отъезда
        /// </summary>
        [Display(Name = "Дата, время вылета/отъезда")]
        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        public DateTime? DepartureDateTime { get; set; }

        /// <summary>
        /// Номер самолета/поезда отъезда
        /// </summary>
        [Display(Name = "Номер самолета/поезда")]
        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        public string DepartureDetails { get; set; }
    }

    public class UserBankDetailsViewModel
    {
        /// <summary>
        /// Общие реквизиты на всю делегацию
        /// </summary>
        [Display(Name = "Общие реквизиты на всю делегацию")]
        public bool IsGeneralBankDetails { get; set; } = false;

        /// <summary>
        /// Ревизиты для договора
        /// </summary>
        [Display(Name = "Ревизиты для договора")]
        [Required(ErrorMessage = "Поле {0} обязятельно для заполнения.")]
        public string BankDetails { get; set; }
    }
}