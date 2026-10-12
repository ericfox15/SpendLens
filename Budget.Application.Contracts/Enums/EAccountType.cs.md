# EAccountType.cs

**Project:** Budget.Application.Contracts
**Layer:** Contracts

## What it does

Classifies what kind of financial account a record represents:

```
Checking    = 1
CreditCard  = 2
Cash        = 3
Savings     = 4
```

## Where it's used

- `FAccount.AccountType` stores this value.
- `AccountComponent` (Angular) renders a `<select>` with these options.
- `getTypeName()` in `AccountComponent` maps the integer back to a readable label.
- Reports and the pass can theoretically group spending by account type (e.g., separate credit-card spend from cash spend).

## Design note

The app doesn't currently enforce different business rules per account type (e.g., no overdraft detection for checking). The enum is here to mirror real-world categorization and make the data meaningful when you later add features like "show credit card debt separately."
