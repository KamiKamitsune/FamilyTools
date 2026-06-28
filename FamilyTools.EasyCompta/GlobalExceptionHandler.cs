using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTools.EasyCompta;

/// <summary>
/// Capture toute exception non gérée, la journalise côté serveur (avec la stack)
/// et renvoie une réponse ProblemDetails générique, sans fuite de détail interne.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = Map(exception);

        // 5xx = imprévu (log Error + stack) ; 4xx = erreur client attendue (log Warning)
        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erreur non gérée sur {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning(exception, "Requête rejetée ({Status}) sur {Method} {Path}",
                status, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title
            }
        });
    }

    private static (int Status, string Title) Map(Exception exception) => exception switch
    {
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Ressource introuvable."),
        ArgumentException or ValidationException => (StatusCodes.Status400BadRequest, "Requête invalide."),
        _ => (StatusCodes.Status500InternalServerError, "Une erreur interne est survenue.")
    };
}
