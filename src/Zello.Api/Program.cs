using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFastEndpoints()
    .SwaggerDocument(o => {
        o.DocumentSettings = s => {
            s.Title = "Zello API";
            s.Version = "v1";
        };
    });

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseFastEndpoints(c => { c.Endpoints.RoutePrefix = "api"; });

    // Use FastEndpoints swagger middleware
    app.UseOpenApi(); // Instead of UseSwagger()
    app.UseSwaggerUi(); // Instead of UseSwaggerUI()
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();