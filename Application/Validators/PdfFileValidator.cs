using FluentValidation;
using WebSupergoo.ABCpdf11;

namespace Application.Validators
{

    public class PdfFileValidator : AbstractValidator<FileInfo>
    {
        public PdfFileValidator()
        {
            RuleFor(x => x)
                .Must(BeValidPdf).WithMessage("El archivo no es un PDF válido")
                .Must(NotBeCorrupted).WithMessage("El PDF está corrupto")
                .Must(NotBePasswordProtected).WithMessage("El PDF está protegido con contraseña")
                .Must(HaveValidSize).WithMessage("El tamaño del PDF no es válido");

            RuleFor(x => x.Length)
                .LessThanOrEqualTo(50 * 1024 * 1024) // 50MB
                .WithMessage("El archivo excede el tamaño máximo permitido (50MB)");
        }

        private bool BeValidPdf(FileInfo file)
        {
            try
            {
                return file.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) &&
                       file.Exists;
            }
            catch
            {
                return false;
            }
        }

        private bool NotBeCorrupted(FileInfo file)
        {
            using var reader = new Doc();
            try
            {
                reader.Read(file.FullName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool NotBePasswordProtected(FileInfo file)
        {
            using var reader = new Doc();
            try
            {
                reader.Read(file.FullName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool HaveValidSize(FileInfo file)
        {
            return file.Length > 0 && file.Length <= 50 * 1024 * 1024;
        }
    }
}
