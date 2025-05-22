using Application.Abstractions.Messaging;
using Core.Entities;

namespace Application.Commands
{
    public record ProcessGuanePdfCommand(
        Cliente Cliente,
        TipoOrigen Origen,
        string ArchivoPath
    ) : ICommand;
}
