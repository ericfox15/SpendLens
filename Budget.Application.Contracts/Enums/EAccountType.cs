namespace Budget.Application.Contracts.Enums;

/// <summary>Type of financial account. Stored as tinyint in dbo.Account.AccountType.</summary>
public enum EAccountType : byte
{
    Checking   = 1,
    CreditCard = 2,
    Cash       = 3,
    Savings    = 4
}
