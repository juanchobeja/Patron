using Application.Abstractions.Messaging;
using Core.Entities;

namespace Application.Queries
{
    public record GetLogsQuery(DateTime StartDate, DateTime EndDate) : IQuery<IEnumerable<LogRecord>>;
}