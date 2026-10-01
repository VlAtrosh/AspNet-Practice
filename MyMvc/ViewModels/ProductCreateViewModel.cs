using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.ViewModels;

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "Введите название")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название от 2 до 100 символов")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите цену")]
    [Range(0.01, 1_000_000, ErrorMessage = "Цена от 0.01 до 1 000 000")]
    [Display(Name = "Цена, ₽")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Введите категорию")]
    [StringLength(50, ErrorMessage = "Категория до 50 символов")]
    [Display(Name = "Категория")]
    public string Category { get; set; } = string.Empty;
}