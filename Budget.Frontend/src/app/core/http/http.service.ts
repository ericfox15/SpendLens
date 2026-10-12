import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

/**
 * Abstract base class for all HTTP services. Mirrors eMAR's HttpService base.
 * Protected methods are prefixed with "http" to avoid JS keyword conflicts (delete, get, etc.)
 */
@Injectable()
export abstract class HttpService {
  protected abstract readonly urlBase: string;

  constructor(protected http: HttpClient) {}

  protected httpGet<T>(route = '', params?: HttpParams): Observable<T> {
    return this.http.get<T>(this.urlBase + route, { params });
  }

  protected httpPost<TRes, TBody>(route = '', body: TBody): Observable<TRes> {
    return this.http.post<TRes>(this.urlBase + route, body);
  }

  protected httpPut<TBody>(route = '', body: TBody): Observable<unknown> {
    return this.http.put(this.urlBase + route, body);
  }

  protected httpDelete(route = ''): Observable<unknown> {
    return this.http.delete(this.urlBase + route);
  }
}
