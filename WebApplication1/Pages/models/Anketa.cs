using System.ComponentModel.DataAnnotations;

namespace AnketaApp.Models
{
    public class Anketa
    {
        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите возраст")]
        [Range(1, 120, ErrorMessage = "Возраст от 1 до 120")]
        [Display(Name = "Возраст")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Город")]
        public string? City { get; set; }

        [Display(Name = "О себе")]
        public string? About { get; set; }
    }
}