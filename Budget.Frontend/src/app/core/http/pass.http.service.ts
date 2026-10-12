import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpService } from './http.service';
import {
  PassGenerateResponse, SubmitPayment, PaymentBindingId, InitializationSettings,
  CategorySpendReport, FPayment, ImportResult
} from '../../shared/model/account.model';

@Injectable({ providedIn: 'root' })
export class PassHttpService extends HttpService {
  protected readonly urlBase = 'api/pass';
  constructor(http: HttpClient) { super(http); }

  generate(startUTC: string, endUTC: string) {
    return this.httpPost<PassGenerateResponse, { StartUTC: string; EndUTC: string }>(
      '/generate', { StartUTC: startUTC, EndUTC: endUTC }
    );
  }

  submitPayments(payments: SubmitPayment[], isOfflineSync = false) {
    return this.httpPost<PaymentBindingId[], { Payments: SubmitPayment[]; IsOfflineSync: boolean }>(
      '/submit/payment', { Payments: payments, IsOfflineSync: isOfflineSync }
    );
  }
}

@Injectable({ providedIn: 'root' })
export class InitializationHttpService extends HttpService {
  protected readonly urlBase = 'api/core/initialization';
  constructor(http: HttpClient) { super(http); }

  getSettings() { return this.httpGet<InitializationSettings>('/settings'); }
}

@Injectable({ providedIn: 'root' })
export class ReportHttpService extends HttpService {
  protected readonly urlBase = 'api/report';
  constructor(http: HttpClient) { super(http); }

  getCategorySpend(year: number, month: number) {
    return this.httpGet<CategorySpendReport[]>(`/category-spend?year=${year}&month=${month}`);
  }
}

@Injectable({ providedIn: 'root' })
export class OverdueHttpService extends HttpService {
  protected readonly urlBase = 'api/overdue';
  constructor(http: HttpClient) { super(http); }

  getOverdue(skip = 0, take = 20) {
    return this.httpGet<FPayment[]>(`/payments?skip=${skip}&take=${take}`);
  }
}

@Injectable({ providedIn: 'root' })
export class AuthHttpService extends HttpService {
  protected readonly urlBase = 'api/authentication';
  constructor(http: HttpClient) { super(http); }

  login(username: string, password: string) {
    return this.httpPost<{ Token: string }, { Username: string; Password: string }>(
      '/login', { Username: username, Password: password }
    );
  }
}

@Injectable({ providedIn: 'root' })
export class ImportHttpService extends HttpService {
  protected readonly urlBase = 'api/import';
  constructor(http: HttpClient) { super(http); }

  importBankStatement(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ImportResult>(this.urlBase + '/bank-statement', formData);
  }
}
