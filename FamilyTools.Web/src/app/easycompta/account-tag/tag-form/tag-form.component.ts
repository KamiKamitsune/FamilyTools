import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { AccountTag } from '@easycompta/models/account-tag';
import { form, FormField, required } from '@angular/forms/signals';
import { AccountTagService } from '@easycompta/data/account-tag.service';

interface TagModel {
  id?: number;
  name: string;
  color: string;
}

/** Formulaire de création / édition d'un tag (nom + couleur). */
@Component({
  selector: 'app-tag-form',
  imports: [FormField],
  templateUrl: './tag-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './tag-form.component.css',
})
export class TagFormComponent {
  private readonly service = inject(AccountTagService);

  /** Tag à éditer ; `null` => mode création. */
  readonly tagToEdit = input<AccountTag | null>(null);
  /** Émis après création / mise à jour pour que le parent réinitialise la sélection. */
  readonly saved = output<void>();

  readonly tagModel = signal<TagModel>(this.empty());
  readonly isEdit = computed(() => this.tagModel().id != null);

  readonly tagForm = form(this.tagModel, schemaPath => {
    required(schemaPath.name, { message: 'Le nom est requis' });
    required(schemaPath.color, { message: 'La couleur est requise' });
  });

  constructor() {
    // Recharge le modèle quand le parent change le tag à éditer.
    effect(() => {
      const tag = this.tagToEdit();
      this.tagModel.set(tag ? { id: tag.id, name: tag.name, color: tag.color } : this.empty());
    });
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    if (this.tagForm().invalid()) return;

    const tag = this.tagModel();
    if (tag.id != null) {
      this.service.update({ id: tag.id, name: tag.name, color: tag.color });
    } else {
      this.service.create({ name: tag.name, color: tag.color });
    }

    this.cancel();
  }

  cancel(): void {
    this.tagModel.set(this.empty());
    this.tagForm().reset();
    this.saved.emit();
  }

  private empty(): TagModel {
    return { name: '', color: '#2196F3' };
  }
}
