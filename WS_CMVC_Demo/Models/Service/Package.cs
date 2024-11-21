using System.ComponentModel.DataAnnotations;

namespace WS_CMVC_Demo.Models.Service
{
    public class Package : BasicItem
    {
        [StringLength(128), Display(Name = "Название пакета")]
        public string Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }

        /// <summary>
        /// Связь с подкатегориями пользователей, которым доступен пакет
        /// </summary>
        public virtual ICollection<UserSubcategoryEventPackage> UserSubcategoryEventPackages { get; set; }

        /// <summary>
        /// Услуги, входящие в пакет
        /// </summary>
        public virtual ICollection<PackageService> PackageServices { get; set; }
    }

    /// <summary>
    /// Привязка пакета к объекту мероприятие-подкатегория
    /// </summary>
    public class UserSubcategoryEventPackage
    {
        [Key]
        public int PackageId { get; set; }

        [Key]
        public int UserSubcategoryEventId { get; set; }

        public virtual Package Package { get; set; }

        public virtual UserSubcategoryEvent UserSubcategoryEvent { get; set; }
    }

    /// <summary>
    /// Услуга, представленная в пакете
    /// </summary>
    public class PackageService : BasicItem
    {
        /// <summary>
        /// Пакет
        /// </summary>
        [Display(Name = "Пакет")]
        public virtual Package Package { get; set; }

        [Required]
        public int PackageId { get; set; }

        /// <summary>
        /// Услуга
        /// </summary>
        [Display(Name = "Услуга")]
        public virtual Service Service { get; set; }

        [Required]
        public int ServiceId { get; set; }

        /// <summary>
        /// Крайняя ранняя дата начала оказания услуги в пакете
        /// </summary>
        [Required, Display(Name = "Дата начала оказания услуги")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Крайняя поздняя дата окончания оказания услуги в пакете
        /// </summary>
        [Required, Display(Name = "Дата окончания оказания услуги")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}")]
        public DateTime FinishDate { get; set; }

        /// <summary>
        /// Минимальное количество дней для брони
        /// </summary>
        [Required, Display(Name = "Минимальное количество дней для брони")]
        public int MinimalDaysCount { get; set; } = 1;

        /// <summary>
        /// Заявки пользователей на бронирование данной услуги
        /// </summary>
        public virtual ICollection<UserPackageService> UserPackageServices { get; set; }
    }

    /// <summary>
    /// Услуга из пакета, забронированная пользователем
    /// </summary>
    public class UserPackageService : BasicItemWithCreator
    {
        [Required]
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        //Меропритяие, на которое бронируется услуга
        //Введено из-за связи пакета и мероприятия многие-ко-многим
        public virtual Event Event { get; set; }

        [Required]
        public int EventId { get; set; }

        /// <summary>
        /// Услуга из пакета
        /// </summary>
        public virtual PackageService PackageService { get; set; }

        [Required]
        public int PackageServiceId { get; set; }

        /// <summary>
        /// Дата начала брони услуги в пакете
        /// </summary>
        [Required, Display(Name = "Дата начала брони")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания брони услуги в пакете
        /// </summary>
        [Required, Display(Name = "Дата окончания брони")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}")]
        public DateTime FinishDate { get; set; }

        /// <summary>
        /// Статус бронирования
        /// </summary>
        [Required, Display(Name = "Статус")]
        public UserPackageServiceStatus Status { get; set; } = UserPackageServiceStatus.draft;

    }

    /// <summary>
    /// Статус бронирования услуги из пакета пользователем
    /// </summary>
    public enum UserPackageServiceStatus : byte
    {
        /// <summary>
        /// Черновик
        /// </summary>
        [Display(Name = "Черновик")]
        draft = 0,

        /// <summary>
        /// Заявка подана
        /// </summary>
        [Display(Name = "Заявка подана")]
        book = 1,

        /// <summary>
        /// Бронь подтверждена
        /// </summary>
        [Display(Name = "Бронь подтверждена")]
        accepted = 2,

        /// <summary>
        /// Заявка отклонена
        /// </summary>
        [Display(Name = "Заявка отклонена")]
        declined = 3,

        /// <summary>
        /// Отправлен договор
        /// </summary>
        [Display(Name = "Отправлен договор")]
        contracted = 4,

        /// <summary>
        /// Оплачено
        /// </summary>
        [Display(Name = "Оплачено")]
        completed = 5,
    }

}
