using Microsoft.AspNetCore.Identity;

namespace vpp_server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
