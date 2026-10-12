import { Component, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface CategoryBreakdown {
  CategoryId: number;
  CategoryName: string;
  TotalSpent: number;
  Percentage: number;
  MonthlyLimit?: number;
}

interface TransactionRow {
  PaymentId: number;
  Merchant: string;
  Amount: number;
  Date: string;
  CategoryName: string;
  AccountName: string;
}

interface RangeResult {
  Categories: CategoryBreakdown[];
  Transactions: TransactionRow[];
  TotalSpent: number;
}

@Component({
  selector: 'app-transaction-range',
  standalone: false,
  template: `
    <div class="container-fluid py-4">
      <h2 class="mb-1" style="color: var(--sr-purple-glow)">
        <i class="fas fa-calendar-alt me-2"></i>Transaction Range
      </h2>
      <p class="mb-4" style="color: var(--sr-muted)">Pick a date range to see how your spending breaks down by category.</p>

      <div class="card mb-4">
        <div class="card-body d-flex align-items-end gap-3 flex-wrap">
          <div>
            <label class="form-label">From</label>
            <input type="date" class="form-control" [(ngModel)]="startDate" />
          </div>
          <div>
            <label class="form-label">To</label>
            <input type="date" class="form-control" [(ngModel)]="endDate" />
          </div>
          <div>
            <label class="form-label">Account</label>
            <select class="form-select" [(ngModel)]="accountId">
              <option [value]="0">All Accounts</option>
              @for (a of accounts; track a.AccountId) {
                <option [value]="a.AccountId">{{ a.Name }}</option>
              }
            </select>
          </div>
          <button class="btn btn-primary" (click)="load()" [disabled]="loading">
            @if (loading) { <span class="spinner-border spinner-border-sm me-1"></span> }
            <i class="fas fa-search me-1"></i>Analyze
          </button>
        </div>
      </div>

      @if (result) {
        <!-- Summary bar -->
        <div class="card mb-4">
          <div class="card-body d-flex align-items-center gap-4">
            <div>
              <div style="color: var(--sr-muted); font-size: 0.8rem;">TOTAL SPENT</div>
              <div style="font-size: 1.8rem; font-weight: 800; color: var(--sr-purple-glow)">\${{ result.TotalSpent | number:'1.2-2' }}</div>
            </div>
            <div>
              <div style="color: var(--sr-muted); font-size: 0.8rem;">TRANSACTIONS</div>
              <div style="font-size: 1.8rem; font-weight: 800; color: var(--sr-purple-light)">{{ result.Transactions.length }}</div>
            </div>
            <div>
              <div style="color: var(--sr-muted); font-size: 0.8rem;">CATEGORIES</div>
              <div style="font-size: 1.8rem; font-weight: 800; color: var(--sr-purple-light)">{{ result.Categories.length }}</div>
            </div>
          </div>
        </div>

        <!-- Category breakdown -->
        <div class="card mb-4">
          <div class="card-header">Spending by Category</div>
          <div class="card-body">
            @for (cat of result.Categories; track cat.CategoryId) {
              <div class="mb-3">
                <div class="d-flex justify-content-between mb-1">
                  <span style="font-weight: 600">{{ cat.CategoryName }}</span>
                  <span>
                    <span style="color: var(--sr-purple-glow)">\${{ cat.TotalSpent | number:'1.2-2' }}</span>
                    <span style="color: var(--sr-muted)" class="ms-2">({{ cat.Percentage }}%)</span>
                  </span>
                </div>
                <div class="progress" style="height: 10px">
                  <div class="progress-bar" [style.width]="cat.Percentage + '%'"></div>
                </div>
              </div>
            }
          </div>
        </div>

        <!-- Transaction list -->
        <div class="card">
          <div class="card-header">All Transactions ({{ result.Transactions.length }})</div>
          <div class="table-responsive">
            <table class="table table-hover mb-0">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Merchant</th>
                  <th>Amount</th>
                  <th>Category</th>
                  <th>Account</th>
                </tr>
              </thead>
              <tbody>
                @for (tx of result.Transactions; track tx.PaymentId) {
                  <tr>
                    <td style="color: var(--sr-muted)">{{ tx.Date | date:'mediumDate' }}</td>
                    <td>{{ tx.Merchant }}</td>
                    <td style="color: var(--sr-purple-glow); font-weight: 600">\${{ tx.Amount | number:'1.2-2' }}</td>
                    <td><span class="badge bg-light">{{ tx.CategoryName }}</span></td>
                    <td style="color: var(--sr-muted)">{{ tx.AccountName }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }

      @if (!result && !loading) {
        <div class="text-center py-5" style="color: var(--sr-muted)">
          <i class="fas fa-chart-pie fa-3x mb-3" style="color: var(--sr-border)"></i>
          <p>Select a date range and click Analyze to see your spending breakdown.</p>
        </div>
      }
    </div>
  `
})
export class TransactionRangeComponent {
  startDate = this.firstOfMonth();
  endDate = this.today();
  accountId = 0;
  accounts: { AccountId: number; Name: string }[] = [];
  result: RangeResult | null = null;
  loading = false;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {
    this.http.get<{ AccountId: number; Name: string }[]>('api/account').subscribe({
      next: a => { this.accounts = a; this.cdr.detectChanges(); }
    });
  }

  load() {
    this.loading = true;
    const start = new Date(this.startDate).toISOString();
    const end = new Date(this.endDate + 'T23:59:59').toISOString();
    this.http.get<RangeResult>(`api/transaction-range?start=${start}&end=${end}&accountId=${this.accountId}`)
      .subscribe({
        next: r => { this.result = r; this.loading = false; this.cdr.detectChanges(); },
        error: () => { this.loading = false; this.cdr.detectChanges(); }
      });
  }

  private firstOfMonth(): string {
    const d = new Date(); d.setDate(1);
    return d.toISOString().split('T')[0];
  }

  private today(): string {
    return new Date().toISOString().split('T')[0];
  }
}
