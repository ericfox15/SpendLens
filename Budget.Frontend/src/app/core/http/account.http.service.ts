import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpService } from './http.service';
import { FAccount } from '../../shared/model/account.model';

@Injectable({ providedIn: 'root' })
export class AccountHttpService extends HttpService {
  protected readonly urlBase = 'api/account';
  constructor(http: HttpClient) { super(http); }

  getAll() { return this.httpGet<FAccount[]>(); }
  getById(id: number) { return this.httpGet<FAccount>(`/${id}`); }
  create(account: Partial<FAccount>) { return this.httpPost<{ AccountId: number }, Partial<FAccount>>('', account); }
  update(id: number, account: Partial<FAccount>) { return this.httpPut(`/${id}`, account); }
  remove(id: number) { return this.httpDelete(`/${id}`); }
}
