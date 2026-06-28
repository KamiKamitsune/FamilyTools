import { computed, Injectable, signal } from '@angular/core';
import { User, UserManager } from 'oidc-client-ts';
import { buildAuthConfig } from './auth.config';

/**
 * Service d'authentification SSO (Keycloak via OIDC).
 *
 * Volontairement sans wrapper Angular tiers (coherent avec le choix chart.js) : on pilote
 * directement `oidc-client-ts`. Expose l'etat de connexion sous forme de signals.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userManager = new UserManager(buildAuthConfig());
  private readonly currentUser = signal<User | null>(null);

  /** Utilisateur courant (null si deconnecte). */
  readonly user = this.currentUser.asReadonly();
  /** Vrai si une session valide (non expiree) est presente. */
  readonly isAuthenticated = computed(() => {
    const user = this.currentUser();
    return user !== null && !user.expired;
  });
  /** Nom affichable de l'utilisateur connecte. */
  readonly userName = computed(
    () => (this.currentUser()?.profile?.['preferred_username'] as string | undefined) ?? null
  );
  /** Vrai si l'utilisateur possede le role realm "admin". */
  readonly isAdmin = computed(() => this.rolesOf(this.currentUser()).includes('admin'));

  constructor() {
    this.userManager.events.addUserLoaded(user => this.currentUser.set(user));
    this.userManager.events.addUserUnloaded(() => this.currentUser.set(null));
    this.userManager.events.addAccessTokenExpired(() => {
      // Tente un renouvellement silencieux ; a defaut on repart sur un login.
      this.userManager.signinSilent().catch(() => this.currentUser.set(null));
    });
    this.userManager.events.addSilentRenewError(() => this.currentUser.set(null));
  }

  /**
   * Restaure la session au demarrage de l'app (renouvellement silencieux si le token a expire).
   * Appele depuis un APP_INITIALIZER.
   */
  async init(): Promise<void> {
    try {
      let user = await this.userManager.getUser();
      if (user && user.expired) {
        user = await this.userManager.signinSilent().catch(() => null);
      }
      this.currentUser.set(user);
    } catch {
      this.currentUser.set(null);
    }
  }

  /** Redirige vers la page de connexion Keycloak. */
  login(returnUrl: string = window.location.pathname + window.location.search): Promise<void> {
    return this.userManager.signinRedirect({ state: { returnUrl } });
  }

  /** Termine le flux de connexion (route de callback) et renvoie l'URL de retour. */
  async completeLogin(): Promise<string> {
    const user = await this.userManager.signinCallback();
    this.currentUser.set(user ?? null);
    const state = user?.state as { returnUrl?: string } | undefined;
    return state?.returnUrl ?? '/';
  }

  /** Deconnexion globale (termine aussi la session Keycloak). */
  logout(): Promise<void> {
    return this.userManager.signoutRedirect();
  }

  /** Jeton d'acces courant, a joindre aux appels API. */
  get accessToken(): string | null {
    return this.currentUser()?.access_token ?? null;
  }

  /** Extrait les roles realm depuis le payload du jeton d'acces. */
  private rolesOf(user: User | null): string[] {
    const token = user?.access_token;
    if (!token) {
      return [];
    }

    try {
      const payloadSegment = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const payload = JSON.parse(atob(payloadSegment));
      return (payload?.realm_access?.roles as string[] | undefined) ?? [];
    } catch {
      return [];
    }
  }
}
