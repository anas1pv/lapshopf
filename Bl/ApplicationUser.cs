using Microsoft.AspNetCore.Identity;

namespace lapshop.Bl
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
