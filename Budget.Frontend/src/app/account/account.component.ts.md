# account.component.ts

**Project:** Budget.Frontend
**Layer:** Account CRUD

## What it does

Full CRUD page for accounts. Inline form (no separate page/modal) that toggles between "list mode" and "edit mode."

## Pattern: inline edit form

```typescript
editing = false;
form: Partial<FAccount> = {};

startCreate() {
  this.form = { AccountType: EAccountType.Checking, IsActive: true };
  this.editing = true;
}

startEdit(account: FAccount) {
  this.form = { ...account };   // spread clone — don't mutate the list item directly
  this.editing = true;
}

cancel() {
  this.editing = false;
  this.form = {};
}
```

`@if (editing)` in the template shows/hides the form card. When `form.AccountId` is set, save() calls `update()`; when it's null, save() calls `create()`.

## Spread clone

`this.form = { ...account }` creates a shallow copy of the account. This is important — if you did `this.form = account`, editing the form would mutate the list item directly (before saving), making Cancel not work correctly.

## delete → deactivate

```typescript
delete(id: number) {
  if (confirm('Deactivate this account?'))
    this.accountHttp.remove(id).subscribe(() => this.load());
}
```

`remove()` calls `DELETE api/account/{id}` which sets `IsActive = 0` — a soft delete, not a real delete. After success, `load()` re-fetches the list (now excluding the deactivated account).

## Bill/Category components

`bill.component.ts` and `category.component.ts` follow the exact same pattern. Once you understand one CRUD component, you understand all three.
