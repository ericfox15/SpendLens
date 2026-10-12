using Budget.Shared.Bill.Model;

namespace Budget.Shared.Bill.Service;

public interface IServiceBill
{
    Task<IEnumerable<FBill>> GetBillsAsync();
    Task<FBill?> GetBillByIdAsync(int billId);
    Task<int> CreateBillAsync(FBill bill);
    Task UpdateBillAsync(FBill bill);
    Task DeleteBillAsync(int billId);
}
