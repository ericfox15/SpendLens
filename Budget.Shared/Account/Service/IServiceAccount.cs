using Budget.Shared.Account.Model;

namespace Budget.Shared.Account.Service;

/// <summary>
/// Business-logic layer for accounts. Simple CRUD — the service just delegates to the repository.
/// More complex domains (Pass) have real business rules here.
/// </summary>
public interface IServiceAccount
{
    Task<IEnumerable<FAccount>> GetAccountsAsync();
    Task<FAccount?> GetAccountByIdAsync(int accountId);
    Task<int> CreateAccountAsync(FAccount account);
    Task UpdateAccountAsync(FAccount account);
    Task DeleteAccountAsync(int accountId);
}
