using Core.Entities;
using Infrastructure.Connection;

namespace Infrastructure.Repositories
{
    public class SolicitudRepository : BaseRepository<Solicitud>
    {
        public SolicitudRepository(ISqlConnectionFactory factory) : base(factory) { }

        public override async Task AddAsync(Solicitud s, CancellationToken ct = default)
        {
            const string sql = @"
INSERT INTO Solicitud(Batch_Id, Solicitud_Id, Etiqueta, Masivo, NombreArchivoOriginal)
VALUES(@BatchId, @SolicitudId, @Etiqueta, @Masivo, @NombreArchivoOriginal)";

            await ExecuteInsertAsync(sql, s, ct);
        }
    }    
}
