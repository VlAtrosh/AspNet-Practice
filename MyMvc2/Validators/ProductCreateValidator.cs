using FluentValidation;
using MyMvcApp.ViewModels;

namespace MyMvcApp.Validators;

public class ProductCreateValidator : AbstractValidator<ProductCreateViewModel>
{
    public ProductCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя обязательно")
            .MinimumLength(2).WithMessage("Имя от 2 символов")
            .MaximumLength(100).WithMessage("Имя до 100 символов");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Цена должна быть больше 0")
            .LessThan(1_000_000).WithMessage("Цена не может быть больше 1 000 000");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Категория обязательна")
            .MaximumLength(50).WithMessage("Категория до 50 символов");

        // Условное правило из конспекта
        When(x => x.Price > 10_000, () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(5)
                .WithMessage("Дорогие товары (от 10 000 ₽) требуют названия минимум 5 символов");
        });
    }
}