using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;
using Zello.Application.Interfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

#region Service Configuration

// Basic Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add claim services
builder.Services.AddScoped<IUserClaimsService, UserClaimsService>();
builder.Services.AddScoped<IAccessLevelService, AccessLevelService>();
builder.Services.AddScoped<IUserIdentityService, UserIdentityService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Configure JSON Handling
builder.Services.AddControllers()
    .AddNewtonsoftJson(options => {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

#region JWT Authentication Setup

// Verify JWT Key availability
var jwtKey = builder.Configuration["Jwt:Key"];
Console.WriteLine($"JWT Key found: {!string.IsNullOrEmpty(jwtKey)}");

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                                       "YourSuperSecretKeyHere"))
        };
    });

#endregion

#region Authorization Policies

builder.Services.AddAuthorization(options => {
    void AddAccessLevelPolicy(string level, AccessLevel minimumLevel) {
        options.AddPolicy($"MinimumAccessLevel_{level}", policy =>
            policy.RequireAssertion(context => {
                var accessLevelClaim = context.User.Claims
                    .FirstOrDefault(c => c.Type == "AccessLevel");

                return accessLevelClaim != null &&
                       Enum.TryParse<AccessLevel>(accessLevelClaim.Value, out var userLevel) &&
                       userLevel >= minimumLevel;
            }));
    }

    // Add policies for each access level
    AddAccessLevelPolicy("Guest", AccessLevel.Guest);
    AddAccessLevelPolicy("Member", AccessLevel.Member);
    AddAccessLevelPolicy("Owner", AccessLevel.Owner);
    AddAccessLevelPolicy("Admin", AccessLevel.Admin);
});

#endregion

#region Swagger Configuration

builder.Services.AddSwaggerGen(c => {
    // Basic Swagger document info
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Zello API",
        Version = "v1"
    });

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    // Add global security requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#endregion

#region Application Building and Pipeline Configuration

var app = builder.Build();

// Development-specific middleware
if (app.Environment.IsDevelopment()) {
    // Configure Swagger
    app.UseSwagger(c => {
        c.SerializeAsV2 = false;
        c.RouteTemplate = "swagger/{documentName}/swagger.yaml";
    });

    // Configure Swagger UI
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Zello API v1");
        c.ConfigObject.DefaultModelRendering =
            Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example;
        c.ConfigObject.DefaultModelExpandDepth = 3;
    });
}

// Configure the HTTP request pipeline
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

// Map controller endpoints
app.MapControllers();

// Start the application
app.Run();

#endregion
