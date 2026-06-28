import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AccountTag } from '@easycompta/models/account-tag';
import { TagFormComponent } from './tag-form/tag-form.component';
import { AccountTagService } from '@easycompta/data/account-tag.service';

@Component({
  selector: 'app-accounttag',
  imports: [TagFormComponent, RouterLink],
  templateUrl: './account-tag.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './account-tag.component.css',
})
export class AccountTagComponent {
  readonly service = inject(AccountTagService);

  /** Tag en cours d'édition ; `null` => le formulaire est en mode création. */
  readonly editingTag = signal<AccountTag | null>(null);

  editTag(tag: AccountTag): void {
    this.editingTag.set(tag);
  }

  deleteTag(id: number): void {
    this.service.delete(id);
    if (this.editingTag()?.id === id) this.editingTag.set(null);
  }

  onSaved(): void {
    this.editingTag.set(null);
  }
}
