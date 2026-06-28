import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpHelperService } from '@core/http/http-helper.service';
import { AppSettings } from '@core/config/app.constants';

/** Montants agrégés renvoyés par l'API, indexés par id (de tag ou d'utilisateur) ou par mois. */
export type AmountById = Record<number, number>;

/** Montant dépensé par un membre sur une catégorie (tag) — « qui dépense avec quel tag ». */
export interface UserTagExpense {
  userId: number;
  tagId: number;
  amount: number;
}

/** Données statistiques d'EasyCompta destinées aux diagrammes. */
@Injectable({
  providedIn: 'root',
})
export class StatisticsService {
  private readonly http = inject(HttpHelperService);

  /** Total des écritures par catégorie (tag) pour un mois donné. */
  public expensesByTagForMonth(month: number, year: number): Observable<AmountById> {
    return this.http.get<AmountById>(`${AppSettings.ENTER_URL}ExpensesByTagForAMonth/${month}/${year}`);
  }

  /** Total des lignes par membre pour une année donnée. */
  public expensesByUserForYear(year: number): Observable<AmountById> {
    return this.http.get<AmountById>(`${AppSettings.LINES_URL}ExpensesByUserForAYear/${year}`);
  }

  /** Total des écritures par catégorie (tag) pour une année donnée. */
  public expensesByTagForYear(year: number): Observable<AmountById> {
    return this.http.get<AmountById>(`${AppSettings.ENTER_URL}ExpensesByTagForAYear/${year}`);
  }

  /** Total des écritures par mois (1-12) pour une année — évolution annuelle. */
  public expensesByMonthForYear(year: number): Observable<AmountById> {
    return this.http.get<AmountById>(`${AppSettings.ENTER_URL}ExpensesByMonthForAYear/${year}`);
  }

  /** Montant dépensé par membre et par catégorie pour un mois donné. */
  public expensesByUserAndTagForMonth(month: number, year: number): Observable<UserTagExpense[]> {
    return this.http.get<UserTagExpense[]>(
      `${AppSettings.LINES_URL}ExpensesByUserAndTagForAMonth/${month}/${year}`,
    );
  }
}
