using Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ProcessGuanePdfCommandValidator : AbstractValidator<ProcessGuanePdfCommand>
    {
        public ProcessGuanePdfCommandValidator()
        {
            RuleFor(x => x.ArchivoPath)
                .NotEmpty().WithMessage("La ruta del archivo no puede estar vacía.")
                .Must(File.Exists).WithMessage(cmd => $"El archivo '{cmd.ArchivoPath}' no existe.")
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
                .IsInEnum().WithMessage("El origen proporcionado no es válido.");

            RuleFor(x => x.Cliente)
                .NotNull().WithMessage("El Cliente no puede ser nulo.");
        }
    }
}
