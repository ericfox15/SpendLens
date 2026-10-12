export enum EAccountType { Checking = 1, CreditCard = 2, Cash = 3, Savings = 4 }
export enum EPaymentStatus { Pending = 0, Paid = 1, Skipped = 2 }
export enum EScheduleType { Monthly = 1, Biweekly = 2, Weekly = 3 }

export interface FAccount {
  AccountId: number;
  Name: string;
  AccountType: EAccountType;
  IsActive: boolean;
}

export interface FCategory {
  CategoryId: number;
  Name: string;
  MonthlyLimit: number | null;
}

export interface FBill {
  BillId: number;
  AccountId: number;
  CategoryId: number;
  Name: string;
  ExpectedAmount: number;
  ScheduleType: EScheduleType;
  DayOfMonth: number | null;
  AnchorDateUTC: string | null;
  IsAutoPay: boolean;
  IsActive: boolean;
  AccountName?: string;
  CategoryName?: string;
}

export interface FPayment {
  PaymentId: number;
  BillId: number | null;
  AccountId: number;
  CategoryId: number;
  DueAtUTC: string | null;
  PaidAtUTC: string | null;
  Amount: number | null;
  Status: EPaymentStatus;
  Note: string | null;
  ModifiedBy: number;
  ModifiedUTC: string;
  BillName?: string;
  CategoryName?: string;
  ExpectedAmount?: number;
  IsAutoPay?: boolean;
}

export interface FSetting {
  Path: string;
  Value: string;
}

export interface AdherenceFlag {
  BillId: number;
  BillName: string;
  SkippedCount: number;
}

export interface CategorySpendReport {
  CategoryId: number;
  CategoryName: string;
  TotalSpent: number;
  MonthlyLimit: number | null;
  Percentage: number;
}

export interface PassGenerateResponse {
  Standard: { Accounts: FAccount[]; Bills: FBill[]; Payments: FPayment[] };
  Extra: { AdherenceFlags: AdherenceFlag[]; OverdueCount: number };
  Optional: { Notes: string[] };
}

export interface SubmitPayment {
  Standard: {
    PaymentId: number;
    BillId: number;
    AccountId: number;
    CategoryId: number;
    DueAtUTC: string | null;
    PaidAtUTC: string | null;
    Amount: number | null;
    Status: EPaymentStatus;
    Note: string | null;
    ModifiedBy: number;
  };
}

export interface PaymentBindingId { TempId: number; Id: number; }

export interface InitializationSettings {
  Settings: Record<string, string>;
  ActionRoutes: string;
  ShowAutoPay: boolean;
  OverBudgetWarnPercent: number;
}

export interface ImportResult {
  Transactions: ImportedTransaction[];
  SpendByCategory: CategorySpendReport[];
  ImportedCount: number;
  SkippedCount: number;
}

export interface ImportedTransaction {
  Date: string;
  Description: string;
  Amount: number;
  Category: string;
  AutoCategorized: boolean;
}
