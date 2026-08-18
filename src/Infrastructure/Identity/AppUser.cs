using Microsoft.AspNetCore.Identity;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer";
    public Guid? AgencyId { get; set; }
    public ICollection<Brand> ManagedBrands { get; set; } = new List<Brand>();
}
