import { Component, OnInit } from '@angular/core';
import { FCategory } from '../shared/model/account.model';
import { CategoryHttpService } from '../core/http/category.http.service';

@Component({
  selector: 'app-category',
  standalone: false,
  template: `
    <div class="container py-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2><i class="fas fa-tags me-2"></i>Categories</h2>
        <button class="btn btn-primary" (click)="startCreate()">
          <i class="fas fa-plus me-1"></i>Add Category
        </button>
      </div>

      @if (editing) {
        <div class="card mb-4">
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
              <button class="btn btn-secondary" (click)="cancel()">Cancel</button>
            </div>
          </div>
        </div>
      }

      <div class="card">
        <div class="table-responsive">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr><th>Name</th><th>Monthly Limit</th><th>Actions</th></tr>
            </thead>
            <tbody>
              @for (cat of categories; track cat.CategoryId) {
                <tr>
                  <td><strong>{{ cat.Name }}</strong></td>
                  <td>{{ cat.MonthlyLimit ? ('$' + (cat.MonthlyLimit | number:'1.2-2')) : 'No limit' }}</td>
                  <td>
                    <button class="btn btn-sm btn-outline-primary me-1" (click)="startEdit(cat)">Edit</button>
                    <button class="btn btn-sm btn-outline-danger" (click)="delete(cat.CategoryId)">Delete</button>
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

  constructor(private categoryHttp: CategoryHttpService) {}

  ngOnInit() { this.load(); }

  load() { this.categoryHttp.getAll().subscribe(c => this.categories = c); }

  startCreate() { this.form = {}; this.editing = true; }
  startEdit(cat: FCategory) { this.form = { ...cat }; this.editing = true; }
  cancel() { this.editing = false; this.form = {}; }

  save() {
    if (this.form.CategoryId) {
      this.categoryHttp.update(this.form.CategoryId, this.form).subscribe(() => { this.load(); this.cancel(); });
    } else {
      this.categoryHttp.create(this.form).subscribe(() => { this.load(); this.cancel(); });
    }
  }

  delete(id: number) {
    if (confirm('Delete this category?'))
      this.categoryHttp.remove(id).subscribe(() => this.load());
  }
}
