using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventosVivos.Api.Auth;
using EventosVivos.Api.Middleware;
using EventosVivos.Application;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Infrastructure;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Hosts like Render/Railway inject the port via the PORT env var.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

const string CorsPolicy = "frontend";

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize/parse enums as camelCase strings ("conferencia", "activo", ...).
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega el token JWT obtenido en POST /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Auth adapters (the API is the composition root).
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IAdminAuthenticator, AdminAuthenticator>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "eventosvivos-dev-signing-key-change-in-production-please-32chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EventosVivos";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EventosVivos";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Create the database/schema and seed reference data on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();

app.Run();

// Exposed so the integration test project can spin up the API with WebApplicationFactory.
public partial class Program { }
