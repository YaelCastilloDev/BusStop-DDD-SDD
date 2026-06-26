using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace BusStop.Web.Configurations;

public sealed class KeycloakRoleTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        var realmAccess = identity?.FindFirst("realm_access")?.Value;

        if (realmAccess is not null)
        {
            using var doc = JsonDocument.Parse(realmAccess);
            if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
            {
                foreach (var role in rolesElement.EnumerateArray())
                {
                    identity!.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
