using Application.Abstractions.Messaging;
using Application.Commands;
using Core.Abstractions;
using Core.Common;
using Core.Entities;
using MediatR;

namespace Application.Handlers
{
    public class CreateLogCommandHandler : ICommandHandler<CreateLogCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateLogCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> Handle(CreateLogCommand request, CancellationToken cancellationToken)
        {
            var log = new LogRecord(
                0, // Id se genera en la BD
                DateTime.Now,
                request.Archivo,
                request.Mensaje,
                request.Servicio,
                request.Level,
                request.Exception,
                request.CorrelationId
            );

            await _unitOfWork.LogRepository.AddAsync(log);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new Core.Notifications.LogCreatedEvent(log), cancellationToken);
            return Result.Success();
        }
    }
}