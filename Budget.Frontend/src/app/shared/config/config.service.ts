import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export interface AppConfig {
  apiBaseUrl: string;
  authenticationMode: string;
}

/**
 * Loads config/config.json at startup. Exposes a `ready` promise that resolves when loaded.
 * The BaseUrlInterceptor waits on this before rewriting request URLs.
 * Mirrors eMAR's ConfigService exactly.
 */
@Injectable({ providedIn: 'root' })
export class ConfigService {
  config: AppConfig = { apiBaseUrl: '', authenticationMode: 'jwt' };
  ready: Promise<void>;
  private resolve!: () => void;

  constructor(private http: HttpClient) {
    this.ready = new Promise(res => (this.resolve = res));
  }

  load(): Promise<void> {
    return this.http
      .get<AppConfig>('config/config.json')
      .toPromise()
      .then(cfg => {
        if (cfg) this.config = cfg;
        this.resolve();
      })
      .catch(() => this.resolve());
  }
}
