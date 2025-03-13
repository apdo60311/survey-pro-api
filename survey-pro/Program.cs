
using survey_pro.Models;
using survey_pro.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using survey_pro.Interfaces;
using survey_pro.Services;
using survey_pro.Middlewares;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Configure mongoDB 
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));


// Configure authentication options
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.Secret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for: {context.Principal!.Identity!.Name}");
            return Task.CompletedTask;
        }
    };
});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();


// Add Authorization Policies
builder.Services.AddAuthorizationBuilder()
                                 .AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"))
                                 .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"))
                                 .AddPolicy("SuperAdminOrAdmin", policy => policy.RequireRole("SuperAdmin", "Admin"))
                                 .AddPolicy("UserOnly", policy => policy.RequireRole("User"));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});



builder.Services.AddHttpsRedirection(
    options =>
{
    options.HttpsPort = builder.Environment.IsDevelopment() ? 5001 : 443;
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
}
);


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
app.UseCors("AllowAll");


app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token);
        var tokenS = jsonToken as JwtSecurityToken;
        // Log or inspect the claims
        var claims = tokenS?.Claims;
    }
    await next();
});


app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    Console.WriteLine($"Incoming Authorization Header: {authHeader}");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<MongoDBExceptionMiddleware>();
app.UseRoleAuthorizationLoggingMiddleware();
app.UseErrorHandlingMiddleware();

app.Run();
