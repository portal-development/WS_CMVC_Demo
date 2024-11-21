using System.ComponentModel.DataAnnotations;

namespace WS_CMVC_Demo.Models.UsersViewModels
{
    public class EmailSenderViewModel
    {
        [Required]
        [Display(Name = "Список идентификаторов пользователей в столбец")]
        public string Users { get; set; }

        [Required]
        [Display(Name = "Тема сообщения")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Текст сообщения")]
        public string Message { get; set; }
    }
}
