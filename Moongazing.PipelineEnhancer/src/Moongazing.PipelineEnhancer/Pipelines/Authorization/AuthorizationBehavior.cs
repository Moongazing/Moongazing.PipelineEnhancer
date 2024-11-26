using MediatR;
using Microsoft.AspNetCore.Http;
using Moongazing.PipelineEnhancer.Exceptions;
using Moongazing.PipelineEnhancer.Extensions;

namespace Doing.Retail.Core.Application.Pipelines.Authorization;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISecuredRequest
{
    private readonly IHttpContextAccessor httpContextAccessor;
    public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {

        ICollection<string>? userRoleClaims = httpContextAccessor.HttpContext?.User?.ClaimRoles() ?? throw new AuthorizationException("Claims not found.");



        bool isMatchedAUserRoleClaimWithRequestRoles = !userRoleClaims.Any(userRoleClaim =>
               userRoleClaim == "Admin" || request.Roles.Contains(userRoleClaim));

        if (isMatchedAUserRoleClaimWithRequestRoles)
        {
            throw new AuthorizationException("You are not authorized.");
        }

        TResponse response = await next();

        return response;
    }
}
