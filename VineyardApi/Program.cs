using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Npgsql;
using VineyardApi.Data;
using VineyardApi.Middleware;
using VineyardApi.Repositories;
using VineyardApi.Services;
using VineyardApi.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
});
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Warning);

// Allow overriding connection string and JWT key via environment variables
var defaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? string.Empty;
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key")
    ?? builder.Configuration["Jwt:Key"]
    ?? string.Empty;

builder.Configuration["ConnectionStrings:DefaultConnection"] = defaultConnection;
builder.Configuration["Jwt:Key"] = jwtKey;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ContentOverrideValidator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Development CORS policy
builder.Services.AddCors(options =>
    options.AddPolicy("Dev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()));

var dataSourceBuilder = new NpgsqlDataSourceBuilder(defaultConnection);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<VineyardDbContext>(options =>
    options.UseNpgsql(dataSource));

builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IThemeRepository, ThemeRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IImageUsageRepository, ImageUsageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContentOverrideRepository, ContentOverrideRepository>();
builder.Services.AddScoped<IContentOverrideService, ContentOverrideService>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
try
{
    var dbInfo = new NpgsqlConnectionStringBuilder(defaultConnection);
    startupLogger.LogInformation("Starting {Environment} with DB host {Host} and database {Database} (User: {UserName})",
        app.Environment.EnvironmentName,
        dbInfo.Host,
        dbInfo.Database,
        dbInfo.Username);
}
catch (Exception ex)
{
    startupLogger.LogWarning(ex, "Unable to parse database connection string for startup log");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Dev");
}

app.UseExceptionHandler("/error");
app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply pending EF Core migrations and seed data at startup
app.MigrateDatabase();
await DbInitializer.SeedAsync(app.Services, app.Lifetime.ApplicationStopping);

app.Run();
