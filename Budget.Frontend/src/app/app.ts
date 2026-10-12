import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: false,
  template: `
    @if (isLoggedIn) {
      <nav class="navbar navbar-expand-lg navbar-dark">
        <div class="container-fluid">
          <a class="navbar-brand" routerLink="/dashboard">
            <i class="fas fa-chart-pie me-2"></i>SpendLens
          </a>
          <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMenu">
            <span class="navbar-toggler-icon"></span>
          </button>
          <div class="collapse navbar-collapse" id="navMenu">
            <ul class="navbar-nav me-auto">
              <li class="nav-item"><a class="nav-link" routerLink="/dashboard" routerLinkActive="active">Dashboard</a></li>
              <li class="nav-item"><a class="nav-link" routerLink="/transaction-range" routerLinkActive="active">Transaction Range</a></li>
              <li class="nav-item"><a class="nav-link" routerLink="/import" routerLinkActive="active">Import CSV</a></li>
              <li class="nav-item"><a class="nav-link" routerLink="/report" routerLinkActive="active">Reports</a></li>
              <li class="nav-item dropdown">
                <a class="nav-link dropdown-toggle" data-bs-toggle="dropdown">Manage</a>
                <ul class="dropdown-menu">
                  <li><a class="dropdown-item" routerLink="/accounts">Accounts</a></li>
                  <li><a class="dropdown-item" routerLink="/categories">Categories</a></li>
                </ul>
              </li>
            </ul>
            <button class="btn btn-outline-light btn-sm" (click)="logout()">
              <i class="fas fa-sign-out-alt me-1"></i>Logout
            </button>
          </div>
        </div>
      </nav>
    }
    <router-outlet></router-outlet>
  `
})
export class App implements OnInit {
  isLoggedIn = false;

  constructor(private router: Router) {}

  ngOnInit() {
    this.isLoggedIn = !!localStorage.getItem('budget_token');
    this.router.events.subscribe(() => {
      this.isLoggedIn = !!localStorage.getItem('budget_token');
    });
  }

  logout() {
    localStorage.removeItem('budget_token');
    this.isLoggedIn = false;
    this.router.navigate(['/login']);
  }
}
