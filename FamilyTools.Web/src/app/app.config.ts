import {ApplicationConfig, inject, provideAppInitializer, provideZoneChangeDetection} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {provideRouter, Router, withComponentInputBinding, withNavigationErrorHandler} from '@angular/router';

import { routes } from './app.routes';
import { errorInterceptor } from '@core/http/error.interceptor';
import { authInterceptor } from '@core/auth/auth.interceptor';
import { AuthService } from '@core/auth/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    // Restaure la session OIDC (refresh silencieux) avant le rendu de l'app.
    provideAppInitializer(() => inject(AuthService).init()),
    provideRouter(routes,
      // Lie les `data`/params de route aux @Input des composants (pages d'erreur).
      withComponentInputBinding(),
      withNavigationErrorHandler((event) => {
        // Une navigation qui echoue (ex. guard rejete, Keycloak injoignable) renvoie
        // vers la page d'erreur generique au lieu de laisser un ecran fige.
        console.error('Erreur de navigation', event?.error);
        void inject(Router).navigate(['/error']);
      }),),
    // authInterceptor en premier : il pose le Bearer, errorInterceptor gere les erreurs.
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))
  ]
};
