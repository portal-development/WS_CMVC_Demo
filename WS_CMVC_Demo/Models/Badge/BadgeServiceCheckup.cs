using System.ComponentModel.DataAnnotations;

namespace WS_CMVC_Demo.Models.Badge
{
    /// <summary>
    /// Отметка об использовании (попытке использования) услуги бейджа
    /// </summary>
    public class BadgeServiceCheckup : BasicItemWithCreator
    {
        /// <summary>
        /// Идентификатор услуги в бейдже
        /// </summary>
        [Display(Name = "Контрольная точка")]
        public int BadgeServiceId { get; set; }

        /// <summary>
        /// Услуга в бейдже
        /// </summary>
        [Display(Name = "Контрольная точка")]
        public virtual BadgeService BadgeService { get; set; }

        /// <summary>
        /// Идентификатор пользователя, воспользовавшегося услугой
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Пользователь, воспользовавшийся услугой
        /// </summary>
        [Display(Name = "Пользователь")]
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Статус попытки (одобрена, отклонена ...)
        /// </summary>
        [Display(Name = "Статус попытки")]
        public ServiceCheckupType Type { get; set; } = ServiceCheckupType.Approve;

        [Display(Name = "Отметку поставил")]
        public override ApplicationUser Creator { get => base.Creator; set => base.Creator = value; }

        [Display(Name = "Дата")]
        public override DateTime CreateDate { get => base.CreateDate; internal set => base.CreateDate = value; }
    }

    /// <summary>
    /// Статус отметки об использовании (попытке использования) услуги бейджа
    /// </summary>
    public enum ServiceCheckupType : byte
    {
        /// <summary>
        /// Одобрено
        /// </summary>
        [Display(Name = "Одобрено")]
        Approve = 0,

        /// <summary>
        /// Отменено
        /// </summary>
        [Display(Name = "Отменено")]
        Canceled = 64,

        /// <summary>
        /// У участника нет доступа к услуге
        /// </summary>
        [Display(Name = "У участника нет доступа к услуге")]
        Forbid = 128,

        /// <summary>
        /// В данный момент услуга недоступна для участника
        /// </summary>
        [Display(Name = "В данный момент услуга недоступна для участника")]
        ForbidTime = 129,

        /// <summary>
        /// Договор по услуге не оплачен
        /// </summary>
        [Display(Name = "Договор по услуге не оплачен")]
        ForbidPay = 130,

        /// <summary>
        /// Повторное использование одноразовой услуги
        /// </summary>
        [Display(Name = "Повторное использование одноразовой услуги")]
        ForbidSingle = 131,

        /// <summary>
        /// Срок повторного использования услуги еще не подошел
        /// </summary>
        [Display(Name = "Срок повторного использования услуги еще не подошел")]
        ForbidPeriodic = 132
    }
}
