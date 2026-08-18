using TheHive.Domain.Common;

namespace TheHive.Domain.Entities;

public class Agency : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;

    private readonly List<Brand> _brands = [];
    public IReadOnlyCollection<Brand> Brands => _brands.AsReadOnly();

    private Agency() { }

    public static Agency Create(string name, string color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(color);
        return new Agency { Name = name, Color = color };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public void SetColor(string color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(color);
        Color = color;
    }
}
