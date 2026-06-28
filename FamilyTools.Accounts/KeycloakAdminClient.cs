using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace FamilyTools.Accounts;

/// <summary>Options de connexion a Keycloak (section "Keycloak" de la configuration).</summary>
public sealed class KeycloakAdminOptions
{
    /// <summary>Realm cible.</summary>
    public string Realm { get; set; } = "familytools";

    /// <summary>Client confidentiel disposant du service account "manage-users".</summary>
    public string ServiceClientId { get; set; } = "familytools-account-svc";

    /// <summary>Secret du client confidentiel (a fournir hors source en prod).</summary>
    public string ServiceClientSecret { get; set; } = string.Empty;
}

/// <summary>Exception portant un message destine a l'utilisateur final (ex. violation de politique).</summary>
public sealed class KeycloakUserFacingException(string message) : Exception(message);

/// <summary>Representation partielle d'un utilisateur Keycloak.</summary>
public sealed class KeycloakUser
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("username")] public string? Username { get; set; }
    [JsonPropertyName("firstName")] public string? FirstName { get; set; }
    [JsonPropertyName("lastName")] public string? LastName { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("emailVerified")] public bool EmailVerified { get; set; }
    [JsonPropertyName("requiredActions")] public List<string>? RequiredActions { get; set; }
}

