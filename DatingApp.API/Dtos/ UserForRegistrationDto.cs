using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegistrationDto
    {
        [Required]
        public string Username { get; set; }

        [Required] [StringLength(8, MinimumLength=4, ErrorMessage="Password is required.  Must be between 4 and 8 characters")]
        public string Password { get; set; }
    }
}