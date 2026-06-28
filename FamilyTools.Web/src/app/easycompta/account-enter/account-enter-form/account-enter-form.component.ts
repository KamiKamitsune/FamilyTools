import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, untracked } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { form, FormField, min, required } from '@angular/forms/signals';
import { AccountEnter, AccountEnterDto } from '@easycompta/models/account-enter';
import { User } from '@shared/models/user';
import { UserService } from '@user/data/user.service';
import { AccountTagService } from '@easycompta/data/account-tag.service';
import { AccountEnterService } from '@easycompta/data/account-enter.service';
import { OperationTypeService } from '@easycompta/data/operation-type.service';

/** Champs scalaires gérés par le signal-form (validés). Les ids liés à un <select> sont des chaînes. */
interface ScalarModel {
  name: string;
  date: string;
  isDisabled: boolean;
  tagId: string;
  operationTypeId: string;
}

/** Ligne de répartition. `modified` = valeur saisie manuellement par l'utilisateur (transitoire). */
interface FormLine {
  id?: number;
  userId: string;
  value: number;
  modified: boolean;
}

@Component({
  selector: 'app-account-enter-form',
  imports: [FormField, DecimalPipe],
  templateUrl: './account-enter-form.component.html',
  styleUrl: './account-enter-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountEnterFormComponent {
  readonly userService = inject(UserService);
  readonly tagService = inject(AccountTagService);
  readonly operationTypeService = inject(OperationTypeService);
  private readonly enterService = inject(AccountEnterService);
  private readonly router = inject(Router);

  readonly enter = input<AccountEnter>();
  readonly isEdit = computed(() => !!this.enter()?.id);

  readonly scalarModel = linkedSignal<AccountEnter | undefined, ScalarModel>({
    source: this.enter,
    computation: enter => ({
      name: enter?.name ?? '',
      date: AccountEnterFormComponent.toDateInput(enter?.date ?? new Date()),
      isDisabled: enter?.isDisabled ?? false,
      tagId: String(enter?.tag?.id ?? 0),
      operationTypeId: String(enter?.operationType?.id ?? 0),
    }),
  });

  readonly enterForm = form(this.scalarModel, path => {
    required(path.name, { message: 'Le nom est requis' });
    required(path.date, { message: 'La date est requise' });
    min(path.tagId, 1, { message: 'Le tag est requis' });
    min(path.operationTypeId, 1, { message: "Le type d'opération est requis" });
  });

  /** Montant total (« valeur max ») réparti entre les membres. */
  readonly totalValue = linkedSignal<AccountEnter | undefined, number>({
    source: this.enter,
    computation: enter => enter?.totalValue ?? 0,
  });

  readonly lines = linkedSignal<AccountEnter | undefined, FormLine[]>({
    source: this.enter,
    computation: enter =>
      (enter?.lines ?? []).map(line => ({
        id: line.id,
        userId: String(line.user?.id ?? 0),
        value: line.value,
        modified: false,
      })),
  });

  /** Somme courante des lignes (doit coller au total ; affichée à titre de contrôle). */
  readonly distributedTotal = computed(() =>
    this.lines().reduce((sum, line) => sum + Number(line.value || 0), 0),
  );

  /** On ne peut ajouter une ligne que s'il reste un membre sans ligne. */
  readonly canAddLine = computed(() => this.lines().length < this.userService.users().length);

  /** Membres sélectionnables pour une ligne : ceux non déjà pris par une autre ligne (+ le sien). */
  availableUsers(currentUserId: string): User[] {
    const usedByOthers = new Set(
      this.lines().map(line => line.userId).filter(userId => userId !== currentUserId),
    );
    return this.userService.users().filter(user => !usedByOthers.has(String(user.id ?? 0)));
  }

  constructor() {
    // Création : une ligne par membre + tag par défaut présélectionné (dès que les données sont chargées).
    effect(() => {
      if (this.isEdit()) {
        return;
      }

      const users = this.userService.users();
      const tags = this.tagService.allTag();

      untracked(() => {
        if (users.length > 0 && this.lines().length === 0) {
          const values = AccountEnterFormComponent.splitEqually(this.totalValue(), users.length);
          this.lines.set(
            users.map((user, i) => ({ userId: String(user.id ?? 0), value: values[i] ?? 0, modified: false })),
          );
        }

        const model = this.scalarModel();
        if (tags.length > 0 && (model.tagId === '0' || model.tagId === '')) {
          this.scalarModel.set({ ...model, tagId: String(tags[0].id ?? 0) });
        }
      });
    });
  }

  /** Le total change -> répartition égale sur tous les membres, on réinitialise les lignes modifiées. */
  onTotalChange(event: Event): void {
    const total = AccountEnterFormComponent.parse((event.target as HTMLInputElement).value);
    this.totalValue.set(total);

    const values = AccountEnterFormComponent.splitEqually(total, this.lines().length);
    this.lines.update(lines => lines.map((line, i) => ({ ...line, value: values[i] ?? 0, modified: false })));
  }

  /** Une valeur est saisie pour un membre -> l'écart est reporté sur les membres non modifiés. */
  onLineValueChange(index: number, event: Event): void {
    const value = AccountEnterFormComponent.parse((event.target as HTMLInputElement).value);

    this.lines.update(lines => {
      const marked = lines.map((line, i) => (i === index ? { ...line, value, modified: true } : line));
      return AccountEnterFormComponent.redistributeToUnmodified(marked, this.totalValue());
    });
  }

  onLineUserChange(index: number, event: Event): void {
    const userId = (event.target as HTMLSelectElement).value;
    this.lines.update(lines => lines.map((line, i) => (i === index ? { ...line, userId } : line)));
  }

  /** Suppression d'un membre -> redispatch du total sur les restants, on réinitialise les lignes modifiées. */
  removeLine(index: number): void {
    this.lines.update(lines => {
      const remaining = lines.filter((_, i) => i !== index);
      const values = AccountEnterFormComponent.splitEqually(this.totalValue(), remaining.length);
      return remaining.map((line, i) => ({ ...line, value: values[i] ?? 0, modified: false }));
    });
  }

  /** Ajout d'un membre : on prend un membre encore libre, puis on répartit le total équitablement. */
  addLine(): void {
    const used = new Set(this.lines().map(line => line.userId));
    const nextUser = this.userService.users().find(user => !used.has(String(user.id ?? 0)));
    if (!nextUser) {
      return; // tous les membres ont déjà une ligne
    }

    this.lines.update(lines => {
      const extended = [...lines, { userId: String(nextUser.id ?? 0), value: 0, modified: false }];
      const values = AccountEnterFormComponent.splitEqually(this.totalValue(), extended.length);
      return extended.map((line, i) => ({ ...line, value: values[i] ?? 0, modified: false }));
    });
  }

  onSubmit(event: SubmitEvent): void {
    event.preventDefault();

    if (this.enterForm().invalid() || this.lines().some(line => Number(line.userId) <= 0)) {
      return;
    }

    const model = this.scalarModel();
    const dto: AccountEnterDto = {
      id: this.enter()?.id,
      name: model.name,
      date: model.date,
      isDisabled: model.isDisabled,
      tagId: Number(model.tagId),
      operationTypeId: Number(model.operationTypeId),
      totalValue: this.totalValue(),
      lines: this.lines().map(line => ({
        id: line.id,
        userId: Number(line.userId),
        value: Number(line.value),
      })),
    };

    const request$ = this.isEdit() ? this.enterService.update(dto) : this.enterService.create(dto);
    request$.subscribe({
      next: () => this.router.navigate(['/easycompta/account-page']),
    });
  }

  // --- Répartition (logique pure) ---

  /** Répartit un total au centime près ; le reliquat est porté par les premières lignes (somme exacte). */
  private static splitEqually(total: number, count: number): number[] {
    if (count <= 0) {
      return [];
    }

    const totalCents = Math.round(total * 100);
    const base = Math.trunc(totalCents / count);
    const remainder = totalCents - base * count;
    const sign = Math.sign(remainder);
    const extra = Math.abs(remainder);

    return Array.from({ length: count }, (_, i) => (base + (i < extra ? sign : 0)) / 100);
  }

  /** Garde les valeurs modifiées et répartit le reste du total sur les lignes non modifiées. */
  private static redistributeToUnmodified(lines: FormLine[], total: number): FormLine[] {
    const unmodifiedCount = lines.filter(line => !line.modified).length;
    if (unmodifiedCount === 0) {
      return lines;
    }

    const modifiedSum = lines.filter(line => line.modified).reduce((sum, line) => sum + Number(line.value), 0);
    const values = AccountEnterFormComponent.splitEqually(total - modifiedSum, unmodifiedCount);

    let k = 0;
    return lines.map(line => (line.modified ? line : { ...line, value: values[k++] ?? 0 }));
  }

  private static parse(value: string): number {
    const n = Number(value);
    return Number.isFinite(n) ? n : 0;
  }

  /** Normalise une date (string ISO ou Date) au format attendu par <input type="date"> (yyyy-MM-dd). */
  private static toDateInput(value: Date | string): string {
    return typeof value === 'string' ? value.slice(0, 10) : value.toISOString().slice(0, 10);
  }
}
