using Budget.Pass.Submit.Model;

namespace Budget.Pass.Submit.Repository;

public interface IRepositoryPassSubmit
{
    Task<int> UpdatePaymentAsync(SubmitPayment payment);
    Task<int> CreateOrUpdatePaymentAsync(SubmitPayment payment);
    Task<bool> IsAlreadyRecordedAsync(SubmitPayment payment);
    Task InsertPaymentHistoryAsync(SubmitPayment payment, bool isOfflineConflict = false);
}
