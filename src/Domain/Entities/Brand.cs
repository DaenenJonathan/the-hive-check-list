using TheHive.Domain.Common;

namespace TheHive.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid AgencyId { get; private set; }
    public Agency? Agency { get; private set; }

    private Brand() { }

    public static Brand Create(string name, Guid agencyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Brand { Name = name, AgencyId = agencyId };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
