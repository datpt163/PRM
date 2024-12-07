using AutoMapper;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Module.Auths.Response;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Auths.QueryHandle
{
    public class RoleDetailQueryHandle : IRequestHandler<RoleDetailQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public RoleDetailQueryHandle(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseMediator> Handle(RoleDetailQuery request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Roles.Find(x => x.Id == request.Id).Include(c => c.Permissions).FirstOrDefaultAsync();
            if (role == null)
                return new ResponseMediator(Messages.role_not_found, null, 404);
            
             return new ResponseMediator("", _mapper.Map<RoleDTO>(role));
        }
    }
}
