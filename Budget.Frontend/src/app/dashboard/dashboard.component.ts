import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface SpendSummary {
  CategoryId: number;
  CategoryName: string;
  TotalSpent: number;
  Percentage: number;
  MonthlyLimit?: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: false,
  template: `
    <div class="container-fluid py-4">
      <h2 class="mb-1" style="color: var(--sr-purple-glow)">
        <i class="fas fa-chart-pie me-2"></i>Dashboard
      </h2>
      <p class="mb-4" style="color: var(--sr-muted)">Spending summary for {{ monthLabel }}.</p>

      <div class="row g-4 mb-4">
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body py-4">
              <div style="color: var(--sr-muted); font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.5px">Total Spent</div>
              <div style="font-size: 2rem; font-weight: 800; color: var(--sr-purple-glow)">\${{ totalSpent | number:'1.2-2' }}</div>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body py-4">
              <div style="color: var(--sr-muted); font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.5px">Categories</div>
              <div style="font-size: 2rem; font-weight: 800; color: var(--sr-purple-light)">{{ rows.length }}</div>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body py-4">
              <div style="color: var(--sr-muted); font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.5px">Over Limit</div>
              <div style="font-size: 2rem; font-weight: 800" [style.color]="overLimitCount > 0 ? '#f87171' : 'var(--sr-purple-light)'">{{ overLimitCount }}</div>
            </div>
          </div>
        </div>
      </div>

      @if (rows.length > 0) {
        <div class="card mb-4">
          <div class="card-header">Spending by Category</div>
          <div class="card-body">
            @for (row of rows; track row.CategoryId) {
              <div class="mb-3">
                <div class="d-flex justify-content-between mb-1">
                  <span style="font-weight: 600">{{ row.CategoryName }}</span>
                  <span>
                    <span style="color: var(--sr-purple-glow)">\${{ row.TotalSpent | number:'1.2-2' }}</span>
                    <span style="color: var(--sr-muted)" class="ms-2">({{ row.Percentage }}%)</span>
                    @if (row.MonthlyLimit) {
                      <span class="ms-2" [style.color]="row.TotalSpent > row.MonthlyLimit ? '#f87171' : '#34d399'">
                        / \${{ row.MonthlyLimit | number:'1.2-2' }}
                      </span>
                    }
                  </span>
                </div>
                <div class="progress" style="height: 10px">
                  <div class="progress-bar"
                       [class.bg-danger]="row.MonthlyLimit && row.TotalSpent > row.MonthlyLimit"
                       [style.width]="row.Percentage + '%'">
                  </div>
                </div>
              </div>
            }
          </div>
        </div>

        <div class="text-center">
          <a routerLink="/transaction-range" class="btn btn-primary">
            <i class="fas fa-calendar-alt me-2"></i>Analyze a Custom Date Range
          </a>
        </div>
      }

      @if (rows.length === 0 && loaded) {
        <div class="text-center py-5" style="color: var(--sr-muted)">
          <i class="fas fa-inbox fa-3x mb-3" style="color: var(--sr-border)"></i>
          <p>No transactions found for this month. Try importing a bank statement.</p>
          <a routerLink="/import" class="btn btn-primary">
            <i class="fas fa-file-import me-2"></i>Import CSV
          </a>
        </div>
      }
    </div>
  `
})
export class DashboardComponent implements OnInit {
  rows: SpendSummary[] = [];
  totalSpent = 0;
  overLimitCount = 0;
  loaded = false;
  monthLabel = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    const now = new Date();
    this.monthLabel = now.toLocaleString('default', { month: 'long', year: 'numeric' });
    const year = now.getFullYear();
    const month = now.getMonth() + 1;
    this.http.get<SpendSummary[]>(`api/report/category-spend?year=${year}&month=${month}`).subscribe(data => {
      this.rows = data.filter(r => r.TotalSpent > 0);
      this.totalSpent = this.rows.reduce((s, r) => s + r.TotalSpent, 0);
      this.overLimitCount = this.rows.filter(r => r.MonthlyLimit && r.TotalSpent > r.MonthlyLimit).length;
      this.loaded = true;
    });
  }
}
