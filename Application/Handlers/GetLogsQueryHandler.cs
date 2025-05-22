using Application.Abstractions.Messaging;
using Application.Queries;
using Core.Abstractions;
using Core.Common;
using Core.Entities;

namespace Application.Handlers
{
    public class GetLogsQueryHandler : IQueryHandler<GetLogsQuery, IEnumerable<LogRecord>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLogsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<LogRecord>>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
        {
            var logs = await _unitOfWork.LogRepository.GetByDateRangeAsync(request.StartDate, request.EndDate);
            return Result.Success(logs);
        }
    }
}