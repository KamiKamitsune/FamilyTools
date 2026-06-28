import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { loadRuntimeConfig } from './app/core/auth/runtime-config';

// Charge la config d'execution (config.json : URL Keycloak) AVANT le bootstrap, afin que
// l'AuthService construise son UserManager avec la bonne authority.
loadRuntimeConfig()
  .then(() => bootstrapApplication(AppComponent, appConfig))
  .catch((err) => console.error(err));
