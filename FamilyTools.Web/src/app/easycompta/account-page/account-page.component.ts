import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AccountPage } from '@easycompta/models/account-page';
import { AccountEnter } from '@easycompta/models/account-enter';
import { AccountPageService } from '@easycompta/data/account-page.service';
import { AccountEnterService } from '@easycompta/data/account-enter.service';
import { AccountTagService } from '@easycompta/data/account-tag.service';
import { ImportEventsService } from '@easycompta/data/import-events.service';
import { UserService } from '@user/data/user.service';
import { NotificationService } from '@core/notification/notification.service';

@Component({
  selector: 'app-accountpage',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './account-page.component.html',
  styleUrl: './account-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountPageComponent implements OnInit {
  private readonly pageService = inject(AccountPageService);
  private readonly enterService = inject(AccountEnterService);
  readonly tagService = inject(AccountTagService);
  readonly userService = inject(UserService);
  private readonly importEvents = inject(ImportEventsService);
  private readonly notifications = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly currentPage = signal<AccountPage | undefined>(undefined);
  readonly dateList = signal<Date[]>([]);
  readonly lastDate = signal<Date>(new Date());
  readonly selectedFile = signal<File | undefined>(undefined);

  readonly selectedTagId = signal<number | null>(null);
  readonly selectedUserId = signal<number | null>(null);

  /** Écritures de la page courante après application des filtres catégorie / personne. */
  readonly filteredEnters = computed<AccountEnter[]>(() => {
    const enters = this.currentPage()?.enters ?? [];
    const tagId = this.selectedTagId();
    const userId = this.selectedUserId();
    return enters.filter(
      enter =>
        (tagId === null || enter.tag?.id === tagId) &&
        (userId === null || (enter.lines ?? []).some(line => line.user?.id === userId)),
    );
  });

  @ViewChild('addCsvButton', { static: false }) addCsvButton!: ElementRef<HTMLInputElement>;

  ngOnInit(): void {
    this.loadMonths();

    // Rafraîchissement temps réel : quand le worker a fini de traiter un import, on recharge.
    this.importEvents.connect();
    this.importEvents.importCompleted$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.notifications.showInfo('Import terminé : données mises à jour.');
        this.loadMonths();
      });
  }

  public onTagFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedTagId.set(value ? Number(value) : null);
  }

  public onUserFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedUserId.set(value ? Number(value) : null);
  }

  public deleteEnter(id: number): void {
    this.enterService.delete(id).subscribe({
      next: () => {
        const page = this.currentPage();
        if (!page) return;
        this.currentPage.set({ ...page, enters: page.enters.filter(enter => enter.id !== id) });
        if (page.id != null) {
          this.refreshPaymentDones(page.id);
        }
      },
      error: error => console.error(error),
    });
  }

  public changePages(event: Event): void {
    const value = new Date((event.target as HTMLSelectElement).value);
    this.loadPage(value.getMonth(), value.getFullYear());
  }

  public onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0]);
  }

  public uploadFile(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.pageService.importCsv(file).subscribe({
      next: () => {
        this.addCsvButton.nativeElement.value = '';
        this.selectedFile.set(undefined);
        // Le traitement est asynchrone (worker) : le tableau se rafraîchira via SignalR.
        this.notifications.showInfo('Import en cours… le tableau se rafraîchira automatiquement.');
      },
      error: error => console.error("Erreur lors de l'envoi", error),
    });
  }

  public clickPaymentDone(event: Event): void {
    const input = event.target as HTMLInputElement;
    const id = Number(input.id);
    const checked = input.checked;

    this.pageService.setPaymentDone(id, checked).subscribe({
      next: response => {
        const page = this.currentPage();
        if (!page) return;
        this.currentPage.set({
          ...page,
          paymentDones: page.paymentDones.map(p => (p.id === response.id ? response : p)),
        });
      },
      error: error => {
        console.log(error);
        input.checked = !input.checked;
      },
    });
  }

  public disabledEnter(event: Event): void {
    const input = event.target as HTMLInputElement;
    const id = Number(input.id);
    const disabled = !input.checked;

    this.pageService.setEnterDisabled(id, disabled).subscribe({
      next: response => {
        const page = this.currentPage();
        if (!page) return;
        this.currentPage.set({
          ...page,
          enters: page.enters.map(e =>
            e.id === response.id ? { ...e, isDisabled: response.isDisabled } : e,
          ),
        });
        if (page.id != null) {
          this.refreshPaymentDones(page.id);
        }
      },
      error: () => {
        input.checked = !input.checked;
      },
    });
  }

  private refreshPaymentDones(pageId: number): void {
    this.pageService.getPaymentDonesByPage(pageId).subscribe({
      next: paymentDones => {
        const page = this.currentPage();
        if (page) {
          this.currentPage.set({ ...page, paymentDones });
        }
      },
    });
  }

  private loadMonths(): void {
    this.pageService.getAllMonths().subscribe({
      next: result => {
        const dates = result
          .map(e => new Date(e))
          .sort((a, b) => b.getTime() - a.getTime());
        this.dateList.set(dates);

        if (dates.length > 0) {
          const last = dates.reduce((a, b) => (a > b ? a : b));
          this.lastDate.set(last);
          this.loadPage(last.getMonth(), last.getFullYear());
        }
      },
      error: console.error,
    });
  }

  private loadPage(monthZeroBased: number, year: number): void {
    this.pageService.getPage(monthZeroBased + 1, year).subscribe({
      next: result => {
        result.date = new Date(result.date);
        this.currentPage.set(result);
      },
      error: console.error,
    });
  }
}
