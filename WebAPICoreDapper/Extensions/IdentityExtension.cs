using System;
using System.Linq;
using System.Security.Claims;

namespace WebAPICoreDapper.Extensions;

public static class IdentityExtension
{
    public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
    {
        var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

        return claim != null ? claim.Value : string.Empty;
    }

    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = ((ClaimsIdentity)claimsPrincipal.Identity)!.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier);
        return Guid.Parse(claim.Value);
    }
}