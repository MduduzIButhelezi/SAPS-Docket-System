using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONTROLLERS
// ============================================================
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;

        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// ============================================================
// DATABASE
// ============================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// HTTP CLIENT
// Used by TranslationService
// ============================================================
builder.Services.AddHttpClient<ITranslationService, TranslationService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            UseProxy = true,
            Proxy = new System.Net.WebProxy(
                "http://proxy.sasa.net:8080")
            {
                UseDefaultCredentials = true
            }
        });

builder.Services.AddHttpContextAccessor();

// ============================================================
// TRANSLATION
// ============================================================
builder.Services.Configure<TranslationOptions>(
    builder.Configuration.GetSection("Translation"));

// ============================================================
// SERVICES
// ============================================================
builder.Services.AddScoped<IAuditService, AuditService>();

// Existing SMS service
builder.Services.Configure<TwilioOptions>(
    builder.Configuration.GetSection("Twilio"));

builder.Services.AddScoped<ISmsService, SmsService>();

// JWT service
builder.Services.AddScoped<IJwtService, JwtService>();

// ============================================================
// JWT AUTHENTICATION
// ============================================================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is missing from configuration.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

// ============================================================
// AUTHORIZATION
// ============================================================
builder.Services.AddAuthorization();

// ============================================================
// CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ============================================================
// SWAGGER
// Swashbuckle.AspNetCore 10.2.3
// ============================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // --------------------------------------------------------
    // JWT / Bearer authentication definition
    // --------------------------------------------------------
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter your JWT token. Example: Bearer eyJhbGciOi..."
        });

    // --------------------------------------------------------
    // Tell Swagger that endpoints require the Bearer token
    // --------------------------------------------------------
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});

// ============================================================
// BUILD APPLICATION
// ============================================================
var app = builder.Build();

// ============================================================
// SWAGGER
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "SAPS Docket System API v1");

        options.RoutePrefix = "swagger";
    });
}

// ============================================================
// MIDDLEWARE
// ============================================================
app.UseCors("Angular");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// ============================================================
// DATABASE SEEDING
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(db);
}

// ============================================================
// START APPLICATION
// ============================================================
app.Run();