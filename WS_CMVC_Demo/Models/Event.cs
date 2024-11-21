using System.ComponentModel.DataAnnotations;
using WS_CMVC_Demo.Models.Service;

namespace WS_CMVC_Demo.Models
{
    public class Event : BasicItem
    {
        [Required, StringLength(128), Display(Name = "Наименование мероприятия")]
        public string Title { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата начала мероприятия")]
        public DateTime DateStart { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата окончания мероприятия")]
        public DateTime DateEnd { get; set; }

        /// <summary>
        /// Подкатегории пользователей, доступные в данном мероприятии
        /// </summary>
        public virtual ICollection<UserSubcategoryEvent> UserSubcategoryEvents { get; set; }

        //Брони услуг на данное мероприятие
        public virtual ICollection<UserPackageService> UserPackageServices { get; set; }
    }
}
