import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '@core/notification/notification.service';

/**
 * Capture toute erreur HTTP, en informe l'utilisateur via NotificationService,
 * la journalise, puis la relaie (throwError) pour que l'appelant puisse réagir.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      notifications.showError(buildMessage(error));
      console.error('Erreur HTTP', req.method, req.url, error);
      return throwError(() => error);
    })
  );
};

function buildMessage(error: HttpErrorResponse): string {
  // status 0 = pas de réponse (serveur injoignable, CORS, réseau)
  if (error.status === 0) {
    return 'Impossible de joindre le serveur. Vérifiez votre connexion.';
  }

  // Certaines API renvoient un message metier explicite : { message: "..." }
  const problem = error.error;
  if (problem && typeof problem === 'object' && typeof problem.message === 'string') {
    return problem.message;
  }

  // ProblemDetails ASP.NET Core : { title, status, detail }
  if (problem && typeof problem === 'object' && typeof problem.title === 'string') {
    return problem.title;
  }

  return `Une erreur est survenue (${error.status}).`;
}
