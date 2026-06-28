using FamilyTools.Data.Context;
using FamilyTools.EasyCompta.Business;
using FamilyTools.EasyCompta.Hubs;
using FamilyTools.EasyCompta.Services;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.AddSqlServerDbContext<EasyComptaContext>("easycompta");
builder.AddRedisOutputCache("cache");
builder.AddRabbitMQClient(connectionName: "messaging");

builder.AddServiceDefaults();

// Authentification SSO via Keycloak + policies (voir FamilyTools.ServiceDefaults).
builder.AddFamilyToolsAuthentication();

// Connexion obligatoire : tout endpoint sans [AllowAnonymous] exige un utilisateur authentifié.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// Couche métier (partagée avec le worker d'import).
builder.Services.AddEasyComptaBusiness();

// Import CSV : l'API publie les fichiers reçus dans RabbitMQ ; le worker dédié les consomme.
builder.Services.AddSingleton<ICsvImportPublisher, CsvImportPublisher>();

// Notifications temps réel : l'API consomme l'événement de fin d'import (publié par le worker)
// et le relaie aux clients via SignalR.
builder.Services.AddSignalR();
builder.Services.AddHostedService<CsvImportEventsConsumer>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Rendre le JSON plus lisible
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<FamilyTools.EasyCompta.GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi().AllowAnonymous();

app.UseHttpsRedirection();

app.UseCors("frontend");

app.UseMiddleware<FamilyTools.EasyCompta.ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapControllers();

app.MapHub<ImportHub>("/hubs/import");

if (app.Environment.IsDevelopment())
{
    //using (var scope = app.Services.CreateScope())
    //{
    //    var context = scope.ServiceProvider.GetRequiredService<EasyComptaContext>();
    //    context.Database.EnsureCreated();
    //    context.EnsureSeedData().GetAwaiter().GetResult();
    //}
}
else
{
    // The default HSTS value is 30 days.
    // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Run();