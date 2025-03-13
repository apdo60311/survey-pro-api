using System.ComponentModel.DataAnnotations;

namespace survey_pro.Dtos
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        public required string Password { get; set; }
    }
}
