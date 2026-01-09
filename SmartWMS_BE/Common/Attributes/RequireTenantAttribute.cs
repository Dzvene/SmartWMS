using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartWMS.API.Common.Attributes;

/// <summary>
/// Marks an endpoint as requiring tenant context.
/// The tenant ID must be provided in the route and validated against user's claims.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireTenantAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get tenant ID from route
        if (!context.RouteData.Values.TryGetValue("tenantId", out var routeTenantId))
        {
            context.Result = new BadRequestObjectResult("Tenant ID is required");
            return;
        }

        // Get user's tenant claim
        var userTenantClaim = user.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(userTenantClaim))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Validate tenant access
        if (routeTenantId?.ToString() != userTenantClaim)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
