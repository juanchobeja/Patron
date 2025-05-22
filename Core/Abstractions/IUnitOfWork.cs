using Core.Entities;

namespace Core.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        ILogRepository LogRepository { get; }
        IFileTrazabilidadRepository TrazabilidadRepository { get; }
        IFormatFileRepository FormatFileRepository { get; }
        IConfiguradorIntegradorRepository ConfiguradorIntegradorRepository { get; }
        IConsecutivoEtiquetaRepository ConsecutivoEtiquetaRepository { get; }
        IFestivosRepository FestivosRepository { get; }
        IRepository<Batch> BatchRepository { get; }
        IRepository<Solicitud> SolicitudRepository { get; }
        IRepository<Documento> DocumentoRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        void BeginTransaction();
    }
}

