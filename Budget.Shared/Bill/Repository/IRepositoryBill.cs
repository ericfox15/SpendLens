using Budget.Shared.Bill.Model;

namespace Budget.Shared.Bill.Repository;

public interface IRepositoryBill
{
    Task<IEnumerable<FBill>> SelectBillsAsync();
    Task<FBill?> SelectBillByIdAsync(int billId);
    Task<IEnumerable<FBill>> SelectActiveBillsAsync();
    Task<int> InsertBillAsync(FBill bill);
    Task UpdateBillAsync(FBill bill);
    Task DeleteBillAsync(int billId);
}
