using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WS_CMVC_Demo.Models.Badge
{
    /// <summary>
    /// Цвет бейджа
    /// </summary>
    public class BadgeColor : BasicItem
    {
        /// <summary>
        /// Код цвета int
        /// </summary>
        [Range(0, 0xFFFFFF)]
        [Display(Name = "Цвет")]
        public int ColorGraph { get; set; } = 0xFF0000;

        /// <summary>
        /// Код цвета html
        /// </summary>
        [NotMapped]
        [Display(Name = "Цвет")]
        public string ColorGraphHex
        {
            get => "#" + ColorGraph.ToString("X6");
            set => ColorGraph = int.Parse(value.Substring(1, 6), System.Globalization.NumberStyles.HexNumber);
        }
    }
}
