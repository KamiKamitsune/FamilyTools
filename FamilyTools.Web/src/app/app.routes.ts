import { Routes } from '@angular/router';
import { authGuard } from '@core/auth/auth.guard';
import { AuthCallbackComponent } from '@core/auth/auth-callback.component';
import { ErrorPageComponent } from '@core/errors/error-page.component';

export const routes: Routes = [
  // Retour du flux de connexion Keycloak (non protege).
  { path: 'auth/callback', component: AuthCallbackComponent },
  {
    path: 'easycompta',
    canActivate: [authGuard],
    loadChildren: () => import('./easycompta/easycompta.routes').then(m => m.easycomptaRoutes),
  },
  {
    path: 'user',
    canActivate: [authGuard],
    loadChildren: () => import('./user/user.routes').then(m => m.userRoutes),
  },
  {
    path: 'account',
    canActivate: [authGuard],
    loadComponent: () => import('./account/account-page.component').then(m => m.AccountPageComponent),
  },
  { path: '', redirectTo: 'easycompta', pathMatch: 'full' },

  // Pages d'erreur (composant unique, configure via `data` -> @Input, cf. withComponentInputBinding).
  {
    path: 'error',
    component: ErrorPageComponent,
    data: {
      code: '500',
      title: 'Une erreur est survenue',
      message: "Une erreur inattendue s'est produite. Veuillez réessayer plus tard.",
    },
  },
  {
    path: 'forbidden',
    component: ErrorPageComponent,
    data: {
      code: '403',
      title: 'Accès refusé',
      message: "Vous n'avez pas les droits nécessaires pour accéder à cette page.",
    },
  },
  // Wildcard : doit rester EN DERNIER (toute route inconnue tombe ici).
  {
    path: '**',
    component: ErrorPageComponent,
    data: {
      code: '404',
      title: 'Page introuvable',
      message: "La page que vous recherchez n'existe pas ou a été déplacée.",
    },
  },
];
