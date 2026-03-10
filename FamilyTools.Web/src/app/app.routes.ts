import { Routes } from '@angular/router';
import { EasycomptaComponent } from './easycompta/easycompta.component';
import { UserComponent } from './user/user.component';
import { AccountEnterComponent } from './easycompta/account-enter/account-enter.component';
import { AccountPageComponent } from './easycompta/account-page/account-page.component';
import { AccountTagComponent } from './easycompta/account-tag/account-tag.component';

export const routes: Routes = [
    {
        path: 'easycompta',
        component: EasycomptaComponent,
        children: [
            {
                path: 'account-page',
                component: AccountPageComponent
            }, {
                path: 'account-enter/:id',
                component: AccountEnterComponent
            }, {
                path: 'account-tag',
                component: AccountTagComponent
            },
            {path: '', redirectTo:'account-page', pathMatch:'full'}
        ]
    },
    { path: 'user', component: UserComponent },
    {path: '', redirectTo: 'easycompta', pathMatch:'full'}
];
