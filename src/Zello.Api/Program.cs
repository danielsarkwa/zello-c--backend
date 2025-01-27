using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Npgsql;
using Zello.Application.ServiceInterfaces;
using Zello.Application.ServiceImplementations;
using Zello.Application.ServiceInterfaces.ExceptionInterfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Repositories;
using Zello.Infrastructure.Services;
using Zello.Infrastructure.TestingDataStorage;
using Zello.Domain.RepositoryInterfaces;
using Zello.Domain.Abstractions;

var builder = WebApplication.CreateBuilder(args);


#region Service Configuration

builder.Configuration.AddUserSecrets<Program>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseNpgsql(connectionString,
        npgsqlOptions => {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });
});

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("https://zello-frontend.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Register DbContext as DbContext for generic access
builder.Services.AddScoped<DbContext>(provider => provider.GetService<ApplicationDbContext>()!);

// Authentication related services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserClaimsService, UserClaimsService>();
builder.Services.AddScoped<IUserIdentityService, UserIdentityService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Entity related services
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskListService, TaskListService>();
builder.Services.AddScoped<IWorkTaskService, WorkTaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
builder.Services.AddScoped<ITaskListRepository, TaskListRepository>();
builder.Services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();
builder.Services.AddScoped<ITaskAssigneeRepository, TaskAssigneeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IWorkspaceValidationService, WorkspaceValidationService>();

// Swagger Service
builder.Services.AddScoped<ISwaggerService, SwaggerService>();

// Basic Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure JSON Handling
builder.Services.AddControllers()
    .AddNewtonsoftJson(options => {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver =
            new Newtonsoft.Json.Serialization.DefaultContractResolver {
                NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
            };
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
                                       throw new InvalidOperationException(
                                           "JWT Key not found in configuration")))
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

        // Add individual policy for the current access level
        options.AddPolicy(level, policy =>
            policy.RequireClaim("AccessLevel", level));
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
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Zello API",
        Version = "v1"
    });

    // Add XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);


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

#region Database Initialization

try {
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    bool dbExists = await context.Database.CanConnectAsync();
    bool isNewDatabase = !dbExists;

    // Create/ensure database exists
    await context.Database.EnsureCreatedAsync();

    // Only seed if this was a new database
    if (isNewDatabase) {
        Console.WriteLine("New database detected - seeding initial data...");
        await DatabaseSeeder.SeedDatabase(context);
        Console.WriteLine("Database seed completed successfully.");
    } else {
        Console.WriteLine("Existing database detected - skipping seed.");
    }

    // Additional database verification
    var databaseName = context.Database.GetDbConnection().Database;
    Console.WriteLine($"Successfully connected to database: {databaseName}");
    Console.WriteLine("Database schema created/verified successfully.");
} catch (Exception ex) {
    Console.WriteLine($"An error occurred while initializing the database:");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    throw;
}

#endregion

// Development-specific middleware
if (app.Environment.IsDevelopment()) {
    using (var scope = app.Services.CreateScope()) {
        var swaggerService = scope.ServiceProvider.GetRequiredService<ISwaggerService>();

        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var apiDirectory = Directory.GetParent(currentDirectory)?.Parent?.Parent?.Parent;
        var srcDirectory = apiDirectory?.Parent;
        var solutionDirectory = srcDirectory?.Parent;

        var yamlPath = Path.Combine(
            solutionDirectory?.FullName ??
            throw new DirectoryNotFoundException("Could not determine solution directory"),
            "Documentation",
            "src",
            "content",
            "schemas",
            "Zello.yaml"
        );


        swaggerService.SaveSwaggerYaml(app, yamlPath);
    }
}

app.UseCors();
// Configure the HTTP request pipeline
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

// Map controller endpoints
app.MapControllers();

// Start the application
app.Run();

#endregion
