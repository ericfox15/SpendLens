import { Component, OnInit } from '@angular/core';
import { FAccount, EAccountType } from '../shared/model/account.model';
import { AccountHttpService } from '../core/http/account.http.service';

@Component({
  selector: 'app-account',
  standalone: false,
  template: `
    <div class="container py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2><i class="fas fa-university me-2"></i>Accounts</h2>
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
              <button class="btn btn-secondary" (click)="cancel()">Cancel</button>
            </div>
          </div>
        </div>
      }

      <div class="card">
        <div class="table-responsive">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr><th>Name</th><th>Type</th><th>Status</th><th>Actions</th></tr>
            </thead>
            <tbody>
              @for (account of accounts; track account.AccountId) {
                <tr>
                  <td><strong>{{ account.Name }}</strong></td>
                  <td>{{ getTypeName(account.AccountType) }}</td>
                  <td>
                    <span class="badge" [class.bg-success]="account.IsActive" [class.bg-secondary]="!account.IsActive">
                      {{ account.IsActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td>
                    <button class="btn btn-sm btn-outline-primary me-1" (click)="startEdit(account)">Edit</button>
                    <button class="btn btn-sm btn-outline-danger" (click)="delete(account.AccountId)">Delete</button>
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

  constructor(private accountHttp: AccountHttpService) {}

  ngOnInit() { this.load(); }

  load() { this.accountHttp.getAll().subscribe(a => this.accounts = a); }

  startCreate() { this.form = { AccountType: EAccountType.Checking, IsActive: true }; this.editing = true; }
  startEdit(account: FAccount) { this.form = { ...account }; this.editing = true; }
  cancel() { this.editing = false; this.form = {}; }

  save() {
    if (this.form.AccountId) {
      this.accountHttp.update(this.form.AccountId, this.form).subscribe(() => { this.load(); this.cancel(); });
    } else {
      this.accountHttp.create(this.form).subscribe(() => { this.load(); this.cancel(); });
    }
  }

  delete(id: number) {
    if (confirm('Deactivate this account?'))
      this.accountHttp.remove(id).subscribe(() => this.load());
  }

  getTypeName(type: EAccountType): string {
    return { [EAccountType.Checking]: 'Checking', [EAccountType.CreditCard]: 'Credit Card', [EAccountType.Cash]: 'Cash', [EAccountType.Savings]: 'Savings' }[type] ?? 'Unknown';
  }
}
