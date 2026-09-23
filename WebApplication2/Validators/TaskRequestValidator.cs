using FluentValidation;
using TaskScheduler.MinimalAPI.Models.DTOs;

namespace TaskScheduler.MinimalAPI.Validators;

public class TaskRequestValidator : AbstractValidator<TaskRequest>
{
    public TaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название задачи обязательно")
            .MaximumLength(200).WithMessage("Название не может быть длиннее 200 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не может быть длиннее 1000 символов");

        RuleFor(x => x.DueDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("Срок выполнения должен быть позже даты начала");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Дата начала обязательна");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5)
            .WithMessage("Приоритет должен быть от 1 до 5");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Категория обязательна");

        RuleForEach(x => x.Tags)
            .Must(kvp => !string.IsNullOrEmpty(kvp.Key))
            .WithMessage("Ключ тега не может быть пустым");
    }
}