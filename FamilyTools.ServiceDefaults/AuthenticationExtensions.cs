using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Authentification/autorisation transversale FamilyTools, adossee a Keycloak (SSO).
/// Pensee pour etre reutilisee a l'identique par toutes les futures applications du suite :
/// il suffit d'appeler <see cref="AddFamilyToolsAuthentication"/> puis, cote pipeline,
/// <c>UseAuthentication()</c> / <c>UseAuthorization()</c>.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>Nom de la policy exigeant le role realm <c>admin</c>.</summary>
    public const string AdminPolicy = "admin";

    /// <summary>
    /// Configure la validation des jetons JWT emis par Keycloak.
    /// </summary>
    /// <param name="keycloakServiceName">Nom de la ressource Keycloak (resolue via service discovery).</param>
    /// <param name="realm">Realm Keycloak.</param>
    /// <param name="audience">Audience attendue dans le jeton (clientId du resource server).</param>
    public static TBuilder AddFamilyToolsAuthentication<TBuilder>(
        this TBuilder builder,
        string keycloakServiceName = "keycloak",
        string realm = "familytools",
        string audience = "easycompta-api")
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services
            .AddAuthentication()
            .AddKeycloakJwtBearer(keycloakServiceName, realm, options =>
            {
                options.Audience = audience;
                // On conserve les noms de claims d'origine (pas de remap automatique vers les URIs SOAP).
                options.MapInboundClaims = false;
                // Keycloak est servi en HTTP en local/dev. A repasser a true derriere HTTPS en prod.
                options.RequireHttpsMetadata = builder.Environment.IsDevelopment() == false
                    && builder.Configuration.GetValue("Keycloak:RequireHttpsMetadata", true);
                options.TokenValidationParameters.NameClaimType = "preferred_username";
                options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

                // Split-horizon Keycloak : le navigateur obtient son jeton via l'URL PUBLIQUE
                // (ex. http://192.168.8.210:8080) alors que l'API joint Keycloak en INTERNE
                // (http://keycloak:8080) pour recuperer les cles. L'`iss` du jeton vaut donc
                // l'URL publique. On l'accepte explicitement quand `Keycloak:ValidIssuers` est
                // fourni (liste separee par des virgules) ; les cles restent lues en interne
                // via l'Authority. Sans cette variable (dev/Aspire), le comportement par defaut
                // (issuer = metadata de l'Authority) s'applique tel quel.
                var validIssuers = builder.Configuration.GetValue<string>("Keycloak:ValidIssuers");
                if (!string.IsNullOrWhiteSpace(validIssuers))
                {
                    options.TokenValidationParameters.ValidIssuers = validIssuers
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }

                options.Events = new JwtBearerEvents
                {
                    // Keycloak place les roles realm dans le claim JSON "realm_access".
                    // On les projette en claims de role standards pour [Authorize(Roles=...)] / policies.
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            MapRealmRoles(context.Principal, identity);
                        }

                        return Task.CompletedTask;
                    },

                    // SignalR (WebSocket) ne peut pas envoyer d'en-tete Authorization :
                    // le jeton arrive en query string. On le recupere pour les routes /hubs.
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken)
                            && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services
            .AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, policy => policy.RequireRole("admin"));

        return builder;
    }

    private static void MapRealmRoles(ClaimsPrincipal principal, ClaimsIdentity identity)
    {
        var realmAccess = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrEmpty(realmAccess))
        {
            return;
        }

        using var document = JsonDocument.Parse(realmAccess);
        if (!document.RootElement.TryGetProperty("roles", out var roles)
            || roles.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var role in roles.EnumerateArray())
        {
            var value = role.GetString();
            if (!string.IsNullOrEmpty(value) && !identity.HasClaim(ClaimTypes.Role, value))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, value));
            }
        }
    }
}
