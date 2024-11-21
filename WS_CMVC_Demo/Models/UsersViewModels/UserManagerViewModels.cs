using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WS_CMVC_Demo.Models.UserPackagesViewModels;
using X.PagedList;

namespace WS_CMVC_Demo.Models.UsersViewModels
{
    public class UserManagerViewModel
    {
        [Display(Name = "Поиск по ФИО")]
        public string FIO { get; set; }

        [Display(Name = "Поиск по Email")]
        public string Email { get; set; }

        public Guid? UserId { get; set; }

        [Display(Name = "Фильтровать по роли:")]
        public Guid? RoleId { get; set; }

        [Display(Name = "Показывать по:")]
        public PageSize PageSize { get; set; } = PageSize.p20;

        public PagedList<ApplicationUser> Users { get; set; }

        public IList<ApplicationRole> Roles { get; set; }
    }

    public class UserForEdit
    {
        public string SecondName { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
    }

    public class UserWithRolesViewModel
    {
        public UserForEdit User { get; set; }

        public IList<string> SelectedRoles { get; set; } = new List<string>();

        public IList<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }

    public class UserManagerResetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }
}