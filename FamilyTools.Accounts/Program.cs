using FamilyTools.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Resource server dedie : cette API valide les jetons emis pour l'audience "accounts-api".
builder.AddFamilyToolsAuthentication(audience: "accounts-api");

// Connexion obligatoire : tout endpoint sans [AllowAnonymous] exige un utilisateur authentifie.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Pont vers l'API Admin de Keycloak (URL resolue par le service discovery Aspire : ressource "keycloak").
builder.Services.Configure<KeycloakAdminOptions>(builder.Configuration.GetSection("Keycloak"));
builder.Services.AddHttpClient<KeycloakAdminClient>(client =>
{
    client.BaseAddress = new Uri("http://keycloak");
});

var app = builder.Build();

// Les erreurs "metier" (mot de passe trop faible, email deja pris...) deviennent un 400 lisible.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (error is KeycloakUserFacingException facing)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = facing.Message });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Une erreur interne est survenue." });
        }
    }
});

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAccountEndpoints();

app.Run();
