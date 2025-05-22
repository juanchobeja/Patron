namespace Core.Abstractions
{
    public interface IConfiguradorIntegradorRepository
    {
        Task<long> GetAndIncrementBatchConsecutiveAsync(CancellationToken ct = default);
        Task<int> GetHoraConfiguradaAsync(string campo, CancellationToken ct = default);
    }
}
