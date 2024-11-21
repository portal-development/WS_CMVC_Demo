using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WS_CMVC_Demo.Models.PackagesViewModels
{
    public class PackageViewModel
    {
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Подкатегории")]
        //Выбранные Подкатегории
        public IEnumerable<int> CheckedUserSubcategories { get; set; } = new List<int>();
    }
}
