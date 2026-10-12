import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthHttpService } from '../core/http/pass.http.service';

@Component({
  selector: 'app-login',
  standalone: false,
  template: `
    <div class="container" style="padding-top: 10vh">
      <div class="row justify-content-center">
        <div class="col-md-4 col-sm-8">
          <div class="card login-card">
            <div class="card-body p-4">
              <h4 class="card-title mb-1" style="color: var(--sr-purple-glow); font-weight: 800">
                <i class="fas fa-chart-pie me-2"></i>SpendLens
              </h4>
              <p class="mb-4" style="color: var(--sr-muted); font-size: 0.85rem">Track your spending, your way.</p>
              @if (error) {
                <div class="alert alert-danger">{{ error }}</div>
              }
              <form (ngSubmit)="login()">
                <div class="mb-3">
                  <label class="form-label">Username</label>
                  <input class="form-control" [(ngModel)]="username" name="username" required autofocus />
                </div>
                <div class="mb-3">
                  <label class="form-label">Password</label>
                  <input class="form-control" type="password" [(ngModel)]="password" name="password" required />
                </div>
                <button class="btn btn-primary w-100" type="submit" [disabled]="loading">
                  @if (loading) { <span class="spinner-border spinner-border-sm me-1"></span> }
                  Sign In
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';
  loading = false;

  constructor(private authHttp: AuthHttpService, private router: Router) {}

  login() {
    this.loading = true;
    this.error = '';
    this.authHttp.login(this.username, this.password).subscribe({
      next: res => {
        localStorage.setItem('budget_token', res.Token);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.error = 'Invalid username or password.';
        this.loading = false;
      }
    });
  }
}
