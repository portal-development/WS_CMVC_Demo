using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WS_CMVC_Demo;
using WS_CMVC_Demo.Models.Badge;
using WS_CMVC_Demo.Models.Service;
using WS_CMVC_Demo.Services;

namespace WS_CMVC_Demo.Models
{
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class ApplicationUser : IdentityUser<Guid>
    {
        private string _secondName, _name, _middleName, _passportNumber;

        [Required, Display(Name = "Адрес электронной почты")]
        public override string Email { get => base.Email; set => base.Email = value; }

        [Display(Name = "Телефон")]
        public override string PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [Required, Display(Name = "Фамилия"), StringLength(50, MinimumLength = 1)]
        public string SecondName { get => _secondName; set => _secondName = value.Trim(); }

        /// <summary>
        /// Имя
        /// </summary>
        [Required, Display(Name = "Имя"), StringLength(128, MinimumLength = 1)]
        public string Name { get => _name; set => _name = value.Trim(); }

        /// <summary>
        /// Отчество
        /// </summary>
        [Display(Name = "Отчество"), StringLength(128)]
        public string MiddleName { get => _middleName; set => _middleName = value?.Trim(); }

        /// <summary>
        /// ФИО в формате "Фамилия И.О."
        /// </summary>
        [Display(Name = "ФИО"), StringLength(128)]
        public string CropName
        {
            get
            {
                if (SecondName?.Length > 0 && Name?.Length > 0)
                {
                    string result = SecondName.ToUpper()[0] + SecondName.ToLower()[1..];
                    result += Convert.ToChar(160).ToString() + Name.ToUpper()[0] + ".";
                    if (MiddleName?.Length > 0)
                    {
                        result += MiddleName.ToUpper()[0] + ".";
                    }

                    return result;
                }

                return "(имя не указано)";
            }
        }

        /// <summary>
        /// Серия и номер паспорта слитно без пробелов
        /// </summary>
        [StringLength(64)]
        [Display(Name = "Серия и номер паспорта")]
        public string PassportNumber { get => _passportNumber; set => _passportNumber = value?.RemoveWhitespace(); }

        /// <summary>
        /// Пользователь регистрировался самостоятельно
        /// </summary>
        public bool RegisteredHimself { get; set; } = true;

        /// <summary>
        /// Идентификатор пользователя который добавил текущего пользователя
        /// </summary>
        public Guid? RegisteredUserId { get; set; }

        /// <summary>
        /// Пользователь который добавил текущего пользователя
        /// </summary>
        public virtual ApplicationUser RegisteredUser { get; set; }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        [Display(Name = "Дата регистрации")]
        public DateTime RegisterDate { get; private set; } = DateTime.Now;

        /// <summary>
        /// Дата последнего входа в систему
        /// </summary>
        [Display(Name = "Дата последнего входа в систему")]
        public DateTime? LastSigInDate { get; set; }

        /// <summary>
        /// Список мероприятий в которых участвует пользователь
        /// </summary>
        public virtual ICollection<EventUser> EventUsers { get; set; }

        /// <summary>
        /// Идентификатор категории пользователя при регистрации
        /// </summary>
        public int? UserCategoryId { get; set; }

        /// <summary>
        /// Категория пользователя при реистрации
        /// </summary>
        [Display(Name = "Категория")]
        public virtual UserCategory UserCategory { get; set; }

        /// <summary>
        /// Идентификатор подкатегории пользователя при регистрации
        /// </summary>
        public int? UserSubcategoryId { get; set; }

        /// <summary>
        /// Подкатегория пользователя при реистрации
        /// </summary>
        [Display(Name = "Подкатегория")]
        public virtual UserSubcategory UserSubcategory { get; set; }

        /// <summary>
        /// Идентификатор страны
        /// </summary>
        public int? CountryId { get; set; }

        /// <summary>
        /// Страна
        /// </summary>
        [Display(Name = "Страна")]
        public virtual UserCountry Country { get; set; }

        /// <summary>
        /// Идентификатор региона
        /// </summary>
        public int? RussiaSubjectId { get; set; }

        /// <summary>
        /// Регион России
        /// </summary>
        [Display(Name = "Регион")]
        public virtual UserRussiaSubject RussiaSubject { get; set; }

        /// <summary>
        /// Идентификатор компетенции
        /// </summary>
        public int? CompetenceId { get; set; }

        /// <summary>
        /// Компетенция
        /// </summary>
        [Display(Name = "Компетенция")]
        public virtual UserCompetence Competence { get; set; }

        /// <summary>
        /// Наименование организации
        /// </summary>
        [Display(Name = "Организация")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Согласие на обработку персональных данных
        /// </summary>
        public bool Agreement { get; set; } = false;

        /// <summary>
        /// Пользователь импортирован из csv (LeaderID)
        /// </summary>
        public bool ImportedUser { get; set; } = false;

        /// <summary>
        /// Забронированные услуги из пакетов
        /// </summary>
        public virtual ICollection<UserPackageService> PackageServices { get; set; }

        public virtual ICollection<UserRole> Roles { get; set; }

        /// <summary>
        /// Возвращает true если у пользователя есть права на редактирование данного пользователя
        /// </summary>
        /// <param name="editorUserId">Идентификатор пользователя-редактора</param>
        /// <returns></returns>
        public bool CanEdit(Guid? editorUserId)
        {
            if (editorUserId == Id || editorUserId == RegisteredUserId)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает true если у пользователя есть права на редактирование данного пользователя
        /// </summary>
        /// <param name="editorUser">Пользователь-редактор</param>
        /// <returns></returns>
        public bool CanEdit(ApplicationUser editorUser)
        {
            return CanEdit(editorUser.Id);
        }

        public bool CanDelete(Guid? editorUserId)
        {
            if (editorUserId != Id && editorUserId == RegisteredUserId && IsDeleteble)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает true если пользователь не "наследил" в системе и его можно безболезненно удалить
        /// </summary>
        public bool IsDeleteble => !LastSigInDate.HasValue;

        #region Реквизиты
        /// <summary>
        /// Общие реквизиты на всю делегацию
        /// </summary>
        [Display(Name = "Общие реквизиты на всю делегацию")]
        public bool IsGeneralBankDetails { get; set; } = false;

        /// <summary>
        /// Ревизиты для договора
        /// </summary>
        [Display(Name = "Ревизиты для договора")]
        public string BankDetails { get; set; }
        #endregion

        #region Информация о прибытии
        /// <summary>
        /// Дата, время прилета/приезда
        /// </summary>
        [Display(Name = "Дата, время прилета/приезда")]
        public DateTime? ArrivalDateTime { get; set; }

        /// <summary>
        /// Номер самолета/поезда приезда
        /// </summary>
        [Display(Name = "Номер самолета/поезда")]
        public string ArrivalDetails { get; set; }

        /// <summary>
        /// Дата, время вылета/отъезда
        /// </summary>
        [Display(Name = "Дата, время вылета/отъезда")]
        public DateTime? DepartureDateTime { get; set; }

        /// <summary>
        /// Номер самолета/поезда отъезда
        /// </summary>
        [Display(Name = "Номер самолета/поезда")]
        public string DepartureDetails { get; set; }
        #endregion

        public virtual ICollection<BadgeServiceCheckup> BadgeServiceCheckups { get; set; }
    }



    public class ApplicationRole : IdentityRole<Guid>
    {
        /// <summary>
        /// Оисание
        /// </summary>
        [DataType(DataType.MultilineText), Display(Name = "Оисание")]
        public string Description { get; set; }

        public virtual ICollection<UserRole> Users { get; set; }

        /// <summary>
        /// Услуги из бэйджей которые доступны для сканирования членам данной роли
        /// </summary>
        public virtual ICollection<BadgeServiceApplicationRole> BadgeServices { get; set; }

    }

    [Table("AspNetSentEmail")]
    public class SentEmail : BasicItemWithCreator
    {
        [Required]
        public string Destination { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public SentEmailStatus Status { get; set; } = SentEmailStatus.Ok;
        public string ErrorMessage { get; set; }
    }
    public enum SentEmailStatus : byte
    {
        Ok,
        Error
    }

    [Table("AspNetSentSms")]
    public class SentSms : BasicItemWithCreator
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Text { get; set; }

        public int? StatusCod { get; set; }

        public string StatusDescription { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public string DeliveredStatus { get; set; }

        public string DeliveredShortMessage { get; set; }

        //public void UpdateFromSmsResponse(SmsResponse response)
        //{
        //    StatusCod = response.result.status.code;
        //    StatusDescription = response.result.status.description;
        //}

        //public void UpdateFromSmsDeliveryReport(SmsDeliveryReport report)
        //{
        //    DeliveredDate = DateTime.Now;
        //    DeliveredStatus = report.status;
        //    DeliveredShortMessage = report.short_message;
        //}
    }

    /// <summary>
    /// Страна
    /// </summary>
    public class UserCountry : DDLitem
    {

    }

    /// <summary>
    /// Регион России
    /// </summary>
    public class UserRussiaSubject : DDLitem
    {

    }

    /// <summary>
    /// Компетенция
    /// </summary>
    public class UserCompetence : DDLitem
    {

    }

    #region BaseIdentity
    public class UserClaim : IdentityUserClaim<Guid>
    {

    }

    public class UserRole : IdentityUserRole<Guid>
    {
        public virtual ApplicationUser User { get; set; }

        public virtual ApplicationRole Role { get; set; }
    }

    public class UserLogin : IdentityUserLogin<Guid>
    {

    }

    public class RoleClaim : IdentityRoleClaim<Guid>
    {

    }

    public class UserToken : IdentityUserToken<Guid>
    {

    }
    #endregion
}
