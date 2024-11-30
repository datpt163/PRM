using Capstone.Domain.Entities;
using Capstone.Application.Module.Positions.Command;
using Capstone.Application.Module.Positions.Response;
using Capstone.Infrastructure.Repository;
using MediatR;

namespace Capstone.Application.Module.Positions.CommandHandle
{
    public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, PositionDto>
    {
        private readonly IRepository<Position> _positionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePositionCommandHandler(IRepository<Position> positionRepository, IUnitOfWork unitOfWork)
        {
            _positionRepository = positionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PositionDto> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
        {
            var position = new Position
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

             _positionRepository.Add(position);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new PositionDto
            {
                Id = position.Id,
                Title = position.Title,
                Description = position.Description,
                CreatedAt = position.CreatedAt,
                UpdatedAt = position.UpdatedAt,
                IsDeleted = position.IsDeleted,
                CreatedBy = position.CreatedBy,
                UpdatedBy = position.UpdatedBy
            };
        }
    }
}
