using Core.Entities;

namespace Core.Abstractions
{
    public interface ILogRepository
    {
        Task AddAsync(LogRecord log);
        Task<IEnumerable<LogRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
