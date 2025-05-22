using Application.Commands;
using FluentValidation;

namespace Application.Validators
{
    public class ProcessPdfCommandValidator : AbstractValidator<ProcessPdfCommand>
    {
        public ProcessPdfCommandValidator()
        {
            RuleFor(x => x.Archivo)
                .NotEmpty().WithMessage("La ruta del archivo no puede estar vacía.")
                .Must(File.Exists).WithMessage(cmd => $"El archivo '{cmd.Archivo}' no existe.")
                .Must(path => Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                .WithMessage("El archivo debe tener extensión .pdf.")
                .Must(path =>
                {
                    try
                    {
                        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                        return stream.Length > 0;
                    }
                    catch { return false; }
                }).WithMessage("El archivo no se puede abrir o está vacío.");

            RuleFor(x => x.Origen)
                .GreaterThan(0).WithMessage("El valor de 'Origen' debe ser mayor a cero.");

            RuleFor(x => x.Cliente)
                .NotNull().WithMessage("El Cliente no puede ser nulo.");
        }
    }
}
