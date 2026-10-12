import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TransactionRangeComponent } from './transaction-range/transaction-range.component';
import { AccountComponent } from './account/account.component';
import { CategoryComponent } from './category/category.component';
import { ReportComponent } from './report/report.component';
import { ImportComponent } from './import/import.component';

const routes: Routes = [
  { path: 'login',              component: LoginComponent },
  { path: 'dashboard',          component: DashboardComponent },
  { path: 'transaction-range',  component: TransactionRangeComponent },
  { path: 'accounts',           component: AccountComponent },
  { path: 'categories',         component: CategoryComponent },
  { path: 'report',             component: ReportComponent },
  { path: 'import',             component: ImportComponent },
  { path: '',                   redirectTo: '/login', pathMatch: 'full' },
  { path: '**',                 redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
