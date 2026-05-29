using Microsoft.AspNetCore.Identity;

namespace lapshop.Bl
{
    public class ApplicationRole : IdentityRole
    {
        public string RolePermisstions { get; set; }
    }
}
