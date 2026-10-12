using Budget.Application.Contracts.Options;
using Budget.Pass.Submit.Model;
using Budget.Shared;
using Dapper;
using Microsoft.Extensions.Options;

namespace Budget.Pass.Submit.Repository;

public class RepositoryPassSubmit(IOptions<RepositoryOptions> options)
    : FRepositoryBase(options.Value.BudgetDatabaseConnectionString), IRepositoryPassSubmit
{
    public async Task<int> UpdatePaymentAsync(SubmitPayment payment)
    {
        const string sql = """
            UPDATE dbo.Payment
            SET Status = @Status, Amount = @Amount, PaidAtUTC = @PaidAtUTC,
                Note = @Note, ModifiedBy = @ModifiedBy, ModifiedUTC = SYSDATETIMEOFFSET()
            WHERE PaymentId = @PaymentId;
            SELECT @PaymentId;
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            payment.Standard.PaymentId,
            payment.Standard.Status,
            payment.Standard.Amount,
            payment.Standard.PaidAtUTC,
            payment.Standard.Note,
            payment.Standard.ModifiedBy
        });
    }

    public async Task<int> CreateOrUpdatePaymentAsync(SubmitPayment payment)
    {
        // For ad-hoc (PRN) payments and auto-pay — no pre-existing row
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.Payment WHERE BillId = @BillId AND DueAtUTC = @DueAtUTC)
                UPDATE dbo.Payment
                SET Status = @Status, Amount = @Amount, PaidAtUTC = @PaidAtUTC,
                    Note = @Note, ModifiedBy = @ModifiedBy, ModifiedUTC = SYSDATETIMEOFFSET()
                WHERE BillId = @BillId AND DueAtUTC = @DueAtUTC;
            ELSE
                INSERT INTO dbo.Payment (BillId, AccountId, CategoryId, DueAtUTC, PaidAtUTC, Amount, Status, Note, ModifiedBy)
                VALUES (@BillId, @AccountId, @CategoryId, @DueAtUTC, @PaidAtUTC, @Amount, @Status, @Note, @ModifiedBy);
            SELECT ISNULL((SELECT PaymentId FROM dbo.Payment WHERE BillId = @BillId AND DueAtUTC = @DueAtUTC), SCOPE_IDENTITY());
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            BillId = payment.Standard.BillId == 0 ? (int?)null : payment.Standard.BillId,
            payment.Standard.AccountId,
            payment.Standard.CategoryId,
            payment.Standard.DueAtUTC,
            payment.Standard.PaidAtUTC,
            payment.Standard.Amount,
            payment.Standard.Status,
            payment.Standard.Note,
            payment.Standard.ModifiedBy
        });
    }

    public async Task<bool> IsAlreadyRecordedAsync(SubmitPayment payment)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Payment WHERE PaymentId = @PaymentId AND Status != 0";
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { payment.Standard.PaymentId }) > 0;
    }

    public async Task InsertPaymentHistoryAsync(SubmitPayment payment, bool isOfflineConflict = false)
    {
        const string sql = """
            INSERT INTO dbo.PaymentHistory (PaymentId, Status, Amount, Note, ModifiedBy, ModifiedUTC, IsOfflineConflict)
            VALUES (@PaymentId, @Status, @Amount, @Note, @ModifiedBy, SYSDATETIMEOFFSET(), @IsOfflineConflict)
            """;
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, new
        {
            payment.Standard.PaymentId,
            payment.Standard.Status,
            payment.Standard.Amount,
            payment.Standard.Note,
            payment.Standard.ModifiedBy,
            IsOfflineConflict = isOfflineConflict
        });
    }
}
