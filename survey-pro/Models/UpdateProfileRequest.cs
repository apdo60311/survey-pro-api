namespace survey_pro.Models
{
    public class UpdateProfileRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
