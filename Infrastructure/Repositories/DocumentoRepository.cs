using Core.Entities;
using Infrastructure.Connection;

namespace Infrastructure.Repositories
{
    public class DocumentoRepository : BaseRepository<Documento>
    {
        public DocumentoRepository(ISqlConnectionFactory factory) : base(factory) { }

        public override async Task AddAsync(Documento documento, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO Documento(Documento_Id, Batch_Id, Folder_Id, RutaImagen, FullPath)
                VALUES(@DocumentoId, @BatchId, @FolderId, @RutaImagen, @FullPath)";

            await ExecuteInsertAsync(sql, documento, ct);
        }
    }

}
