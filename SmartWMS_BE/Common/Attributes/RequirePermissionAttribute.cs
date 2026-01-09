using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Common.Attributes;

/// <summary>
/// Marks an endpoint as requiring specific permission(s).
/// Checks user's permission claims against required permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly Permission[] _permissions;
    private readonly bool _requireAll;

    /// <summary>
    /// Requires specific permission(s) to access the endpoint.
    /// </summary>
    /// <param name="permissions">Required permission(s)</param>
    public RequirePermissionAttribute(params Permission[] permissions)
    {
        _permissions = permissions;
        _requireAll = false;
    }

    /// <summary>
    /// Requires permission(s) with option to require all or any.
    /// </summary>
    /// <param name="requireAll">If true, all permissions are required. If false, any one is sufficient.</param>
    /// <param name="permissions">Required permission(s)</param>
    public RequirePermissionAttribute(bool requireAll, params Permission[] permissions)
    {
        _permissions = permissions;
        _requireAll = requireAll;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get user's permissions from claims
        var userPermissions = user.FindAll("permission")
            .Select(c => c.Value)
            .ToHashSet();

        // Check if user has required permissions
        var hasPermission = _requireAll
            ? _permissions.All(p => userPermissions.Contains(((int)p).ToString()))
            : _permissions.Any(p => userPermissions.Contains(((int)p).ToString()));

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
