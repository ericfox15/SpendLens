import { Component, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface ImportedTransaction {
  Date: string;
  Description: string;
  Amount: number;
  Category: string;
  AutoCategorized: boolean;
}

interface CategorySpend {
  CategoryName: string;
  TotalSpent: number;
  Percentage: number;
}

interface ImportResult {
  Transactions: ImportedTransaction[];
  SpendByCategory: CategorySpend[];
  ImportedCount: number;
  SkippedCount: number;
}

@Component({
  selector: 'app-import',
  standalone: false,
  template: `
    <div class="container py-4">
      <h2 class="mb-1" style="color: var(--sr-purple-glow)">
        <i class="fas fa-file-import me-2"></i>Import Bank Statement
      </h2>
      <p class="mb-4" style="color: var(--sr-muted)">
        Upload a CSV file from your bank. Expected format: <code>Date,Description,Amount</code>.
        Negative amounts are expenses; positive (deposits) are skipped.
      </p>

      <div class="card mb-4">
        <div class="card-body">
          <div class="mb-3">
            <label class="form-label">Bank Statement CSV</label>
            <input type="file" class="form-control" accept=".csv" (change)="onFileSelected($event)" />
          </div>
          <button class="btn btn-primary" (click)="upload()" [disabled]="!selectedFile || loading">
            @if (loading) { <span class="spinner-border spinner-border-sm me-1"></span> }
            Import & Analyze
          </button>
          @if (selectedFile) {
            <span class="ms-3" style="color: var(--sr-muted)">{{ selectedFile.name }}</span>
          }
        </div>
      </div>

      @if (error) {
        <div class="alert alert-danger mb-4">{{ error }}</div>
      }

      @if (result) {
        <div class="alert alert-success mb-4">
          Imported <strong>{{ result.ImportedCount }}</strong> transactions.
          Skipped {{ result.SkippedCount }} rows (deposits, invalid lines).
        </div>

        <!-- Spending breakdown -->
        <div class="card mb-4">
          <div class="card-header d-flex justify-content-between">
            <span>Spending Breakdown</span>
            <span style="color: var(--sr-purple-glow)">Total: \${{ totalSpent | number:'1.2-2' }}</span>
          </div>
          <div class="card-body">
            @for (cat of result.SpendByCategory; track cat.CategoryName) {
              <div class="mb-3">
                <div class="d-flex justify-content-between mb-1">
                  <span style="font-weight: 600; color: var(--sr-purple-light, #c4b5fd)">{{ cat.CategoryName }}</span>
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

        <!-- Transaction table -->
        <div class="card">
          <div class="card-header">All Transactions ({{ result.Transactions.length }})</div>
          <div class="table-responsive">
            <table class="table table-sm table-hover mb-0">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Amount</th>
                  <th>Category</th>
                </tr>
              </thead>
              <tbody>
                @for (tx of result.Transactions; track $index) {
                  <tr>
                    <td style="color: var(--sr-muted)">{{ tx.Date }}</td>
                    <td>{{ tx.Description }}</td>
                    <td style="color: var(--sr-purple-glow); font-weight: 600">\${{ tx.Amount | number:'1.2-2' }}</td>
                    <td><span class="badge" style="background: rgba(139,92,246,0.2); color: var(--sr-purple-light, #c4b5fd)">{{ tx.Category }}</span></td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }
    </div>
  `
})
export class ImportComponent {
  selectedFile: File | null = null;
  result: ImportResult | null = null;
  loading = false;
  totalSpent = 0;
  error = '';

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
    this.result = null;
    this.error = '';
  }

  upload() {
    if (!this.selectedFile) return;
    this.loading = true;
    this.error = '';

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http.post<ImportResult>('api/import/bank-statement', formData).subscribe({
      next: result => {
        this.result = result;
        this.totalSpent = (result.SpendByCategory || []).reduce((sum, c) => sum + c.TotalSpent, 0);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Import failed. Please check your CSV format.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
