import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface CategorySpendRow {
  CategoryId: number;
  CategoryName: string;
  TotalSpent: number;
  Percentage: number;
  MonthlyLimit?: number;
}

@Component({
  selector: 'app-report',
  standalone: false,
  template: `
    <div class="container py-4">
      <h2 class="mb-1" style="color: var(--sr-purple-glow)"><i class="fas fa-chart-pie me-2"></i>Monthly Report</h2>
      <p class="mb-4" style="color: var(--sr-muted)">Category breakdown for a given month.</p>

      <div class="card mb-4">
        <div class="card-body d-flex align-items-center gap-3">
          <div>
            <label class="form-label mb-0 small">Year</label>
            <input type="number" class="form-control" [(ngModel)]="year" style="width:100px" />
          </div>
          <div>
            <label class="form-label mb-0 small">Month</label>
            <select class="form-select" [(ngModel)]="month" style="width:140px">
              @for (m of months; track m.value) {
                <option [value]="m.value">{{ m.label }}</option>
              }
            </select>
          </div>
          <button class="btn btn-primary mt-3" (click)="load()">Load Report</button>
        </div>
      </div>

      @if (rows.length > 0) {
        <div class="card">
          <div class="card-header d-flex justify-content-between">
            <span>Spending by Category</span>
            <span style="color: var(--sr-purple-glow)">Total: \${{ totalSpent | number:'1.2-2' }}</span>
          </div>
          <div class="card-body">
            @for (row of rows; track row.CategoryId) {
              <div class="mb-3">
                <div class="d-flex justify-content-between mb-1">
                  <span style="font-weight: 600; color: var(--sr-purple-light)">{{ row.CategoryName }}</span>
                  <span>
                    <span style="color: var(--sr-purple-glow)">\${{ row.TotalSpent | number:'1.2-2' }}</span>
                    <span style="color: var(--sr-muted)" class="ms-2">({{ row.Percentage }}%)</span>
                    @if (row.MonthlyLimit) {
                      <span class="ms-2" [style.color]="row.TotalSpent > row.MonthlyLimit ? '#f87171' : '#34d399'">
                        / \${{ row.MonthlyLimit | number:'1.2-2' }} limit
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
      }

      @if (rows.length === 0 && loaded) {
        <div class="text-center py-5" style="color: var(--sr-muted)">
          <i class="fas fa-inbox fa-3x mb-3" style="color: var(--sr-border)"></i>
          <p>No transactions found for this period.</p>
        </div>
      }
    </div>
  `
})
export class ReportComponent implements OnInit {
  year = new Date().getFullYear();
  month = new Date().getMonth() + 1;
  rows: CategorySpendRow[] = [];
  totalSpent = 0;
  loaded = false;

  months = [
    { value: 1, label: 'January' }, { value: 2, label: 'February' },
    { value: 3, label: 'March' }, { value: 4, label: 'April' },
    { value: 5, label: 'May' }, { value: 6, label: 'June' },
    { value: 7, label: 'July' }, { value: 8, label: 'August' },
    { value: 9, label: 'September' }, { value: 10, label: 'October' },
    { value: 11, label: 'November' }, { value: 12, label: 'December' }
  ];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() { this.load(); }

  load() {
    this.http.get<CategorySpendRow[]>(`api/report/category-spend?year=${this.year}&month=${this.month}`).subscribe({
      next: rows => {
        this.rows = rows.filter(r => r.TotalSpent > 0);
        this.totalSpent = this.rows.reduce((sum, r) => sum + r.TotalSpent, 0);
        this.loaded = true;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loaded = true;
        this.cdr.detectChanges();
      }
    });
  }
}
