using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Abstractions
{
    public interface IFestivosRepository
    {
        Task<DateTime> ObtenerSiguienteDiaHabilAsync(DateTime desdeFecha, CancellationToken ct = default);
    }

}
