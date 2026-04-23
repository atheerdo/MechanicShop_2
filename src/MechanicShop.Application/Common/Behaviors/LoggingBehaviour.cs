using MechanicShop.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

public class LoggingBehaviour<TRequest>(
    ILogger<TRequest> logger,
    IUser user,
    IIdentityService identityService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdentityService _identityService = identityService;

    public async Task Process(TRequest request, CancellationToken ct)
    {
       var requestName = typeof(TRequest).Name;
       var userId = _user.Id ?? string.Empty;
       string? userName = string.Empty;

       if(!string.IsNullOrWhiteSpace(userId))
        {
            userName = await _identityService.GetUserNameAsync(userId);
        }

       _logger.LogInformation("Request: {Name} {@UserId} {@UserName} {@Request}", requestName, userId, userName, request);
    }
}