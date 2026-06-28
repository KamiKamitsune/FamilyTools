import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  OnInit,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Chart, registerables, TooltipItem } from 'chart.js';
import { AccountPageService } from '@easycompta/data/account-page.service';
import { AccountTagService } from '@easycompta/data/account-tag.service';
import { StatisticsService, AmountById } from '@easycompta/data/statistics.service';
import { UserService } from '@user/data/user.service';

Chart.register(...registerables);

const MONTH_LABELS = ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Juin', 'Juil', 'Août', 'Sep', 'Oct', 'Nov', 'Déc'];

/**
 * Page « Diagrammes de l'année » : dépenses de l'année sélectionnée par catégorie
 * (donut, avec part en %), par membre (barres) et évolution mensuelle sur l'année
 * (courbe). Rendu via chart.js piloté à la main.
 */
@Component({
  selector: 'app-account-chart-year',
  imports: [RouterLink],
  templateUrl: './account-chart-year.component.html',
  styleUrl: './account-chart-year.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountChartYearComponent implements OnInit {
  private readonly pageService = inject(AccountPageService);
  private readonly statsService = inject(StatisticsService);
  readonly tagService = inject(AccountTagService);
  readonly userService = inject(UserService);
  private readonly destroyRef = inject(DestroyRef);

  readonly yearList = signal<number[]>([]);
  readonly selectedYear = signal<number | undefined>(undefined);

  private readonly tagData = signal<AmountById>({});
  private readonly userData = signal<AmountById>({});
  private readonly monthlyData = signal<AmountById>({});

  private readonly tagCanvas = viewChild<ElementRef<HTMLCanvasElement>>('tagCanvas');
  private readonly userCanvas = viewChild<ElementRef<HTMLCanvasElement>>('userCanvas');
  private readonly monthCanvas = viewChild<ElementRef<HTMLCanvasElement>>('monthCanvas');

  private tagChart?: Chart;
  private userChart?: Chart;
  private monthChart?: Chart;

  constructor() {
    // Chaque graphe se (re)rend dès que son canvas existe et que ses données / libellés changent.
    effect(() => {
      this.tagCanvas();
      this.tagData();
      this.tagService.allTag();
      this.renderTagChart();
    });

    effect(() => {
      this.userCanvas();
      this.userData();
      this.userService.users();
      this.renderUserChart();
    });

    effect(() => {
      this.monthCanvas();
      this.monthlyData();
      this.renderMonthChart();
    });

    this.destroyRef.onDestroy(() => {
      this.tagChart?.destroy();
      this.userChart?.destroy();
      this.monthChart?.destroy();
    });
  }

  ngOnInit(): void {
    this.pageService.getAllMonths().subscribe({
      next: result => {
        // Années distinctes déduites des mois disponibles, triées du plus récent au plus ancien.
        const years = [...new Set(result.map(date => new Date(date).getFullYear()))].sort((a, b) => b - a);
        this.yearList.set(years);

        if (years.length > 0) {
          this.selectedYear.set(years[0]);
          this.loadData(years[0]);
        }
      },
      error: console.error,
    });
  }

  public changeYear(event: Event): void {
    const year = Number((event.target as HTMLSelectElement).value);
    this.selectedYear.set(year);
    this.loadData(year);
  }

  private loadData(year: number): void {
    this.statsService.expensesByTagForYear(year).subscribe({
      next: data => this.tagData.set(data),
      error: console.error,
    });
    this.statsService.expensesByUserForYear(year).subscribe({
      next: data => this.userData.set(data),
      error: console.error,
    });
    this.statsService.expensesByMonthForYear(year).subscribe({
      next: data => this.monthlyData.set(data),
      error: console.error,
    });
  }

  private renderTagChart(): void {
    const canvas = this.tagCanvas()?.nativeElement;
    if (!canvas) return;

    const tagById = new Map(this.tagService.allTag().map(tag => [tag.id, tag]));
    const entries = Object.entries(this.tagData())
      .map(([id, amount]) => ({ id: Number(id), amount: Math.abs(amount) }))
      .filter(entry => entry.amount > 0);

    this.tagChart?.destroy();
    this.tagChart = new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: entries.map(entry => tagById.get(entry.id)?.name ?? `#${entry.id}`),
        datasets: [
          {
            data: entries.map(entry => entry.amount),
            backgroundColor: entries.map(entry => tagById.get(entry.id)?.color ?? '#999999'),
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'right' },
          tooltip: {
            callbacks: {
              // Part en % du total : bornée à 100, donc aucune tranche ne peut le dépasser.
              label: (ctx: TooltipItem<'doughnut'>) => {
                const value = ctx.parsed;
                const total = (ctx.dataset.data as number[]).reduce((sum, v) => sum + v, 0);
                const pct = total > 0 ? Math.min(100, (value / total) * 100) : 0;
                return `${ctx.label}: ${value.toFixed(2)} € (${pct.toFixed(1)} %)`;
              },
            },
          },
        },
      },
    });
  }

  private renderUserChart(): void {
    const canvas = this.userCanvas()?.nativeElement;
    if (!canvas) return;

    const userById = new Map(this.userService.users().map(user => [user.id, user]));
    const entries = Object.entries(this.userData())
      .map(([id, amount]) => ({ id: Number(id), amount: Math.abs(amount) }))
      .filter(entry => entry.amount > 0);

    this.userChart?.destroy();
    this.userChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: entries.map(entry => userById.get(entry.id)?.userName ?? `#${entry.id}`),
        datasets: [
          {
            label: 'Dépenses (€)',
            data: entries.map(entry => entry.amount),
            backgroundColor: '#4f8cff',
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true } },
      },
    });
  }

  private renderMonthChart(): void {
    const canvas = this.monthCanvas()?.nativeElement;
    if (!canvas) return;

    const data = this.monthlyData();
    const values = MONTH_LABELS.map((_, index) => Math.abs(data[index + 1] ?? 0));

    this.monthChart?.destroy();
    this.monthChart = new Chart(canvas, {
      type: 'line',
      data: {
        labels: MONTH_LABELS,
        datasets: [
          {
            label: 'Dépenses (€)',
            data: values,
            borderColor: '#4f8cff',
            backgroundColor: 'rgba(79, 140, 255, 0.2)',
            fill: true,
            tension: 0.3,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true } },
      },
    });
  }
}
