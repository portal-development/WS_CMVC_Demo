using System.ComponentModel.DataAnnotations;

namespace WS_CMVC_Demo.Models.Service
{
    public class Hotel : BasicItem
    {
        /// <summary>
        /// Наименование отеля
        /// </summary>
        [Required, StringLength(128), Display(Name = "Название")]
        public string Name { get; set; }

        /// <summary>
        /// Класс отеля, звезд
        /// </summary>
        [Display(Name = "Класс отеля, звезд")]
        public byte? CountOfStars { get; set; }

        /// <summary>
        /// Адрес отеля
        /// </summary>
        [StringLength(256), Display(Name = "Адрес")]
        public string Address { get; set; }

        /// <summary>
        /// Варианты размещения
        /// </summary>
        public virtual ICollection<HotelOption> HotelOptions { get; set; }
    }

    public class HotelOption : BasicItem
    {

        public virtual Hotel Hotel { get; set; }

        [Required]
        public int HotelId { get; set; }

        /// <summary>
        /// Наименование варианта размещения
        /// </summary>
        [Required]
        [StringLength(128), Display(Name = "Наименование")]
        public string Name { get; set; }

        /// <summary>
        /// Описание варианта размещения
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }

        /// <summary>
        /// Количество мест в номере
        /// </summary>
        [Display(Name = "Количество мест в номере")]
        public byte NumberOfSeats { get; set; } = 1;

        /// <summary>
        /// Услуги проживания в данном варианте размещения
        /// </summary>
        public virtual ICollection<Service> Services { get; set; }
    }
}
