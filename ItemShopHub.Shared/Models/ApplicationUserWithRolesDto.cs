using System.ComponentModel.DataAnnotations;

namespace ItemShopHub.Shared.Models;

public class ApplicationUserWithRolesDto : ApplicationUserDto
{
    public List<string>? Roles { get; set; }
}
