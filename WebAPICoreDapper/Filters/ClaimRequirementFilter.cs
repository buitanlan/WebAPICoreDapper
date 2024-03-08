using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text.Json;
using WebAPICoreDapper.Utilities.Constants;

namespace WebAPICoreDapper.Filters;

public class ClaimRequirementFilter(FunctionCode function, ActionCode action) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var claimsIdentity = context.HttpContext.User.Identity as ClaimsIdentity;
        var permissionsClaim = context.HttpContext.User.Claims.SingleOrDefault(c => c.Type == SystemConstants.UserClaim.Permissions);
        if (permissionsClaim != null)
        {
            var permissions = JsonSerializer.Deserialize<List<string>>(permissionsClaim.Value);
            var functionArr = function.ToString().Split("_");
            var functionId = string.Join(".", functionArr);
            if (!permissions.Contains(functionId + "_" + action))
            {
                context.Result = new ForbidResult();
            }
        }
        else
        {
            context.Result = new ForbidResult();
        }
    }

}
