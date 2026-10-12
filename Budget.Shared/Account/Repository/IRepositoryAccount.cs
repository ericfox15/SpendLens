using Budget.Shared.Account.Model;

namespace Budget.Shared.Account.Repository;

/// <summary>
/// Repository interface for Account data access. The interface lives here; the implementation
/// does too (unlike some patterns that separate them). Registered as Scoped in DI.
/// </summary>
public interface IRepositoryAccount
{
    Task<IEnumerable<FAccount>> SelectAccountsAsync();
    Task<FAccount?> SelectAccountByIdAsync(int accountId);
    Task<int> InsertAccountAsync(FAccount account);
    Task UpdateAccountAsync(FAccount account);
    Task DeleteAccountAsync(int accountId);
}
