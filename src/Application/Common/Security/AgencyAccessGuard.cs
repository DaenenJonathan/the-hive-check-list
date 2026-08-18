using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;

namespace TheHive.Application.Common.Security;

public static class AgencyAccessGuard
{
    public static void EnsureCanAccessAgency(ICurrentUserService currentUser, Guid? resourceAgencyId)
    {
        if (currentUser.Role != "AgencyManager")
            return;

        if (resourceAgencyId is null
            || !Guid.TryParse(currentUser.AgencyId, out var userAgencyId)
            || resourceAgencyId != userAgencyId)
            throw new ForbiddenAccessException();
    }
}
