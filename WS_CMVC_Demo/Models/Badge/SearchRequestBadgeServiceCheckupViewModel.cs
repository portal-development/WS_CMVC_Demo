using System.ComponentModel.DataAnnotations;
using WS_CMVC_Demo.Models.UserPackagesViewModels;
using X.PagedList;

namespace WS_CMVC_Demo.Models.Badge
{
    public class SearchRequestBadgeServiceCheckupViewModel
    {
        [Display(Name = "Любой тип контрольной точки")]
        public int? BadgeServiceId { get; set; }

        [Display(Name = "Любой статус попытки")]
        public ServiceCheckupType? Type { get; set; }

        [Display(Name = "Сортировать по:")]
        public OrderBy Order { get; set; } = OrderBy.DateDesc;

        [Display(Name = "Показывать по:")]
        public PageSize PageSize { get; set; } = PageSize.p20;

        [Display(Name = "Отметку поставил")]
        public string Searchstring { get; set; }

        [Display(Name = "Дата начала")]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Дата конца")]
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Подкатегория пользователя при реистрации
        /// </summary>
        [Display(Name = "Любая подкатегория")]
        public IEnumerable<int> UserSubcategoryId { get; set; } = new List<int>();

        /// <summary>
        /// Идентификатор страны
        /// </summary>
        [Display(Name = "Любая страна")]
        public IEnumerable<int> CountryId { get; set; } = new List<int>();

        /// <summary>
        /// Идентификатор региона
        /// </summary>
        [Display(Name = "Любой регион")]
        public IEnumerable<int> RussiaSubjectId { get; set; } = new List<int>();

        /// <summary>
        /// Идентификатор компетенции
        /// </summary>
        [Display(Name = "Любая компетенция")]
        public IEnumerable<int> CompetenceId { get; set; } = new List<int>();

        public IPagedList<BadgeServiceCheckup> Requests { get; set; }
    }

    public enum OrderBy
    {
        /// <summary>
        /// Дате добавления (сначала новые)
        /// </summary>
        [Display(Name = "Сортировка по дате (сначала новые)")]
        DateDesc,

        /// <summary>
        /// Дате добавления (сначала новые)
        /// </summary>
        [Display(Name = "Сортировка по дате (сначала старые)")]
        Date
    }
}
