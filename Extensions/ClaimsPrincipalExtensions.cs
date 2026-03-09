using System.Security.Claims;

namespace NGMHS.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name) ?? "Unknown User";
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Admin");
    }
}
