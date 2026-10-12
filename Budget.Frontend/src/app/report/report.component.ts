import { Component, OnInit } from '@angular/core';
import { CategorySpendReport } from '../shared/model/account.model';
import { ReportHttpService } from '../core/http/pass.http.service';

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
                  <span><strong>{{ row.CategoryName }}</strong></span>
                  <span>
                    \${{ row.TotalSpent | number:'1.2-2' }}
                    <span class="text-muted ms-2">({{ row.Percentage }}%)</span>
                    @if (row.MonthlyLimit) {
                      <span class="ms-2" [class.text-danger]="row.TotalSpent > row.MonthlyLimit"
                            [class.text-success]="row.TotalSpent <= row.MonthlyLimit">
                        / \${{ row.MonthlyLimit | number:'1.2-2' }} limit
                      </span>
                    }
                  </span>
                </div>
                <div class="progress" style="height: 20px">
                  <div class="progress-bar"
                       [class.bg-danger]="row.MonthlyLimit && row.TotalSpent > row.MonthlyLimit"
                       [class.bg-success]="!row.MonthlyLimit || row.TotalSpent <= row.MonthlyLimit"
                       [style.width]="row.Percentage + '%'">
                    {{ row.Percentage }}%
                  </div>
                </div>
              </div>
            }
          </div>
        </div>
      }

      @if (rows.length === 0 && loaded) {
        <div class="alert alert-info">No paid payments found for this period.</div>
      }
    </div>
  `
})
export class ReportComponent implements OnInit {
  year = new Date().getFullYear();
  month = new Date().getMonth() + 1;
  rows: CategorySpendReport[] = [];
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

  constructor(private reportHttp: ReportHttpService) {}

  ngOnInit() { this.load(); }

  load() {
    this.reportHttp.getCategorySpend(this.year, this.month).subscribe(rows => {
      this.rows = rows.filter(r => r.TotalSpent > 0);
      this.totalSpent = this.rows.reduce((sum, r) => sum + r.TotalSpent, 0);
      this.loaded = true;
    });
  }
}
