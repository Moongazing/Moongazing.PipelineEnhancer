using System.Security.Claims;

namespace Moongazing.PipelineEnhancer.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static List<string>? Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        var result = claimsPrincipal?.FindAll(claimType)?.Select(x => x.Value).ToList();
        return result;
    }
    public static List<string>? ClaimRoles(this ClaimsPrincipal claimsPrincipal)
    {
        const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        var roleClaims = claimsPrincipal?.Claims.Where(x => x.Type == RoleClaimType);
        return roleClaims?.Select(x => x.Value).ToList();
    }



}