/// <summary>
/// Pont (BFF) vers l'API d'administration de Keycloak. Le portail FamilyTools ne parle qu'a
/// cette API ; c'est elle qui detient les droits d'administration (service account) et qui
/// applique les regles cote serveur (verification du mot de passe actuel, politique, 2FA).
/// </summary>
public sealed class KeycloakAdminClient(HttpClient http, IOptions<KeycloakAdminOptions> options)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly KeycloakAdminOptions _options = options.Value;

    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string Realm => _options.Realm;

    // --- Profil -------------------------------------------------------------

    public async Task<KeycloakUser?> GetUserAsync(string userId, CancellationToken ct)
    {
        using var request = await AdminRequestAsync(HttpMethod.Get, $"users/{userId}", ct);
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KeycloakUser>(Json, ct);
    }

    public async Task UpdateProfileAsync(string userId, string firstName, string lastName, string email, CancellationToken ct)
    {
        // On repart de l'utilisateur existant pour ne pas ecraser d'autres attributs.
        var current = await GetUserAsync(userId, ct)
            ?? throw new KeycloakUserFacingException("Compte introuvable.");

        var emailChanged = !string.Equals(current.Email, email, StringComparison.OrdinalIgnoreCase);

        var body = new Dictionary<string, object?>
        {
            ["firstName"] = firstName,
            ["lastName"] = lastName,
            ["email"] = email,
            // Pas de SMTP en local : on considere l'email comme verifie pour ne pas bloquer la connexion.
            ["emailVerified"] = emailChanged ? true : current.EmailVerified,
        };

        using var request = await AdminRequestAsync(HttpMethod.Put, $"users/{userId}", ct);
        request.Content = JsonContent.Create(body, options: Json);
        using var response = await http.SendAsync(request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    // --- Mot de passe -------------------------------------------------------

    /// <summary>Verifie le mot de passe actuel via un grant ROPC (sans creer de session exploitable).</summary>
    public async Task<bool> VerifyCurrentPasswordAsync(string username, string password, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _options.ServiceClientId,
            ["client_secret"] = _options.ServiceClientSecret,
            ["username"] = username,
            ["password"] = password,
            ["scope"] = "openid",
        });

        using var response = await http.PostAsync(
            $"realms/{Realm}/protocol/openid-connect/token", content, ct);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return false;
    }

    public async Task ResetPasswordAsync(string userId, string newPassword, CancellationToken ct)
    {
        var body = new { type = "password", value = newPassword, temporary = false };

        using var request = await AdminRequestAsync(HttpMethod.Put, $"users/{userId}/reset-password", ct);
        request.Content = JsonContent.Create(body, options: Json);
        using var response = await http.SendAsync(request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    // --- 2FA (TOTP) ---------------------------------------------------------

    public async Task<bool> HasOtpAsync(string userId, CancellationToken ct)
    {
        using var request = await AdminRequestAsync(HttpMethod.Get, $"users/{userId}/credentials", ct);
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var credentials = await response.Content.ReadFromJsonAsync<List<CredentialRepresentation>>(Json, ct);
        return credentials?.Any(c => string.Equals(c.Type, "otp", StringComparison.OrdinalIgnoreCase)) ?? false;
    }

    /// <summary>
    /// Active la 2FA : on positionne l'action requise CONFIGURE_TOTP. La saisie du secret et la
    /// validation du code se font sur la page (themee) de Keycloak a la prochaine connexion, ou
    /// l'enrolement TOTP est gere de maniere sure (on ne re-implemente pas la cryptographie OTP).
    /// </summary>
    public async Task EnableOtpAsync(string userId, CancellationToken ct)
    {
        var user = await GetUserAsync(userId, ct)
            ?? throw new KeycloakUserFacingException("Compte introuvable.");

        var actions = new HashSet<string>(user.RequiredActions ?? [], StringComparer.OrdinalIgnoreCase)
        {
            "CONFIGURE_TOTP",
        };

        using var request = await AdminRequestAsync(HttpMethod.Put, $"users/{userId}", ct);
        request.Content = JsonContent.Create(new { requiredActions = actions.ToArray() }, options: Json);
        using var response = await http.SendAsync(request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    /// <summary>Desactive la 2FA : suppression des identifiants OTP et de l'action requise eventuelle.</summary>
    public async Task DisableOtpAsync(string userId, CancellationToken ct)
    {
        using var listRequest = await AdminRequestAsync(HttpMethod.Get, $"users/{userId}/credentials", ct);
        using var listResponse = await http.SendAsync(listRequest, ct);
        listResponse.EnsureSuccessStatusCode();
        var credentials = await listResponse.Content.ReadFromJsonAsync<List<CredentialRepresentation>>(Json, ct) ?? [];

        foreach (var otp in credentials.Where(c => string.Equals(c.Type, "otp", StringComparison.OrdinalIgnoreCase)))
        {
            using var del = await AdminRequestAsync(HttpMethod.Delete, $"users/{userId}/credentials/{otp.Id}", ct);
            using var delResponse = await http.SendAsync(del, ct);
            delResponse.EnsureSuccessStatusCode();
        }

        // Retire CONFIGURE_TOTP si l'utilisateur l'avait demande puis annule avant la prochaine connexion.
        var user = await GetUserAsync(userId, ct);
        if (user?.RequiredActions?.Any(a => a.Equals("CONFIGURE_TOTP", StringComparison.OrdinalIgnoreCase)) == true)
        {
            var remaining = user.RequiredActions
                .Where(a => !a.Equals("CONFIGURE_TOTP", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            using var request = await AdminRequestAsync(HttpMethod.Put, $"users/{userId}", ct);
            request.Content = JsonContent.Create(new { requiredActions = remaining }, options: Json);
            using var response = await http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
    }

    // --- Infrastructure -----------------------------------------------------

    private async Task<HttpRequestMessage> AdminRequestAsync(HttpMethod method, string path, CancellationToken ct)
    {
        var token = await GetAdminTokenAsync(ct);
        var request = new HttpRequestMessage(method, $"admin/realms/{Realm}/{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _options.ServiceClientId,
                ["client_secret"] = _options.ServiceClientSecret,
            });

            using var response = await http.PostAsync(
                $"realms/{Realm}/protocol/openid-connect/token", content, ct);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(Json, ct)
                ?? throw new InvalidOperationException("Reponse de jeton Keycloak vide.");

            _cachedToken = token.AccessToken;
            // Marge de securite de 30 s avant l'expiration reelle.
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(Math.Max(10, token.ExpiresIn - 30));
            return _cachedToken!;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <summary>Transforme un 4xx Keycloak en message exploitable (politique de mot de passe, etc.).</summary>
    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
        {
            throw new KeycloakUserFacingException(TranslateError(payload));
        }

        response.EnsureSuccessStatusCode();
    }

    /// <summary>Traduit les codes d'erreur Keycloak les plus courants en messages francais.</summary>
    private static string TranslateError(string payload)
    {
        string? code = null;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("errorMessage", out var em))
            {
                code = em.GetString();
            }
            else if (doc.RootElement.TryGetProperty("error_description", out var ed))
            {
                code = ed.GetString();
            }
            else if (doc.RootElement.TryGetProperty("error", out var e))
            {
                code = e.GetString();
            }
        }
        catch (JsonException)
        {
            // payload non-JSON : on retombe sur un message generique.
        }

        return code switch
        {
            null or "" => "La modification a ete refusee par le serveur d'identite.",
            "invalidPasswordMinLengthMessage" => "Le mot de passe doit contenir au moins 8 caracteres.",
            "invalidPasswordMinUpperCaseCharsMessage" => "Le mot de passe doit contenir au moins une majuscule.",
            "invalidPasswordMinLowerCaseCharsMessage" => "Le mot de passe doit contenir au moins une minuscule.",
            "invalidPasswordMinSpecialCharsMessage" => "Le mot de passe doit contenir au moins un caractere special.",
            "invalidPasswordNotUsernameMessage" => "Le mot de passe ne doit pas etre identique au nom d'utilisateur.",
            "invalidPasswordHistoryMessage" => "Ce mot de passe a deja ete utilise recemment.",
            _ when code.Contains("email", StringComparison.OrdinalIgnoreCase)
                => "Cette adresse email est invalide ou deja utilisee.",
            _ => "La modification a ete refusee : " + code,
        };
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    }

    private sealed class CredentialRepresentation
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
    }
}
