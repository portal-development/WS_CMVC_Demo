using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WS_CMVC_Demo.Models.Badge;
using WS_CMVC_Demo.Models.Service;

namespace WS_CMVC_Demo.Models
{
    /// <summary>
    /// Категория пользователя на мероприятии
    /// </summary>
    public class UserCategory : BasicItem
    {
        [Required, StringLength(128), Display(Name = "Наименование категории")]
        public string Title { get; set; }

        /// <summary>
        /// Подкатегории в данной категории
        /// </summary>
        public virtual ICollection<UserSubcategory> Subcategories { get; set; }

        /// <summary>
        /// Идентификатор для вставки в ссылку при регистрации
        /// </summary>
        [NotMapped]
        public string IdForRegister
        {
            get => (Id * 3265).ToString("X");
            set
            {
                var i = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
                if (i % 3265 != 0)
                {
                    throw new Exception("Неверный идентификатор ссылки");
                }
                Id = i / 3265;
            }
        }
    }

    /// <summary>
    /// Подкатегория пользователя на мероприятии
    /// </summary>
    public class UserSubcategory : BasicItem
    {
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [Display(Name = "Категория")]
        public virtual UserCategory Category { get; set; }

        [Required, StringLength(128), Display(Name = "Наименование подкатегории")]
        public string Title { get; set; }

        /// <summary>
        /// Исключенные особые поля
        /// </summary>
        [Display(Name = "Особые поля для заполнения")]
        public string[] IncludeProperties { get; set; }

        /// <summary>
        /// Список особых полей доступных для исключения
        /// </summary>
        public readonly string[] ExcludebleProperties = { nameof(ApplicationUser.CountryId), nameof(ApplicationUser.RussiaSubjectId), nameof(ApplicationUser.CompetenceId), nameof(ApplicationUser.CompanyName) };

        /// <summary>
        /// Исключенные особые поля
        /// </summary>
        public IEnumerable<string> ExcludeProperties => ExcludebleProperties.Except(IncludeProperties ?? Array.Empty<string>());

        /// <summary>
        /// Мероприятия в которых фигурирует данная подкатегория
        /// </summary>
        public virtual ICollection<UserSubcategoryEvent> Events { get; set; }

        /// <summary>
        /// Флаг автоматического назначения пакета при регистрации
        /// </summary>
        public bool AutoPackage { get; set; } = false;

        /// <summary>
        /// Флаг, обозначающий что пакеты для этой подкатегории оплачиваются участником
        /// </summary>
        [Display(Name = "Платные пакеты")]
        public bool NotFreePackage { get; set; } = true;

        [Display(Name = "Цвет бейджа")]
        public int? BadgeColorId { get; set; }

        [Display(Name = "Цвет бейджа")]
        public virtual BadgeColor BadgeColor { get; set; }

        [Display(Name = "Цвет текста на бейдже")]
        public int? BadgeTextColorId { get; set; }

        [Display(Name = "Цвет текста на бейдже")]
        public virtual BadgeColor BadgeTextColor { get; set; }

        [Display(Name = "Наименование подкатегории для печати бейджа"), StringLength(128)]
        public string TitleForPrint { get; set; }
    }

    /// <summary>
    /// Связь подкатегории пользователя с мероприятием
    /// </summary>
    public class UserSubcategoryEvent : BasicItem
    {
        public int EventId { get; set; }

        public int UserSubcategoryId { get; set; }

        public virtual Event Event { get; set; }

        public virtual UserSubcategory UserSubcategoryCategory { get; set; }

        public virtual ICollection<UserSubcategoryEventPackage> UserSubcategoryEventPackages { get; set; }
    }
}
