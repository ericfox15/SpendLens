import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface FAccount {
  AccountId: number;
  Name: string;
  AccountType: number;
  IsActive: boolean;
}

@Component({
  selector: 'app-account',
  standalone: false,
  template: `
    <div class="container py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 style="color: var(--sr-purple-glow)"><i class="fas fa-university me-2"></i>Accounts</h2>
        <button class="btn btn-primary" (click)="startCreate()">
          <i class="fas fa-plus me-1"></i>Add Account
        </button>
      </div>

      @if (editing) {
        <div class="card mb-4">
          <div class="card-header">{{ form.AccountId ? 'Edit' : 'New' }} Account</div>
          <div class="card-body">
            <div class="row g-3">
              <div class="col-md-6">
                <label class="form-label">Name</label>
                <input class="form-control" [(ngModel)]="form.Name" required />
              </div>
              <div class="col-md-6">
                <label class="form-label">Type</label>
                <select class="form-select" [(ngModel)]="form.AccountType">
                  <option [value]="1">Checking</option>
                  <option [value]="2">Credit Card</option>
                  <option [value]="3">Cash</option>
                  <option [value]="4">Savings</option>
                </select>
              </div>
            </div>
            <div class="mt-3">
              <button class="btn btn-success me-2" (click)="save()">Save</button>
              <button class="btn btn-outline-secondary" (click)="cancel()">Cancel</button>
            </div>
          </div>
        </div>
      }

      <div class="card">
        <div class="table-responsive">
          <table class="table table-hover mb-0">
            <thead>
              <tr><th>Name</th><th>Type</th><th>Status</th><th>Actions</th></tr>
            </thead>
            <tbody>
              @for (account of accounts; track account.AccountId) {
                <tr>
                  <td style="font-weight: 600">{{ account.Name }}</td>
                  <td>{{ getTypeName(account.AccountType) }}</td>
                  <td>
                    <span class="badge" [class.bg-success]="account.IsActive" [class.bg-secondary]="!account.IsActive">
                      {{ account.IsActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td>
                    <button class="btn btn-sm btn-outline-secondary me-1" (click)="startEdit(account)">Edit</button>
                    <button class="btn btn-sm btn-outline-secondary" (click)="deactivate(account.AccountId)">Deactivate</button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `
})
export class AccountComponent implements OnInit {
  accounts: FAccount[] = [];
  editing = false;
  form: Partial<FAccount> = {};

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() { this.load(); }

  load() {
    this.http.get<FAccount[]>('api/account').subscribe({
      next: a => { this.accounts = a; this.cdr.detectChanges(); }
    });
  }

  startCreate() { this.form = { AccountType: 1, IsActive: true }; this.editing = true; }
  startEdit(account: FAccount) { this.form = { ...account }; this.editing = true; }
  cancel() { this.editing = false; this.form = {}; }

  save() {
    if (this.form.AccountId) {
      this.http.put(`api/account/${this.form.AccountId}`, this.form).subscribe(() => { this.load(); this.cancel(); });
    } else {
      this.http.post('api/account', this.form).subscribe(() => { this.load(); this.cancel(); });
    }
  }

  deactivate(id: number) {
    if (confirm('Deactivate this account?'))
      this.http.delete(`api/account/${id}`).subscribe(() => this.load());
  }

  getTypeName(type: number): string {
    return { 1: 'Checking', 2: 'Credit Card', 3: 'Cash', 4: 'Savings' }[type] ?? 'Unknown';
  }
}
