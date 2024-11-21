using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WS_CMVC_Demo.Models.Service;
using X.PagedList;

namespace WS_CMVC_Demo.Models.UserPackagesViewModels
{
    public class UserPackageViewModel
    {
        public string PackageName { get; set; }

        public string PackageDescription { get; set; }

        //Есть ли выбор в каком то из типов услуг внутри пакета: Настраиваемый или фиксированый пакет
        public bool Fixed { get; set; }

        //Услуги
        public IList<UserPackageService> Services { get; set; } = new List<UserPackageService>();

        //Количество свободных мест по квотам
        public int FreePackagesCount { get; set; }

        //Выбран ли уже пакет
        public bool Choosed { get; set; } = false;

        [Required]
        public int PackageId { get; set; }

        //Начало действия пакета
        public DateTime StartDate { get; set; }

        //Конец действия пакета
        public DateTime EndDate { get; set; }

        public int EventId { get; set; }
    }

    public class UserPackageService
    {
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }

        [Required]
        public int ServiceId { get; set; }

        //Начало действия выбранной услуги
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime StartDate { get; set; }

        //Конец действия выбранной услуги
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime EndDate { get; set; }

        //Начало действия услуги, зашито в пакете
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}")]
        public DateTime ServiceStartDate { get; set; }

        //Окончание действия услуги, зашито в пакете
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy}")]
        public DateTime ServiceEndDate { get; set; }

        //Есть ли в пакете другие услуги данного типа
        public bool RadioEnabled { get; set; }

        //Либо других услуг данного типа в пакете нет, либо услуга уже выбрана из перечня
        [Required]
        public bool RadioChecked { get; set; }

        //Название типа услуги
        public string RadioGroupName { get; set; }

        //Возможность изменять даты
        public bool CanChangeDates { get; set; } = false;

        public int? MinimalDaysCount { get; set; }

        public bool IsHotel { get; set; } = false;

        public bool ShowDates { get; set; } = true;

        public int FreePlaces { get; set; }
    }

    public class EventsWithRequestsViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; }

        //Начало мероприятия
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        //Конец мероприятия
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool HasRequest { get; set; }

        [Display(Name = "Выбранный пакет")]
        public string PackageName { get; set; }

        public int PackageId { get; set; }

        [Display(Name = "Статус заявки")]
        public UserPackageServiceStatus Status { get; set; }

        /// <summary>
        /// Имеет ли пользователь подкатегорию для данного мероприятия
        /// </summary>
        public bool HasUserSubCategory { get; set; } = false;

    }

    public class UserSubCategoryEventViewModel
    {
        public string EventTitle { get; set; }

        [Display(Name = "Категория участника")]
        public int UserSubCategoryEventId { get; set; }

        public List<SelectListItem> SubCategories { get; set; }
    }

    public class SortedRequestViewModel
    {
        /// <summary>
        /// Статус бронирования
        /// </summary>
        [Display(Name = "Статус бронирования:")]
        public UserPackageServiceStatus? Status { get; set; }

        [Display(Name = "Сортировать по:")]
        public OrderBy Order { get; set; } = OrderBy.DateDesc;

        [Display(Name = "Показывать по:")]
        public PageSize PageSize { get; set; } = PageSize.p20;

        public string Searchstring { get; set; }

        public IPagedList<RequestViewModel> Requests { get; set; }

        public Guid? UserId { get; set; }
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
        Date,

        /// <summary>
        /// Делегациям
        /// </summary>
        [Display(Name = "Сортировка по делегациям")]
        Delagate
    }

    public enum PageSize
    {
        [Display(Name = "Показывать по: 10")]
        p10 = 10,

        [Display(Name = "Показывать по: 20")]
        p20 = 20,

        [Display(Name = "Показывать по: 50")]
        p50 = 50,

        [Display(Name = "Показывать по: 100")]
        p100 = 100
    }

    public class RequestViewModel
    {
        [Display(Name = "Название пакета")]
        public string PackageName { get; set; }

        [Display(Name = "Дата изменения")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yy HH:mm}")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "ФИО"), StringLength(128)]
        public string FIO { get; set; }

        [Display(Name = "Руководитель делегации"), StringLength(128)]
        public string DelegationFIO { get; set; }

        [Display(Name = "Статус")]
        public UserPackageServiceStatus Status { get; set; }

        public Guid UserId { get; set; }

        public Guid? DelegationUserId { get; set; }

        public int PackageId { get; set; }

        public bool RightPackage { get; set; }
    }

    public class DownloadRequestViewModel
    {
        public List<string> Columns { get; set; } = new List<string>();

        [Display(Name = "Начало выгрузки")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Конец выгрузки")]
        public DateTime EndDate { get; set; }
    }
}
