using Budget.Shared.Account.Model;
using Budget.Shared.Account.Repository;

namespace Budget.Shared.Account.Service;

public class ServiceAccount(IRepositoryAccount repositoryAccount) : IServiceAccount
{
    public Task<IEnumerable<FAccount>> GetAccountsAsync() => repositoryAccount.SelectAccountsAsync();
    public Task<FAccount?> GetAccountByIdAsync(int accountId) => repositoryAccount.SelectAccountByIdAsync(accountId);
    public Task<int> CreateAccountAsync(FAccount account) => repositoryAccount.InsertAccountAsync(account);
    public Task UpdateAccountAsync(FAccount account) => repositoryAccount.UpdateAccountAsync(account);
    public Task DeleteAccountAsync(int accountId) => repositoryAccount.DeleteAccountAsync(accountId);
}
