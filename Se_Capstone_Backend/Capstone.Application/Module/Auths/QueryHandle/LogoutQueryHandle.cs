using Capstone.Api.Module.Auths.Request;
using Capstone.Application.Module.Auths.Response;
using Capstone.Application.Resources;
using Capstone.Domain.Module.Auth.TokenBlackList;
using MediatR;

namespace Capstone.Application.Module.Auth.QueryHandle
{
    public class LogoutQueryHandle : IRequestHandler<LogoutQuery, LogoutResponse>
    {
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public LogoutQueryHandle(ITokenBlacklistService tokenBlacklistService)
        {
            _tokenBlacklistService = tokenBlacklistService;
        }

        public async Task<LogoutResponse> Handle(LogoutQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return new LogoutResponse { Success = false, ErrorMessage = Messages.token_required };
                }
                var token = request.Token.ToString().Replace("Bearer ", "").Trim();

                await _tokenBlacklistService.BlacklistTokenAsync(token, 403);

                return new LogoutResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new LogoutResponse { Success = false, ErrorMessage = Messages.error_processing_request + ex.Message };
            }
        }
    }
}
