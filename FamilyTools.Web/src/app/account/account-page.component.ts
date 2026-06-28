import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AccountService } from './account.service';
import { NotificationService } from '@core/notification/notification.service';

/** Politique de mot de passe (alignee sur Keycloak) : 8+, majuscule, minuscule, caractere special. */
function passwordPolicy(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string;
  if (!value) {
    return null;
  }
  const ok =
    value.length >= 8 &&
    /[A-Z]/.test(value) &&
    /[a-z]/.test(value) &&
    /[^A-Za-z0-9]/.test(value);
  return ok ? null : { passwordPolicy: true };
}

/** Confirme que les deux mots de passe saisis sont identiques. */
function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const pwd = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pwd && confirm && pwd !== confirm ? { mismatch: true } : null;
}

@Component({
  selector: 'app-account-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './account-page.component.html',
  styleUrl: './account-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountPageComponent {
  private readonly account = inject(AccountService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(true);
  readonly savingProfile = signal(false);
  readonly savingPassword = signal(false);
  readonly savingTwoFactor = signal(false);
  readonly twoFactorEnabled = signal(false);
  readonly username = signal('');

  readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
  });

  readonly passwordForm = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, passwordPolicy]],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordsMatch }
  );

  constructor() {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.loading.set(true);
    this.account.getProfile().subscribe({
      next: profile => {
        this.username.set(profile.username);
        this.twoFactorEnabled.set(profile.twoFactorEnabled);
        this.profileForm.patchValue({
          firstName: profile.firstName,
          lastName: profile.lastName,
          email: profile.email,
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.savingProfile.set(true);
    this.account.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: () => {
        this.notifications.showInfo('Profil mis a jour.');
        this.savingProfile.set(false);
      },
      error: () => this.savingProfile.set(false),
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    this.savingPassword.set(true);
    const { currentPassword, newPassword } = this.passwordForm.getRawValue();
    this.account.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.notifications.showInfo('Mot de passe modifie.');
        this.passwordForm.reset();
        this.savingPassword.set(false);
      },
      error: () => this.savingPassword.set(false),
    });
  }

  toggleTwoFactor(): void {
    this.savingTwoFactor.set(true);
    const request = this.twoFactorEnabled()
      ? this.account.disableTwoFactor()
      : this.account.enableTwoFactor();

    request.subscribe({
      next: result => {
        this.notifications.showInfo(result.message);
        this.savingTwoFactor.set(false);
        // L'etat reel est relu cote serveur (l'activation se finalise a la prochaine connexion).
        this.loadProfile();
      },
      error: () => this.savingTwoFactor.set(false),
    });
  }
}
