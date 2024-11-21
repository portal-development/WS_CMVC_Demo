using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WS_CMVC_Demo.Models
{
    /// <summary>
    /// Связь пользователя с мероприятием
    /// </summary>
    public class EventUser : IItemWithDate, IItemWithCreator
    {
        [Key]
        public Guid UserId { get; set; }

        [Key]
        public int UserSubcategoryEventId { get; set; }

        /// <summary>
        /// привязка к объекту мероприятие-подкатегория
        /// </summary>
        public virtual UserSubcategoryEvent UserSubcategoryEvent { get; set; }

        public virtual ApplicationUser User { get; set; }

        public DateTime CreateDate { get; protected set; } = DateTime.Now;

        public Guid CreateUserId { get; set; }

        /// <summary>
        /// Пользователь добавивший запись
        /// </summary>
        [ForeignKey("CreateUserId")]
        public virtual ApplicationUser Creator { get; set; }
    }
}
