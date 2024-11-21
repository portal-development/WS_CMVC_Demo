using System.ComponentModel.DataAnnotations;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.Badge;

namespace WS_CMVC_Demo.Models.Service
{
    /// <summary>
    /// Услуга.
    /// </summary>
    /// Может предоставляться в разные временные промежутки в зависимости от квот.
    /// Доступность услуги в пакете проверяется через сопоставление квот с временными рамками PackageService
    public class Service : BasicItem
    {
        /// <summary>
        /// Тип услуги
        /// </summary>
        [Display(Name = "Тип услуги")]
        public virtual ServiceType ServiceType { get; set; }

        [Required]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Наименование услуги
        /// </summary>
        [Required, StringLength(256), Display(Name = "Услуга")]
        public string Name { get; set; }

        /// <summary>
        /// Описание услуги
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }

        /// <summary>
        /// Вариант размещения
        /// </summary>
        [Display(Name = "Размещение")]
        public virtual HotelOption HotelOption { get; set; }

        public int? HotelOptionId { get; set; }

        /// <summary>
        /// Квоты по данной услуге
        /// </summary>
        public virtual ICollection<Quota> Quotas { get; set; }

        /// <summary>
        /// Пакеты, в которых представлена услуга
        /// </summary>
        public virtual ICollection<PackageService> PackageServices { get; set; }

        [Display(Name = "Пиктограмма")]
        public int? BadgeServiceId { get; set; }

        /// <summary>
        /// Пиктограмма на бейдже (иконка)
        /// </summary>
        [Display(Name = "Пиктограмма")]

        public virtual BadgeService BadgeService { get; set; }

    }
    /// <summary>
    /// Тип услуги
    /// </summary>
    public class ServiceType : BasicItem
    {
        [Required, StringLength(128), Display(Name = "Тип услуги")]
        public string Name { get; set; }

        /// <summary>
        /// Услуги данного типа
        /// </summary>
        public virtual ICollection<Service> Services { get; set; }

        /// <summary>
        /// Имеют ли значение даты в данном типе услуги
        /// </summary>
        public bool ShowDates { get; set; } = true;

        /// <summary>
        /// Порядок
        /// </summary>
        [Display(Name = "Порядок")]
        public int? Order { get; set; }
    }

    /// <summary>
    /// Квота оказания услуги
    /// </summary>
    /// Количество доступных единиц услуги в конкретные временные рамки
    public class Quota : BasicItem
    {
        [Display(Name = "Услуга")]
        public virtual Service Service { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required, Display(Name = "Дата начала квоты")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy HH:mm}")]
        public DateTime StartDate { get; set; }

        [Required, Display(Name = "Дата окончания квоты")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy HH:mm}")]
        public DateTime FinishDate { get; set; }

        /// <summary>
        /// Доступно единиц услуги в данное время
        /// </summary>
        [Required, Display(Name = "Доступно")]
        public int AvailableNum { get; set; }

        /// <summary>
        /// Стоимость услуги в сутки в рамках данной квоты
        /// </summary>
        [Required, Display(Name = "Стоимость(сутки)")]
        public int Cost { get; set; } = 0;

        /// <summary>
        /// Зависит ли стоимость услуги от сроков
        /// </summary>
        [Required, Display(Name = "Фикс. стоимость")]
        public bool FixedCost { get; set; } = false;
    }
}
