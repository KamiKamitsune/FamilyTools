import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Route de retour du flux OIDC (redirect_uri = /auth/callback).
 * Finalise la connexion puis redirige vers la page initialement demandee.
 */
@Component({
  selector: 'app-auth-callback',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '<p>Connexion en cours…</p>',
})
export class AuthCallbackComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  async ngOnInit(): Promise<void> {
    try {
      const returnUrl = await this.auth.completeLogin();
      await this.router.navigateByUrl(returnUrl);
    } catch {
      await this.router.navigateByUrl('/');
    }
  }
}
