namespace Budget.Application.Contracts.Enums;

/// <summary>Bit-flag permissions stored in the Permission table, matching eMAR's EPermission design.</summary>
public enum EPermission : long
{
    Read  = 1,
    Write = 2,
    Admin = 4
}
