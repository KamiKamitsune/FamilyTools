import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Garde de route : exige une session valide. Sinon, declenche la connexion Keycloak
 * et memorise l'URL demandee pour y revenir apres login.
 */
export const authGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthService);

  if (auth.isAuthenticated()) {
    return true;
  }

  await auth.login(state.url);
  return false;
};

/**
 * Garde reservee aux administrateurs (role realm "admin").
 * A appliquer sur les ecrans de gestion sensibles.
 */
export const adminGuard: CanActivateFn = async (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    await auth.login(state.url);
    return false;
  }

  // Connecte mais role insuffisant : rediriger vers la page 403 (plutot qu'un blocage muet).
  return auth.isAdmin() || router.createUrlTree(['/forbidden']);
};
