using System.ComponentModel.DataAnnotations;

namespace survey_pro.Dtos
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
