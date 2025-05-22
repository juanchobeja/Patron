using Application.Abstractions.Messaging;
using Core.Entities;

namespace Application.Commands
{
    public record ProcessPdfCommand(
        Cliente Cliente,
        int Origen,
        string Archivo
    ) : ICommand;
}
