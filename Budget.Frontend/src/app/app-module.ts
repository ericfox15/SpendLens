import { NgModule, provideBrowserGlobalErrorListeners, provideAppInitializer, inject } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxsModule } from '@ngxs/store';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { DecimalPipe, DatePipe } from '@angular/common';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { ConfigService } from './shared/config/config.service';
import { BaseUrlInterceptor } from './core/interceptor/base-url.interceptor';
import { SecurityInterceptor } from './core/interceptor/security.interceptor';

// Feature components
import { LoginComponent } from './auth/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TransactionRangeComponent } from './transaction-range/transaction-range.component';
import { AccountComponent } from './account/account.component';
import { CategoryComponent } from './category/category.component';
import { ReportComponent } from './report/report.component';
import { ImportComponent } from './import/import.component';

/**
 * Root NgModule — mirrors eMAR's app.module.ts structure.
 * Registers all feature components, imports framework modules, sets up NGXS, interceptors.
 * provideAppInitializer loads config.json before any HTTP calls are made.
 */
@NgModule({
  declarations: [
    App,
    LoginComponent,
    DashboardComponent,
    TransactionRangeComponent,
    AccountComponent,
    CategoryComponent,
    ReportComponent,
    ImportComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    AppRoutingModule,
    NgbModule,
    NgxsModule.forRoot([], { developmentMode: true }),
    ToastrModule.forRoot({ positionClass: 'toast-bottom-right' })
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    // Load config.json before the app starts — mirrors eMAR's APP_INITIALIZER
    provideAppInitializer(() => inject(ConfigService).load()),
    // Interceptors run in order: base URL first, then auth token
    { provide: HTTP_INTERCEPTORS, useClass: BaseUrlInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: SecurityInterceptor, multi: true },
    DecimalPipe,
    DatePipe
  ],
  bootstrap: [App]
})
export class AppModule {}
