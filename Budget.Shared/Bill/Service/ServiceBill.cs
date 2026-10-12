using Budget.Shared.Bill.Model;
using Budget.Shared.Bill.Repository;

namespace Budget.Shared.Bill.Service;

public class ServiceBill(IRepositoryBill repositoryBill) : IServiceBill
{
    public Task<IEnumerable<FBill>> GetBillsAsync() => repositoryBill.SelectBillsAsync();
    public Task<FBill?> GetBillByIdAsync(int billId) => repositoryBill.SelectBillByIdAsync(billId);
    public Task<int> CreateBillAsync(FBill bill) => repositoryBill.InsertBillAsync(bill);
    public Task UpdateBillAsync(FBill bill) => repositoryBill.UpdateBillAsync(bill);
    public Task DeleteBillAsync(int billId) => repositoryBill.DeleteBillAsync(billId);
}
