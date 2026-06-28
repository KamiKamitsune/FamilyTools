import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

/** Profil self-service de l'utilisateur connecte (source : Keycloak via l'API Accounts). */
export interface AccountProfile {
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  twoFactorEnabled: boolean;
}

export interface UpdateProfile {
  firstName: string;
  lastName: string;
  email: string;
}

export interface ChangePassword {
  currentPassword: string;
  newPassword: string;
}

/**
 * Acces a l'API transversale "Mon compte" (resource server Accounts, prefixe proxy /identity).
 * Le jeton Bearer est ajoute automatiquement par authInterceptor.
 */
@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly base = '/identity/account';

  getProfile(): Observable<AccountProfile> {
    return this.http.get<AccountProfile>(`${this.base}/profile`);
  }

  updateProfile(body: UpdateProfile): Observable<void> {
    return this.http.put<void>(`${this.base}/profile`, body);
  }

  changePassword(body: ChangePassword): Observable<void> {
    return this.http.put<void>(`${this.base}/password`, body);
  }

  enableTwoFactor(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/2fa/enable`, {});
  }

  disableTwoFactor(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/2fa/disable`, {});
  }
}
