import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

/**
 * Adds the Bearer JWT token to every API request.
 * The token is stored in localStorage under 'budget_token'.
 * Mirrors eMAR's security interceptor.
 */
@Injectable()
export class SecurityInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = localStorage.getItem('budget_token');
    if (token && !req.url.includes('config/config.json')) {
      return next.handle(req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      }));
    }
    return next.handle(req);
  }
}
