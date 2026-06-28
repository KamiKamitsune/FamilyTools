using Microsoft.AspNetCore.Http;

namespace FamilyTools.EasyCompta;

/// <summary>
/// Authentification par clé d'API via le header <c>X-Api-Key</c>.
/// Désactivée tant que <c>Security:ApiKey</c> n'est pas renseigné en configuration
/// (comportement par défaut : aucune contrainte).
/// </summary>
internal sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string HeaderName = "X-Api-Key";
    private static readonly string[] OpenPaths = ["/health", "/alive", "/openapi"];

    private readonly string? _apiKey = configuration["Security:ApiKey"];

    public async Task InvokeAsync(HttpContext context)
    {
        // Pas de clé configurée -> middleware inactif. Endpoints d'infra toujours ouverts.
        if (string.IsNullOrWhiteSpace(this._apiKey) || this.IsOpenPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided)
            || !string.Equals(provided.ToString(), this._apiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Clé d'API manquante ou invalide.");
            return;
        }

        await next(context);
    }

    private bool IsOpenPath(PathString path)
        => OpenPaths.Any(p => path.StartsWithSegments(p));
}
