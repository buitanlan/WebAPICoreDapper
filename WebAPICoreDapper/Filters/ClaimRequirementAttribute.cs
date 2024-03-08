using Microsoft.AspNetCore.Mvc;

namespace WebAPICoreDapper.Filters;

public class ClaimRequirementAttribute : TypeFilterAttribute
{
    public ClaimRequirementAttribute(FunctionCode function, ActionCode action) : base(typeof(ClaimRequirementFilter))
    {
        Arguments = [function, action];
    }
}
