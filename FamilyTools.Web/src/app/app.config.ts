import {ApplicationConfig, inject, provideZoneChangeDetection} from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import {provideRouter, Router, withNavigationErrorHandler} from '@angular/router';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes,
      withNavigationErrorHandler((error) => {
        const router = inject(Router);
        if (error?.error) {
          console.error('Navigation error occurred:', error.error.message());
        }
        router.navigate(['/error']);
      }),),
    provideHttpClient()
  ]
};
