import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpService } from './http.service';
import { FCategory } from '../../shared/model/account.model';

@Injectable({ providedIn: 'root' })
export class CategoryHttpService extends HttpService {
  protected readonly urlBase = 'api/category';
  constructor(http: HttpClient) { super(http); }

  getAll() { return this.httpGet<FCategory[]>(); }
  getById(id: number) { return this.httpGet<FCategory>(`/${id}`); }
  create(category: Partial<FCategory>) { return this.httpPost<{ CategoryId: number }, Partial<FCategory>>('', category); }
  update(id: number, category: Partial<FCategory>) { return this.httpPut(`/${id}`, category); }
  remove(id: number) { return this.httpDelete(`/${id}`); }
}
