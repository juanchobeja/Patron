using Application.Commands;
using FluentValidation;

namespace Application.Validators
{
    public class CreateLogCommandValidator : AbstractValidator<CreateLogCommand>
    {
        public CreateLogCommandValidator()
        {
            RuleFor(x => x.Archivo)
                .NotEmpty().WithMessage("El nombre del archivo es obligatorio")
                .MaximumLength(500).WithMessage("El nombre del archivo no puede exceder 500 caracteres");

            RuleFor(x => x.Mensaje)
                .NotEmpty().WithMessage("El mensaje es obligatorio");

            RuleFor(x => x.Servicio)
                .NotEmpty().WithMessage("El servicio es obligatorio")
                .MaximumLength(20).WithMessage("El servicio no puede exceder 20 caracteres");

            RuleFor(x => x.Level)
                .NotEmpty().WithMessage("El nivel es obligatorio")
                .Must(level => new[] { "INFO", "WARNING", "ERROR" }.Contains(level))
                .WithMessage("El nivel debe ser INFO, WARNING o ERROR");
        }
    }
}