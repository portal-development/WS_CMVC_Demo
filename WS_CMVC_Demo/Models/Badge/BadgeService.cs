using System.ComponentModel.DataAnnotations;

namespace WS_CMVC_Demo.Models.Badge
{
    /// <summary>
    /// Услуга в бейдже (для проверки по qr кодам)
    /// </summary>
    public class BadgeService : DDLitem
    {
        /// <summary>
        /// URL адрес пиктограммы услуги
        /// </summary>
        [StringLength(128), Display(Name = "URL адрес пиктограммы")]
        public string IcoUrl { get; set; }

        /// <summary>
        /// Тип периодичности
        /// </summary>
        [Display(Name = "Тип периодичности")]
        public BadgeServicePeriodType PeriodType { get; set; } = BadgeServicePeriodType.Multiple;

        /// <summary>
        /// Время периода (для периодического типа)
        /// </summary>
        [Display(Name = "Время периода (для периодического типа)")]
        public TimeSpan? PeriodTime { get; set; }

        /// <summary>
        /// Рекомендованное время начала (например для приемов пищи)
        /// </summary>
        [Display(Name = "Рекомендованное время начала (например для приемов пищи)")]
        public TimeSpan? RecommendedStartTime { get; set; }

        /// <summary>
        /// Рекомендованное время окончания (например для приемов пищи)
        /// </summary>
        [Display(Name = "Рекомендованное время окончания (например для приемов пищи)")]
        public TimeSpan? RecommendedEndTime { get; set; }

        /// <summary>
        /// Проверять доступность по пакету
        /// </summary>
        [Display(Name = "Проверять доступность по пакету")]
        public bool CheckByPackage { get; set; } = false;

        /// <summary>
        /// Роли которые имеют право сканировать услугу
        /// </summary>
        [Display(Name = "Роли которые имеют право сканировать услугу")]
        public virtual ICollection<BadgeServiceApplicationRole> Roles { get; set; }
    }

    /// <summary>
    /// Тип периодичности для услуги бейджа
    /// </summary>
    public enum BadgeServicePeriodType : byte
    {
        /// <summary>
        /// Многоразовая
        /// </summary>
        [Display(Name = "Многоразовая")]
        Multiple,

        /// <summary>
        /// Одноразовая
        /// </summary>
        [Display(Name = "Одноразовая")]
        Single,

        /// <summary>
        /// Периодическая
        /// </summary>
        [Display(Name = "Периодическая")]
        Periodic
    }

    /// <summary>
    /// Связь услуги из бейджа с ролью
    /// </summary>
    public class BadgeServiceApplicationRole
    {
        public int BadgeServiceId { get; set; }

        public virtual BadgeService BadgeService { get; set; }

        public Guid RoleId { get; set; }

        public virtual ApplicationRole Role { get; set; }
    }
}
