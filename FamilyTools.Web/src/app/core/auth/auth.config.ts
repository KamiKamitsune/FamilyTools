import { UserManagerSettings, WebStorageStateStore } from 'oidc-client-ts';
import { runtimeConfig } from './runtime-config';

/**
 * Configuration OIDC du client SPA face a Keycloak (realm "familytools").
 *
 * L'`authority` (URL du serveur Keycloak) est lue dans la config d'EXECUTION
 * (`runtime-config` / `config.json`), pas codee en dur : le meme build sert en dev
 * (localhost:8080, AppHost Aspire) comme sur la VM (URL publique injectee par la CI).
 *
 * Fabrique appelee paresseusement par `AuthService` : `loadRuntimeConfig()` (main.ts)
 * a alors deja resolu la valeur.
 */
const CLIENT_ID = 'familytools-web';

export function buildAuthConfig(): UserManagerSettings {
  const origin = window.location.origin;

  return {
    authority: runtimeConfig().keycloakAuthority,
    client_id: CLIENT_ID,
    redirect_uri: `${origin}/auth/callback`,
    post_logout_redirect_uri: origin,
    response_type: 'code', // Authorization Code Flow + PKCE (client public)
    scope: 'openid profile email',

    // Renouvellement silencieux via refresh token (pas d'iframe) -> pas de reconnexion.
    automaticSilentRenew: true,
    // On ne surveille pas la session via iframe (evite les soucis de cookies tiers).
    monitorSession: false,

    // Persiste la session dans localStorage : survit au rechargement de page.
    userStore: new WebStorageStateStore({ store: window.localStorage }),
  };
}
