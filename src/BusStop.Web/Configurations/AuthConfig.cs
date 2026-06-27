using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BusStop.Web.Configurations;

public static class AuthConfig
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var authSection = configuration.GetSection("Authentication");
        var metadataAddress = authSection["MetadataAddress"] ?? throw new InvalidOperationException("Authentication:MetadataAddress is required.");
        var validIssuer = authSection["ValidIssuer"] ?? throw new InvalidOperationException("Authentication:ValidIssuer is required.");
        var audience = authSection["Audience"] ?? throw new InvalidOperationException("Authentication:Audience is required.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MetadataAddress = metadataAddress;
            options.RequireHttpsMetadata = !environment.IsDevelopment();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = validIssuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = "sub"
            };
        });

        services.AddTransient<Microsoft.AspNetCore.Authentication.IClaimsTransformation, KeycloakRoleTransformation>();

        return services;
    }
}
