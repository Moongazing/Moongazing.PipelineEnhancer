using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Moongazing.PipelineEnhancer.Extensions;

public static class ClaimExtensions
{

    public static void AddRoles(this ICollection<Claim> claims, string[] roles) =>
        roles.ToList().ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));
}
