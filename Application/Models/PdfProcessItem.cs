using Core.Entities;

namespace Application.Models
{   

    public record PdfProcessItem(
        Cliente Cliente,
        int Origen,
        string Archivo
    );

}
