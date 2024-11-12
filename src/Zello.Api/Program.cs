using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options => {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });


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

builder.Services.AddAuthorization();

// Configure Swagger with JWT support and YAML
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zello API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger(c => {
        c.SerializeAsV2 = false;
        c.RouteTemplate = "swagger/{documentName}/swagger.yaml";
    });

    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Zello API v1");
        c.ConfigObject.DefaultModelRendering =
            Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example;
        c.ConfigObject.DefaultModelExpandDepth = 3;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
