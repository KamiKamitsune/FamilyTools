import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NotificationComponent } from '@shared/ui/notification/notification.component';
import { AuthService } from '@core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, NotificationComponent],
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'FamilyTools.Web';

  private readonly auth = inject(AuthService);

  readonly userName = this.auth.userName;
  readonly isAuthenticated = this.auth.isAuthenticated;

  logout(): void {
    void this.auth.logout();
  }
}
