using Budget.Shared.Account.Model;
using Budget.Shared.Bill.Model;
using Budget.Shared.Payment.Model;
using Budget.Pass.Generate.Model;

namespace Budget.Pass.Generate.Repository;

public interface IRepositoryPassGenerate
{
    Task<IEnumerable<FBill>> SelectActiveBillsAsync();
    Task<IEnumerable<FAccount>> SelectActiveAccountsAsync();
    Task<IEnumerable<FPayment>> SelectExistingPaymentsAsync(DateTimeOffset startUTC, DateTimeOffset endUTC);
    Task<int> InsertPaymentAsync(FPayment payment);
    Task<IEnumerable<AdherenceFlag>> SelectAdherenceFlagsAsync();
    Task<int> SelectOverdueCountAsync();
}
