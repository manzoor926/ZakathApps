using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ZakathApp.API.Data;
using ZakathApp.API.Services;
using ZakathApp.API.Helpers;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. DATABASE CONFIGURATION
// ========================================
builder.Services.AddDbContext<ZakathDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ZakathConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

// ========================================
// 2. JWT AUTHENTICATION
// ========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// ========================================
// 3. CORS CONFIGURATION
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// ========================================
// 4. DEPENDENCY INJECTION - SERVICES
// ========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IZakathCalculationService, ZakathCalculationService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IMadhabService, MadhabService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHijriDateService, HijriDateService>();

// ========================================
// 5. HELPERS
// ========================================
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<HijriDateHelper>();

// ========================================
// 6. CONTROLLERS & SWAGGER
// ========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Zakath API",
        Version = "v1",
        Description = "Complete Islamic Zakath Calculation API - Multi-Language, Multi-Currency, Multi-Madhab",
        Contact = new OpenApiContact
        {
            Name = "Zakath App Support",
            Email = "support@zakathapp.com"
        }
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// 7. LOGGING
// ========================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ========================================
// BUILD APP
// ========================================
var app = builder.Build();

// ========================================
// 8. MIDDLEWARE PIPELINE
// ========================================

// Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zakath API V1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

// HTTPS Redirection (Commented out for HTTP testing)
// app.UseHttpsRedirection();

// CORS - Must be before Authentication
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// ========================================
// 9. GLOBAL EXCEPTION HANDLER
// ========================================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = new
        {
            success = false,
            message = "An internal server error occurred.",
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(error);
    });
});

// ========================================
// 10. DATABASE MIGRATION (Optional)
// ========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ZakathDbContext>();
        // Uncomment to auto-migrate on startup
        // context.Database.Migrate();

        // Check if database is accessible
        if (context.Database.CanConnect())
        {
            Console.WriteLine("✅ Database connection successful!");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ An error occurred while migrating the database.");
    }
}

// ========================================
// 11. RUN APPLICATION
// ========================================
Console.WriteLine("🕌 Zakath API is starting...");
Console.WriteLine($"📅 Current Date: {DateTime.Now}");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");

app.Run();

Console.WriteLine("🕌 Zakath API stopped.");