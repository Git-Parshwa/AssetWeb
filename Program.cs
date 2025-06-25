using System.Security.Claims;
using System.Text;
using AssetWeb.Data;
using AssetWeb.Mapping;
using AssetWeb.Models.Domain;
using AssetWeb.Repositories;
using AssetWeb.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

if (Environment.GetEnvironmentVariable("RENDER") == null)
{
    // We're running locally → load from .env
    Env.Load();
}

var connectionString = Environment.GetEnvironmentVariable("DB_CONN");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var emailUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
var enableSwagger = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");
Console.WriteLine("JWT_KEY length: " + (jwtKey?.Length ?? 0));


if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("JWT settings are missing from environment variables.");

if (string.IsNullOrWhiteSpace(emailUsername) || string.IsNullOrWhiteSpace(emailPassword))
    throw new InvalidOperationException("Email credentials are missing from environment variables.");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection String is missing from environment variables.");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AssetWeb API", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "Oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.PostConfigure<EmailSettings>(email =>
{
    email.UserName = emailUsername!;
    email.Password = emailPassword!;
});

builder.Services.AddScoped<EmailService>();
builder.Services.AddTransient<EmailService>();

// Add AssetWebDbContext registration
// builder.Services.AddDbContext<AssetWebDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("AssetWebConnectionString")));
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Database connection string not found.");

builder.Services.AddDbContext<AssetWebAuthDbContext>(options =>
    options.UseSqlServer(connectionString));    

builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IProfileRepository, SqlProfileRepository>();
builder.Services.AddScoped<IImageRepository, SqlImageRepository>();
builder.Services.AddScoped<ISiteRepository, SqlSiteRepository>();
builder.Services.AddScoped<IFileRepository, SqlFileRepository>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AssetWebAuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment() || enableSwagger?.ToLower() == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images")),
    RequestPath = "/wwwroot/Images"
});

// Important: UseAuthentication must come before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/test-anon", [AllowAnonymous] () => Results.Ok("Hello anonymous"));

app.MapGet("/test-db", async (AssetWebAuthDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok("DB Connected") : Results.Problem("DB unreachable");
    }
    catch (Exception ex)
    {
        return Results.Problem("DB error: " + ex.Message);
    }
});


app.MapControllers();

app.Run();

