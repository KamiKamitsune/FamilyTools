import { inject, Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from '@core/auth/auth.service';

/**
 * Connexion SignalR au hub d'imports de l'API. Émet `importCompleted$` chaque fois que
 * le serveur signale qu'un import CSV a fini d'être traité et persisté.
 */
@Injectable({ providedIn: 'root' })
export class ImportEventsService {
  private readonly auth = inject(AuthService);
  private connection?: HubConnection;
  private readonly completed = new Subject<void>();

  readonly importCompleted$ = this.completed.asObservable();

  async connect(): Promise<void> {
    if (this.connection) {
      return;
    }

    this.connection = new HubConnectionBuilder()
      // Le hub est protege : on fournit le jeton (transmis en query string par SignalR).
      .withUrl('/hubs/import', { accessTokenFactory: () => this.auth.accessToken ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('importCompleted', () => this.completed.next());

    try {
      await this.connection.start();
    } catch (error) {
      // Pas de temps réel disponible : l'utilisateur pourra rafraîchir manuellement.
      console.error("Connexion au hub d'imports impossible", error);
    }
  }
}
