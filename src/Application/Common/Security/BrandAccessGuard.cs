using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;

namespace TheHive.Application.Common.Security;

public static class BrandAccessGuard
{
    public static void EnsureCanAccessBrand(ICurrentUserService currentUser, Guid? resourceBrandId)
    {
        if (currentUser.Role != "Manager")
            return;

        if (resourceBrandId is null || !currentUser.BrandIds.Contains(resourceBrandId.Value))
            throw new ForbiddenAccessException();
    }
}
