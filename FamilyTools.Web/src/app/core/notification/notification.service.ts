import { Injectable, signal } from '@angular/core';

export interface AppNotification {
  id: number;
  message: string;
  type: 'error' | 'info';
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private nextId = 0;
  readonly notifications = signal<AppNotification[]>([]);

  public showError(message: string, durationMs = 6000): void {
    this.push(message, 'error', durationMs);
  }

  public showInfo(message: string, durationMs = 4000): void {
    this.push(message, 'info', durationMs);
  }

  public dismiss(id: number): void {
    this.notifications.update(list => list.filter(n => n.id !== id));
  }

  private push(message: string, type: AppNotification['type'], durationMs: number): void {
    const id = this.nextId++;
    this.notifications.update(list => [...list, { id, message, type }]);

    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }
  }
}
