using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using survey_pro.Interfaces;
using survey_pro.Models;
using survey_pro.Settings;

namespace survey_pro.Services;

public class RoleService : IRoleService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Role> _roles;


    public RoleService(IOptions<MongoDbSettings> mongoSettings)
    {
        var client = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
        _users = database.GetCollection<User>(mongoSettings.Value.UsersCollection);
        _roles = database.GetCollection<Role>(mongoSettings.Value.RolesCollection);
    }

    public async Task<bool> AddUserToRoleAsync(string userId, string role)
    {
        // Check if role exists
        if (!await RoleExistsAsync(role))
            await CreateRoleAsync(role);

        // Add the role to user if they don't already have it
        var update = Builders<User>.Update.AddToSet(u => u.Roles, role);
        var result = await _users.UpdateOneAsync(u => u.Id == userId && !u.Roles.Contains(role), update);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveUserFromRoleAsync(string userId, string role)
    {
        var update = Builders<User>.Update.Pull(u => u.Roles, role);
        var result = await _users.UpdateOneAsync(u => u.Id == userId && u.Roles.Contains(role), update);

        return result.ModifiedCount > 0;
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user?.Roles ?? new List<string>();
    }

    public async Task<List<string>> GetAllRolesAsync()
    {
        var roles = await _roles.Find(_ => true).ToListAsync();
        return roles.Select(r => r.Name).ToList();
    }

    public async Task<bool> RoleExistsAsync(string role)
    {
        var count = await _roles.CountDocumentsAsync(r => r.Name == role);
        return count > 0;
    }

    public async Task<bool> CreateRoleAsync(string role)
    {
        if (await RoleExistsAsync(role))
            return false;

        await _roles.InsertOneAsync(new Role { Name = role });
        return true;
    }
}
