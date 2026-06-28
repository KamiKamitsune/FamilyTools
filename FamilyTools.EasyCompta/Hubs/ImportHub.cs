using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FamilyTools.EasyCompta.Hubs;

/// <summary>
/// Hub temps réel des imports CSV. Le serveur émet l'événement <c>importCompleted</c>
/// lorsqu'un import a fini d'être traité et persisté ; le front s'y abonne pour se rafraîchir.
/// Réservé aux utilisateurs authentifiés (le jeton arrive en query string, voir AuthenticationExtensions).
/// </summary>
[Authorize]
public sealed class ImportHub : Hub;
