using AuthGateway.BFF.Auth.Antiforgery;
using AuthGateway.BFF.Auth.DataProtection;
using AuthGateway.BFF.Auth.OIDC;
using AuthGateway.BFF.Auth.TokenManagement;

namespace AuthGateway.BFF.Auth;

internal static class AuthModule
{
    internal static IServiceCollection AddAuth(this WebApplicationBuilder builder)
    {
        builder.AddRedisDataProtection();
        builder.AddOidcAuthentication();
        builder.Services.AddAuthorizationPolicies();

        // Configure anti-forgery services
        var oidcOptions = builder.Configuration.GetSection(OpenIdConnectOptions.Key)
            .Get<OpenIdConnectOptions>();
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__CSRF";
            options.Cookie.HttpOnly = true;
            options.Cookie.MaxAge = TimeSpan.FromHours(oidcOptions?.CookieExpireTimeSpanHours ?? 8);
            options.Cookie.SecurePolicy = oidcOptions?.RequireSecureCookies ?? true
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = oidcOptions?.RequireSecureCookies ?? true
                ? SameSiteMode.Strict
                : SameSiteMode.Lax;
        });

        // Register token management service
        builder.Services.AddScoped<ITokenService, TokenService>();

        // Register HttpClientFactory for token refresh
        builder.Services.AddHttpClient();

        return builder.Services;
    }

    internal static IApplicationBuilder UseAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        // Add anti-forgery protection middleware (must be after UseAuthentication)
        app.UseAntiforgeryProtection();

        // Map BFF management endpoints
        app.MapBffEndpoints();

        return app;
    }
}