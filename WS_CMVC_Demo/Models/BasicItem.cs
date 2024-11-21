using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WS_CMVC_Demo.Models
{
    public interface IBasicItem
    {
        int Id { get; set; }
    }

    public interface IItemWithDate
    {
        /// <summary>
        /// Время добавления записи
        /// </summary>
        [Required]
        DateTime CreateDate { get; }
    }

    public interface IItemWithCreator
    {
        /// <summary>
        /// Пользователь добавивший запись
        /// </summary>
        [Required]
        ApplicationUser Creator { get; set; }
    }

    public class BasicItem : IBasicItem
    {
        public int Id { get; set; }
    }

    public class BasicItemWithDate : BasicItem, IItemWithDate
    {
        /// <summary>
        /// Время добавления записи
        /// </summary>
        [Required]
        public virtual DateTime CreateDate { get; internal set; } = DateTime.Now;
    }

    public class BasicItemWithCreator : BasicItemWithDate, IItemWithCreator
    {
        [Required]
        public virtual Guid CreateUserId { get; set; }

        /// <summary>
        /// Пользователь добавивший запись
        /// </summary>
        [ForeignKey("CreateUserId")]
        public virtual ApplicationUser Creator { get; set; }
    }

    /// <summary>
    /// Класс строки для выпадающего списка значений
    /// </summary>
    public abstract class DDLitem : BasicItem
    {
        /// <summary>
        /// Наименование
        /// </summary>
        [Required, StringLength(128), Display(Name = "Наименование")]
        public string Title { get; set; }

        /// <summary>
        /// Порядок
        /// </summary>
        [Display(Name = "Порядок")]
        public int? Order { get; set; }
    }
}
