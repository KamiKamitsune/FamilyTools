import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AccountEnterFormComponent } from './account-enter-form/account-enter-form.component';
import { AccountEnter } from '@easycompta/models/account-enter';

@Component({
  selector: 'app-accountenter',
  imports: [AccountEnterFormComponent],
  templateUrl: './account-enter.component.html',
  styleUrl: './account-enter.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountEnterComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly data = toSignal(this.route.data, { requireSync: true });
  readonly enter = computed(() => this.data()['enter'] as AccountEnter);
}
