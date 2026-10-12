using Budget.Pass.Submit.Model;
using Budget.Pass.Submit.Repository;
using System.Transactions;

namespace Budget.Pass.Submit.Service;

/// <summary>
/// Processes each payment individually in its own TransactionScope.
/// Key eMAR patterns replicated here:
///   - One transaction PER payment (one bad payment can't fail the batch)
///   - Temp negative IDs: frontend assigns -1, -2... server returns TempId→RealId bindings
///   - IsOfflineSync: conflict → write PaymentHistory row instead of overwriting
/// </summary>
public class ServicePassSubmit(IRepositoryPassSubmit repositoryPassSubmit) : IServicePassSubmit
{
    public async Task<IEnumerable<PaymentBindingId>> SubmitPaymentsAsync(SubmitPaymentRequest request)
    {
        var bindings = new List<PaymentBindingId>();

        foreach (var payment in request.Payments)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            int realId;
            if (payment.Standard.PaymentId > 0)
            {
                // Existing row (scheduled payment) — just update it
                realId = await repositoryPassSubmit.UpdatePaymentAsync(payment);

                // Offline sync: if the server already has a newer status, record conflict in history
                if (request.IsOfflineSync && await repositoryPassSubmit.IsAlreadyRecordedAsync(payment))
                    await repositoryPassSubmit.InsertPaymentHistoryAsync(payment, isOfflineConflict: true);
            }
            else
            {
                // New row (ad-hoc / auto-pay) — create or find by (BillId, DueAtUTC)
                realId = await repositoryPassSubmit.CreateOrUpdatePaymentAsync(payment);
            }

            scope.Complete();
            bindings.Add(new PaymentBindingId(payment.Standard.PaymentId, realId));
        }

        return bindings;
    }
}
