import { Routes } from '@angular/router';
import { EasycomptaComponent } from './easycompta.component';
import { accountEnterResolver } from './resolvers/account-enter.resolver';

export const easycomptaRoutes: Routes = [
  {
    path: '',
    component: EasycomptaComponent,
    children: [
      {
        path: 'account-page',
        loadComponent: () =>
          import('./account-page/account-page.component').then(m => m.AccountPageComponent),
      },
      {
        path: 'account-enter',
        loadComponent: () =>
          import('./account-enter/account-enter.component').then(m => m.AccountEnterComponent),
      },
      {
        path: 'account-enter/:id',
        resolve: { enter: accountEnterResolver },
        loadComponent: () =>
          import('./account-enter/account-enter.component').then(m => m.AccountEnterComponent),
      },
      {
        path: 'account-tag',
        loadComponent: () =>
          import('./account-tag/account-tag.component').then(m => m.AccountTagComponent),
      },
      {
        path: 'account-chart-month',
        loadComponent: () =>
          import('./account-chart-month/account-chart-month.component').then(
            m => m.AccountChartMonthComponent,
          ),
      },
      {
        path: 'account-chart-year',
        loadComponent: () =>
          import('./account-chart-year/account-chart-year.component').then(
            m => m.AccountChartYearComponent,
          ),
      },
      { path: 'account-chart', redirectTo: 'account-chart-month', pathMatch: 'full' },
      { path: '', redirectTo: 'account-page', pathMatch: 'full' },
    ],
  },
];
