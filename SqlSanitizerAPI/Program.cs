using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SqlSanitizerAPI.Configuration;
using SqlSanitizerAPI.Controllers;
using SqlSanitizerAPI.Middleware;
using SqlSanitizerAPI.Repositories;
using SqlSanitizerAPI.Services.SanitizationService;
using SqlSanitizerAPI.Services.TokenSevice;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add User Secrets configuration
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

#region Configuration with Validation

// Configure and validate JwtSettings
builder.Services.Configure<JwtSettingOptions>(builder.Configuration.GetSection(JwtSettingOptions.SectionName));
builder.Services.AddSingleton<IValidateOptions<JwtSettingOptions>, JwtSettingsValidator>();

// Configure and validate RepositoryOptions
builder.Services.Configure<RepositoryOptions>(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString(ConfigurationSections.DefaultConnection) ?? string.Empty;
    options.DbSchema = builder.Configuration.GetValue<string>(DatabaseConfigKeys.DbSchema) ?? string.Empty;
    options.SqlCommandDefaultTimeout = builder.Configuration.GetValue<int>(DatabaseConfigKeys.SqlCommandDefaultTimeout);
    options.LogConnectionMessages = builder.Configuration.GetValue<bool>(DatabaseConfigKeys.LogConnectionMessages);
});
builder.Services.AddSingleton<IValidateOptions<RepositoryOptions>, RepositoryOptionsValidator>();

// Configure and validate SanitizationServiceOptions
builder.Services.Configure<SanitizationServiceOptions>(builder.Configuration.GetSection(ConfigurationSections.SanitizationService));
builder.Services.AddSingleton<IValidateOptions<SanitizationServiceOptions>, SanitizationServiceOptionsValidator>();

// Configure AuthControllerOptions
builder.Services.Configure<AuthControllerOptions>(builder.Configuration.GetSection(ConfigurationSections.AuthController));

#endregion Configuration with Validation

#region Service Injection

builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<ISanitizationService, SanitizationService>();
builder.Services.AddScoped<ITokenService, TokenService>();

#endregion Service Injection

#region JWT Authentication

var jwtSettings = builder.Configuration.GetSection(JwtSettingOptions.SectionName);
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

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
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

#endregion JWT Authentication

#region Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

#endregion Swagger

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();