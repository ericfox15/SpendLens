import { Component } from '@angular/core';
import { ImportResult } from '../shared/model/account.model';
import { ImportHttpService } from '../core/http/pass.http.service';

@Component({
  selector: 'app-import',
  standalone: false,
  template: `
    <div class="container py-4">
      <h2 class="mb-2"><i class="fas fa-file-import me-2"></i>Import Bank Statement</h2>
      <p class="text-muted mb-4">
        Upload a CSV file from your bank. Expected format: <code>Date,Description,Amount</code>.
        Most banks (Chase, Bank of America, Wells Fargo) export this format.
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
            <span class="ms-3 text-muted">{{ selectedFile.name }}</span>
          }
        </div>
      </div>

      @if (result) {
        <div class="alert alert-success mb-4">
          ✅ Imported <strong>{{ result.ImportedCount }}</strong> transactions.
          Skipped {{ result.SkippedCount }} rows (deposits, invalid lines).
        </div>

        <!-- Spending breakdown -->
        <div class="card mb-4">
          <div class="card-header">
            <strong>Spending Breakdown</strong>
            <span class="text-muted ms-2">Total: \${{ totalSpent | number:'1.2-2' }}</span>
          </div>
          <div class="card-body">
            @for (cat of result.SpendByCategory; track cat.CategoryName) {
              <div class="mb-3">
                <div class="d-flex justify-content-between mb-1">
                  <span><strong>{{ cat.CategoryName }}</strong></span>
                  <span>\${{ cat.TotalSpent | number:'1.2-2' }} <span class="text-primary">({{ cat.Percentage }}%)</span></span>
                </div>
                <div class="progress" style="height:18px">
                  <div class="progress-bar bg-primary" [style.width]="cat.Percentage + '%'">
                    {{ cat.Percentage }}%
                  </div>
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
              <thead class="table-light">
                <tr>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Amount</th>
                  <th>Category</th>
                  <th>Auto?</th>
                </tr>
              </thead>
              <tbody>
                @for (tx of result.Transactions; track $index) {
                  <tr>
                    <td>{{ tx.Date }}</td>
                    <td>{{ tx.Description }}</td>
                    <td>\${{ tx.Amount | number:'1.2-2' }}</td>
                    <td><span class="badge bg-light text-dark">{{ tx.Category }}</span></td>
                    <td>
                      @if (tx.AutoCategorized) {
                        <i class="fas fa-robot text-success" title="Auto-categorized"></i>
                      } @else {
                        <i class="fas fa-question text-warning" title="Uncategorized"></i>
                      }
                    </td>
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

  constructor(private importHttp: ImportHttpService) {}

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
    this.result = null;
  }

  upload() {
    if (!this.selectedFile) return;
    this.loading = true;
    this.importHttp.importBankStatement(this.selectedFile).subscribe({
      next: result => {
        console.log('Import response:', result);
        this.result = result;
        this.totalSpent = (result.SpendByCategory || []).reduce((sum: number, c: any) => sum + c.TotalSpent, 0);
        this.loading = false;
      },
      error: err => {
        console.error('Import error:', err);
        alert('Import failed. Please check your CSV format.');
        this.loading = false;
      }
    });
  }
}
