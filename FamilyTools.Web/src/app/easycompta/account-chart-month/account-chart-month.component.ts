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
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Chart, registerables, TooltipItem } from 'chart.js';
import { AccountPageService } from '@easycompta/data/account-page.service';
import { AccountTagService } from '@easycompta/data/account-tag.service';
import { StatisticsService, AmountById, UserTagExpense } from '@easycompta/data/statistics.service';
import { UserService } from '@user/data/user.service';

Chart.register(...registerables);

/** Palette cyclique pour distinguer les membres dans les graphes empilés. */
const USER_COLORS = ['#4f8cff', '#4CAF50', '#FF9800', '#E91E63', '#9C27B0', '#00BCD4', '#FFC107', '#795548'];

/**
 * Page « Diagrammes du mois » : dépenses du mois sélectionné par catégorie
 * (donut, avec part en %) et répartition membre × catégorie (barres empilées).
 * Rendu via chart.js piloté à la main.
 */
@Component({
  selector: 'app-account-chart-month',
  imports: [DatePipe, RouterLink],
  templateUrl: './account-chart-month.component.html',
  styleUrl: './account-chart-month.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountChartMonthComponent implements OnInit {
  private readonly pageService = inject(AccountPageService);
  private readonly statsService = inject(StatisticsService);
  readonly tagService = inject(AccountTagService);
  readonly userService = inject(UserService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dateList = signal<Date[]>([]);
  readonly selectedDate = signal<Date | undefined>(undefined);

  private readonly tagData = signal<AmountById>({});
  private readonly userTagData = signal<UserTagExpense[]>([]);

  private readonly tagCanvas = viewChild<ElementRef<HTMLCanvasElement>>('tagCanvas');
  private readonly userTagCanvas = viewChild<ElementRef<HTMLCanvasElement>>('userTagCanvas');

  private tagChart?: Chart;
  private userTagChart?: Chart;

  constructor() {
    // Chaque graphe se (re)rend dès que son canvas existe et que ses données / libellés changent.
    effect(() => {
      this.tagCanvas();
      this.tagData();
      this.tagService.allTag();
      this.renderTagChart();
    });

    effect(() => {
      this.userTagCanvas();
      this.userTagData();
      this.tagService.allTag();
      this.userService.users();
      this.renderUserTagChart();
    });

    this.destroyRef.onDestroy(() => {
      this.tagChart?.destroy();
      this.userTagChart?.destroy();
    });
  }

  ngOnInit(): void {
    this.pageService.getAllMonths().subscribe({
      next: result => {
        const dates = result.map(date => new Date(date)).sort((a, b) => b.getTime() - a.getTime());
        this.dateList.set(dates);

        if (dates.length > 0) {
          this.selectedDate.set(dates[0]);
          this.loadData(dates[0]);
        }
      },
      error: console.error,
    });
  }

  public changeMonth(event: Event): void {
    const date = new Date((event.target as HTMLSelectElement).value);
    this.selectedDate.set(date);
    this.loadData(date);
  }

  private loadData(date: Date): void {
    const month = date.getMonth() + 1;
    const year = date.getFullYear();

    this.statsService.expensesByTagForMonth(month, year).subscribe({
      next: data => this.tagData.set(data),
      error: console.error,
    });
    this.statsService.expensesByUserAndTagForMonth(month, year).subscribe({
      next: data => this.userTagData.set(data),
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

  private renderUserTagChart(): void {
    const canvas = this.userTagCanvas()?.nativeElement;
    if (!canvas) return;

    const rows = this.userTagData();
    const tagById = new Map(this.tagService.allTag().map(tag => [tag.id, tag]));
    const userById = new Map(this.userService.users().map(user => [user.id, user]));

    const tagIds = [...new Set(rows.map(row => row.tagId))];
    const userIds = [...new Set(rows.map(row => row.userId))];
    const amountOf = (userId: number, tagId: number) =>
      Math.abs(rows.find(row => row.userId === userId && row.tagId === tagId)?.amount ?? 0);

    const datasets = userIds.map((userId, index) => ({
      label: userById.get(userId)?.userName ?? `#${userId}`,
      data: tagIds.map(tagId => amountOf(userId, tagId)),
      backgroundColor: USER_COLORS[index % USER_COLORS.length],
    }));

    this.userTagChart?.destroy();
    this.userTagChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: tagIds.map(tagId => tagById.get(tagId)?.name ?? `#${tagId}`),
        datasets,
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { position: 'top' } },
        scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true } },
      },
    });
  }
}
