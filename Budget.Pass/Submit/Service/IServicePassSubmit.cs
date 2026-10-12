using Budget.Pass.Submit.Model;

namespace Budget.Pass.Submit.Service;

public interface IServicePassSubmit
{
    Task<IEnumerable<PaymentBindingId>> SubmitPaymentsAsync(SubmitPaymentRequest request);
}
