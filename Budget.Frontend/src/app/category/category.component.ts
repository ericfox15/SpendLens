import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface FCategory {
  CategoryId: number;
  Name: string;
  MonthlyLimit?: number;
}

@Component({
  selector: 'app-category',
  standalone: false,
  template: `
    <div class="container py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 style="color: var(--sr-purple-glow)"><i class="fas fa-tags me-2"></i>Categories</h2>
        <button class="btn btn-primary" (click)="startCreate()">
          <i class="fas fa-plus me-1"></i>Add Category
        </button>
      </div>

      @if (editing) {
        <div class="card mb-4">
          <div class="card-header">{{ form.CategoryId ? 'Edit' : 'New' }} Category</div>
          <div class="card-body">
            <div class="row g-3">
              <div class="col-md-6">
                <label class="form-label">Name</label>
                <input class="form-control" [(ngModel)]="form.Name" />
              </div>
              <div class="col-md-6">
                <label class="form-label">Monthly Limit (optional)</label>
                <div class="input-group">
                  <span class="input-group-text">\$</span>
                  <input class="form-control" type="number" [(ngModel)]="form.MonthlyLimit" placeholder="No limit" />
                </div>
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
              <tr><th>Name</th><th>Monthly Limit</th><th>Actions</th></tr>
            </thead>
            <tbody>
              @for (cat of categories; track cat.CategoryId) {
                <tr>
                  <td style="font-weight: 600">{{ cat.Name }}</td>
                  <td>
                    @if (cat.MonthlyLimit) {
                      <span style="color: var(--sr-purple-glow)">\${{ cat.MonthlyLimit | number:'1.2-2' }}</span>
                    } @else {
                      <span style="color: var(--sr-muted)">No limit</span>
                    }
                  </td>
                  <td>
                    <button class="btn btn-sm btn-outline-secondary me-1" (click)="startEdit(cat)">Edit</button>
                    <button class="btn btn-sm btn-outline-secondary" (click)="remove(cat.CategoryId)">Delete</button>
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
export class CategoryComponent implements OnInit {
  categories: FCategory[] = [];
  editing = false;
  form: Partial<FCategory> = {};

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() { this.load(); }

  load() {
    this.http.get<FCategory[]>('api/category').subscribe({
      next: c => { this.categories = c; this.cdr.detectChanges(); }
    });
  }

  startCreate() { this.form = {}; this.editing = true; }
  startEdit(cat: FCategory) { this.form = { ...cat }; this.editing = true; }
  cancel() { this.editing = false; this.form = {}; }

  save() {
    if (this.form.CategoryId) {
      this.http.put(`api/category/${this.form.CategoryId}`, this.form).subscribe(() => { this.load(); this.cancel(); });
    } else {
      this.http.post('api/category', this.form).subscribe(() => { this.load(); this.cancel(); });
    }
  }

  remove(id: number) {
    if (confirm('Delete this category?'))
      this.http.delete(`api/category/${id}`).subscribe({
        next: () => this.load(),
        error: (err) => { alert(err.error?.Message || 'Delete failed'); }
      });
  }
}
