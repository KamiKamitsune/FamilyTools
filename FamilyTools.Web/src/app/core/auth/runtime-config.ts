/**
 * Configuration chargee a l'EXECUTION (et non figee au build) : permet de pointer le SPA
 * vers le bon serveur Keycloak selon l'environnement (dev local vs VM) sans recompiler.
 *
 * Le fichier `config.json` est servi a la racine (dossier `public/`). En CI, le job `deploy`
 * le regenere avec l'URL publique reelle de la VM. En dev, la valeur par defaut (localhost:8080)
 * correspond au Keycloak demarre par l'AppHost Aspire.
 *
 * IMPORTANT : `loadRuntimeConfig()` doit etre appele AVANT `bootstrapApplication` (cf. main.ts),
 * pour que la valeur soit disponible quand `AuthService` construit son `UserManager`.
 */
export interface RuntimeConfig {
  /** Autorite OIDC complete, ex. `http://192.168.8.210:8080/realms/familytools`. */
  keycloakAuthority: string;
}

const DEFAULT_CONFIG: RuntimeConfig = {
  keycloakAuthority: 'http://localhost:8080/realms/familytools',
};

let current: RuntimeConfig = DEFAULT_CONFIG;

/** Charge `config.json` ; en cas d'echec, conserve les valeurs par defaut (dev). */
export async function loadRuntimeConfig(): Promise<void> {
  try {
    const res = await fetch('config.json', { cache: 'no-store' });
    if (res.ok) {
      current = { ...DEFAULT_CONFIG, ...(await res.json()) };
    }
  } catch {
    // Reseau indisponible ou fichier absent : on garde la config par defaut.
  }
}

/** Config courante (valeurs par defaut tant que `loadRuntimeConfig` n'a pas resolu). */
export function runtimeConfig(): RuntimeConfig {
  return current;
}
