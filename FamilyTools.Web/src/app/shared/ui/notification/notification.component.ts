import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService } from '@core/notification/notification.service';

@Component({
  selector: 'app-notification',
  imports: [],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationComponent {
  private readonly service = inject(NotificationService);
  readonly notifications = this.service.notifications;

  public dismiss(id: number): void {
    this.service.dismiss(id);
  }
}
