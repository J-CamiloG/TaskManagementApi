using FluentValidation;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Validators
{
    public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto> 
    {
        public UpdateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("El título es requerido")
                .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres");

            RuleFor(x => x.StateId)
                .GreaterThan(0).WithMessage("El StateId debe ser mayor a 0");

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.Now).WithMessage("La fecha de vencimiento debe ser futura")
                .When(x => x.DueDate.HasValue);
        }
    }
}