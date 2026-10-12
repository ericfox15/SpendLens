# ControllerCategory.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

CRUD endpoints for spending categories.

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | `api/category` | Read | Get all categories |
| GET | `api/category/{id}` | Read | Get category by ID |
| POST | `api/category` | Write | Create new category |
| PUT | `api/category/{id}` | Write | Update category (name, monthly limit) |
| DELETE | `api/category/{id}` | Write | Soft-delete category |

## MonthlyLimit is optional

When creating a category without a `MonthlyLimit`, send `null` (or omit the field from JSON). The API accepts `decimal?` — a null limit means "no budget cap for this category."

## Same pattern as other CRUD controllers

Reads → repository direct. Writes → through service. Categories don't have complex business logic, so the service is thin delegation.
