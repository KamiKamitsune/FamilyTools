import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Page d'erreur generique (code + titre + message), reutilisee pour toutes les routes
 * d'erreur. Le contenu est fourni par les `data` de route, liees aux @Input grace a
 * `withComponentInputBinding()` (cf. app.config.ts) :
 *   - /error      -> 500 (erreur inattendue / navigation echouee)
 *   - /forbidden  -> 403 (connecte mais role insuffisant, cf. adminGuard)
 *   - **          -> 404 (route inconnue)
 */
@Component({
  selector: 'app-error-page',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="error-page">
      <div class="error-card">
        @if (code()) {
          <p class="error-code">{{ code() }}</p>
        }
        <h1 class="error-title">{{ title() }}</h1>
        <p class="error-message">{{ message() }}</p>
        <a class="error-home" routerLink="/">Retour à l'accueil</a>
      </div>
    </div>
  `,
  styles: `
    .error-page {
      display: flex;
      justify-content: center;
      padding: 60px 20px;
    }

    .error-card {
      background: #fff;
      border-radius: 20px;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
      padding: 48px 40px;
      text-align: center;
      max-width: 440px;
      width: 100%;
    }

    .error-code {
      font-size: 56px;
      font-weight: 700;
      color: #3f51b5;
      line-height: 1;
      margin-bottom: 12px;
    }

    .error-title {
      font-size: 24px;
      color: #333;
      margin-bottom: 12px;
    }

    .error-message {
      font-size: 15px;
      color: #666;
      margin-bottom: 28px;
    }

    .error-home {
      display: inline-block;
      background: #3f51b5;
      color: #fff;
      text-decoration: none;
      font-weight: 500;
      border-radius: 8px;
      padding: 10px 22px;
      transition: background 0.2s;
    }

    .error-home:hover {
      background: #303f9f;
    }
  `,
})
export class ErrorPageComponent {
  /** Code affiche en gros (ex. "404"). Vide = pas de code affiche. */
  readonly code = input('');
  /** Titre de la page. */
  readonly title = input('Une erreur est survenue');
  /** Message explicatif. */
  readonly message = input("Une erreur inattendue s'est produite. Veuillez réessayer.");
}
