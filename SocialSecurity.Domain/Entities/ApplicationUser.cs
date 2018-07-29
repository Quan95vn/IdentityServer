using Microsoft.AspNetCore.Identity;

namespace SocialSecurity.Domain.Entities
{
    /// <summary>
    /// ApplicationUser
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ApiKey { get; set; }
    }
}