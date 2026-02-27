using MeetSpace.API.Authorization;
using FluentValidation;
using MeetSpace.Application;
using MeetSpace.Domain.Entities;
using MeetSpace.Infrastructure;
using MeetSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

// Add services to the container
builder.Services.AddControllers();

// Add Swagger/OpenAPI with OAuth2 password flow
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "MeetSpace API", Version = "v1" });

    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
        Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
        {
            Password = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "Profile" },
                    { "roles", "Roles" },
                    { "api", "API access" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile", "roles", "api" }
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add JWT Authentication (FR-2) — OpenIddict handles both token issuance and validation
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<AppDbContext>();
    })
    .AddServer(options =>
    {
        // Token endpoint for Resource Owner Password Credentials (ROPC) grant
        options.SetTokenEndpointUris("/connect/token");
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();

        // Register custom scope for the API
        options.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles,
            "api");

        // In development, use auto-generated certificates.
        // In production, replace with AddEncryptionCertificate / AddSigningCertificate.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Disable encryption so Angular can decode the access token as a plain JWT.
        // For SPAs the bearer token is already protected in transit via HTTPS.
        options.DisableAccessTokenEncryption();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        // Validate tokens issued by the local OpenIddict server
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Configure authentication to use OpenIddict validation as the default scheme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

// RBAC authorization policies (FR-2)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.EmployeeOrAbove, policy =>
        policy.RequireRole("Employee", "OfficeManager", "Admin"));

    options.AddPolicy(Policies.ManagerOrAbove, policy =>
        policy.RequireRole("OfficeManager", "Admin"));

    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireRole("Admin"));
});

// Register Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Run migrations and seed roles + default admin user + OpenIddict client
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var adminPassword = builder.Configuration["AdminSeed:Password"];
    await DbSeeder.SeedAsync(userManager, roleManager, applicationManager, logger, adminPassword);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MeetSpace API v1");
        options.OAuthClientId("MeetSpace-angular");
        options.OAuthScopes("openid", "profile", "roles", "api");
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

// Global exception handler — converts domain/validation exceptions to RFC 7807 problem details
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    var ex = feature?.Error;

    (int status, string title, object? errors) = ex switch
    {
        ValidationException ve => (400, "Validation failed",
            ve.Errors.GroupBy(e => e.PropertyName)
                     .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
        KeyNotFoundException knfe => (404, knfe.Message, null),
        InvalidOperationException ioe when ioe.Message.Contains("already exists") ||
                                           ioe.Message.Contains("already in use") =>
            (409, ioe.Message, null),
        InvalidOperationException ioe => (400, ioe.Message, null),
        _ => (500, "An unexpected error occurred.", null)
    };

    ctx.Response.StatusCode = status;
    ctx.Response.ContentType = "application/problem+json";

    var problem = new { title, status, errors };
    await ctx.Response.WriteAsJsonAsync(problem);
}));

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
