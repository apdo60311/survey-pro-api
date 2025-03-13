using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using survey_pro.Interfaces;
using survey_pro.Models;
using survey_pro.Settings;

namespace survey_pro.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly JwtSettings _jwtSettings;

        private readonly IRoleService? _roleService;

        public UserService(
            IOptions<MongoDbSettings> mongoSettings,
            IOptions<JwtSettings> jwtSettings, IRoleService? roleService = null)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _users = database.GetCollection<User>(mongoSettings.Value.UsersCollection);
            _jwtSettings = jwtSettings.Value;
            _roleService = roleService;
        }

        public async Task<AuthResponse> Authenticate(AuthRequest model)
        {
            var user = await _users.Find(user => user.Email == model.Email).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles,
                Token = token
            };
        }

        public async Task<AuthResponse> Register(RegisterRequest model)
        {
            if (await _users.Find(user => user.Email == model.Email).AnyAsync())
                throw new ApplicationException("Email is already registered");


            var roles = model.Roles ?? new List<string> { "User" };
            if (!roles.Any())
                roles.Add("User");

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Roles = roles
            };


            await _users.InsertOneAsync(user);

            if (_roleService != null)
            {
                foreach (var role in roles)
                {
                    await _roleService.CreateRoleAsync(role);
                }
            }


            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles,
                Token = token
            };
        }

        public async Task<User> GetById(string id)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        private string GenerateJwtToken(User user)
        {
            if (string.IsNullOrEmpty(_jwtSettings.Secret))
            {
                throw new ApplicationException("JWT secret is not configured");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            if (key.Length < 32)
            {
                throw new ApplicationException("JWT secret key is too short. It should be at least 32 bytes for HMAC-SHA256");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<User> UpdateProfile(string userId, UpdateProfileRequest model)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                if (await _users.Find(u => u.Email == model.Email && u.Id != userId).AnyAsync())
                    throw new ApplicationException("Email is already registered to another account");

                user.Email = model.Email;
            }

            if (!string.IsNullOrEmpty(model.Username))
            {
                user.Username = model.Username;
            }

            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                    throw new ApplicationException("Current password is incorrect");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await _users.ReplaceOneAsync(u => u.Id == userId, user);

            return user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _users.Find(_ => true).ToListAsync();
        }


    }
}