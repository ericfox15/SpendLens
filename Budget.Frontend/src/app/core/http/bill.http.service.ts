import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpService } from './http.service';
import { FBill } from '../../shared/model/account.model';

@Injectable({ providedIn: 'root' })
export class BillHttpService extends HttpService {
  protected readonly urlBase = 'api/bill';
  constructor(http: HttpClient) { super(http); }

  getAll() { return this.httpGet<FBill[]>(); }
  getById(id: number) { return this.httpGet<FBill>(`/${id}`); }
  create(bill: Partial<FBill>) { return this.httpPost<{ BillId: number }, Partial<FBill>>('', bill); }
  update(id: number, bill: Partial<FBill>) { return this.httpPut(`/${id}`, bill); }
  remove(id: number) { return this.httpDelete(`/${id}`); }
}
