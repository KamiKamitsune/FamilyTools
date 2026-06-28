using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FamilyTools.Accounts;

/// <summary>
/// Endpoints self-service "Mon compte" du portail FamilyTools. Chaque appel agit uniquement
/// sur le compte de l'utilisateur authentifie (l'identifiant Keycloak vient du claim <c>sub</c>),
/// jamais sur un autre compte : la creation/gestion d'autrui reste reservee a l'administrateur.
/// </summary>
public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account").RequireAuthorization();

        group.MapGet("/profile", async (ClaimsPrincipal principal, KeycloakAdminClient keycloak, CancellationToken ct) =>
        {
            var userId = UserId(principal);
            var user = await keycloak.GetUserAsync(userId, ct);
            if (user is null)
            {
                return Results.NotFound();
            }

            var twoFactor = await keycloak.HasOtpAsync(userId, ct);
            return Results.Ok(new ProfileResponse(
                user.Username ?? string.Empty,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                twoFactor));
        });

        group.MapPut("/profile", async (UpdateProfileRequest body, ClaimsPrincipal principal, KeycloakAdminClient keycloak, CancellationToken ct) =>
        {
            if (!TryValidate(body, out var error))
            {
                return Results.BadRequest(new { message = error });
            }

            await keycloak.UpdateProfileAsync(UserId(principal), body.FirstName.Trim(), body.LastName.Trim(), body.Email.Trim(), ct);
            return Results.NoContent();
        });

        group.MapPut("/password", async (ChangePasswordRequest body, ClaimsPrincipal principal, KeycloakAdminClient keycloak, CancellationToken ct) =>
        {
            if (!TryValidate(body, out var error))
            {
                return Results.BadRequest(new { message = error });
            }

            var username = principal.FindFirstValue("preferred_username")
                ?? principal.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Results.BadRequest(new { message = "Utilisateur inconnu." });
            }

            var valid = await keycloak.VerifyCurrentPasswordAsync(username, body.CurrentPassword, ct);
            if (!valid)
            {
                return Results.BadRequest(new { message = "Le mot de passe actuel est incorrect." });
            }

            await keycloak.ResetPasswordAsync(UserId(principal), body.NewPassword, ct);
            return Results.NoContent();
        });

        group.MapPost("/2fa/enable", async (ClaimsPrincipal principal, KeycloakAdminClient keycloak, CancellationToken ct) =>
        {
            await keycloak.EnableOtpAsync(UserId(principal), ct);
            return Results.Ok(new { message = "La double authentification sera configuree a votre prochaine connexion." });
        });

        group.MapPost("/2fa/disable", async (ClaimsPrincipal principal, KeycloakAdminClient keycloak, CancellationToken ct) =>
        {
            await keycloak.DisableOtpAsync(UserId(principal), ct);
            return Results.Ok(new { message = "Double authentification desactivee." });
        });

        return app;
    }

    private static string UserId(ClaimsPrincipal principal) =>
        principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new KeycloakUserFacingException("Session invalide.");

    private static bool TryValidate(object model, out string error)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            error = string.Empty;
            return true;
        }

        error = results.FirstOrDefault()?.ErrorMessage ?? "Donnees invalides.";
        return false;
    }
}

public sealed record ProfileResponse(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    bool TwoFactorEnabled);

public sealed class UpdateProfileRequest
{
    [Required(ErrorMessage = "Le prenom est obligatoire.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "Le mot de passe actuel est obligatoire.")]
    public string CurrentPassword { get; set; } = string.Empty;

    // La politique fine (majuscule/minuscule/special) est appliquee par Keycloak ; on garde ici
    // le minimum structurel pour un retour immediat sans aller-retour serveur.
    [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;
}
