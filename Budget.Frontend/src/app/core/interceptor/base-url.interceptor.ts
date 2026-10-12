import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ConfigService } from '../../shared/config/config.service';

/**
 * Waits for ConfigService to load, then prepends the API base URL to all relative requests.
 * If apiBaseUrl is empty (production: Angular served from wwwroot), requests go to same origin.
 * Mirrors eMAR's BaseUrlInterceptor.
 */
@Injectable()
export class BaseUrlInterceptor implements HttpInterceptor {
  constructor(private configService: ConfigService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (req.url.startsWith('config/')) {
      return next.handle(req);
    }
    return from(this.configService.ready).pipe(
      switchMap(() => {
        const baseUrl = this.configService.config.apiBaseUrl;
        if (baseUrl && !req.url.startsWith('http')) {
          const url = new URL(req.url, baseUrl).toString();
          return next.handle(req.clone({ url }));
        }
        return next.handle(req);
      })
    );
  }
}
