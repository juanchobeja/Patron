using Core.Abstractions;
using Core.Entities;
using Infrastructure.Connection;
using Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
{
    private readonly ISqlConnectionFactory _factory;
    private SqlConnection _conn;
    private SqlTransaction _tx;
    private bool _disposed;

    public ILogRepository LogRepository { get; }
    public IFileTrazabilidadRepository TrazabilidadRepository { get; }
    public IFormatFileRepository FormatFileRepository { get; }
    public IConfiguradorIntegradorRepository ConfiguradorIntegradorRepository { get; }
    public IConsecutivoEtiquetaRepository ConsecutivoEtiquetaRepository { get; }
    public IFestivosRepository FestivosRepository { get; }
    public IRepository<Batch> BatchRepository { get; }
    public IRepository<Solicitud> SolicitudRepository { get; }
    public IRepository<Documento> DocumentoRepository { get; }

    public UnitOfWork(ISqlConnectionFactory factory)
    {
        _factory = factory;
        Initialize();
        var txFactory = new TransactionConnectionFactory(_conn, _tx);

        LogRepository = new LogRepository(txFactory);
        TrazabilidadRepository = new FileTrazabilidadRepository(txFactory);
        FormatFileRepository = new FormatFileRepository(txFactory);
        ConfiguradorIntegradorRepository = new ConfiguradorIntegradorRepository(txFactory);
        ConsecutivoEtiquetaRepository = new ConsecutivoEtiquetaRepository(txFactory);
        FestivosRepository = new FestivosRepository(txFactory); 
        BatchRepository = new BatchRepository(txFactory);
        SolicitudRepository = new SolicitudRepository(txFactory);
        DocumentoRepository = new DocumentoRepository(txFactory);
    }

    private void Initialize()
    {
        _conn = _factory.CreateConnection();
        _conn.Open();
        _tx = _conn.BeginTransaction();
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        if (_tx == null) throw new InvalidOperationException("Transacción finalizada");
        try
        {
            await _tx.CommitAsync(ct);
            return 1;
        }
        catch
        {
            if (_tx.Connection != null) await _tx.RollbackAsync(ct);
            throw;
        }
        finally
        {
            _tx.Dispose();
            _tx = null!;
        }
    }

    public void BeginTransaction()
    {
        if (_tx != null) throw new InvalidOperationException("Transacción en curso");
        if (_conn == null || _conn.State != ConnectionState.Open) Initialize();
        else _tx = _conn.BeginTransaction();
    }

    //public void Dispose()
    //{
    //    if (!_disposed)
    //    {
    //        if (_tx != null) _tx.Rollback();
    //        _conn?.Dispose();
    //        _disposed = true;
    //    }
    //}

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_tx != null)
                await _tx.RollbackAsync();   // proteger rollback
            _conn?.Dispose();
            _disposed = true;
        }
    }
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
}
