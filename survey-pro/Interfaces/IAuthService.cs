using survey_pro.Interfaces;
using survey_pro.Models;

namespace survey_pro.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponse> Authenticate(AuthRequest model);
        Task<AuthResponse> Register(RegisterRequest model);
        Task<User> GetById(string id);
        Task<User> UpdateProfile(string userId, UpdateProfileRequest model);
        Task<List<User>> GetAllUsers();

    }
}