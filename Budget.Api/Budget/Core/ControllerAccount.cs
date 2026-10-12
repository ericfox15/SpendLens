using Budget.Api.Budget.Attribute;
using Budget.Application.Contracts.Enums;
using Budget.Shared.Account.Model;
using Budget.Shared.Account.Repository;
using Budget.Shared.Account.Service;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Budget.Core;

/// <summary>
/// CRUD endpoints for financial accounts.
/// Simple reads call the repository directly (no business rules needed).
/// Writes go through the service layer (mirrors eMAR's thin-controller rule).
/// </summary>
[Route("api/account")]
public class ControllerAccount(IServiceAccount serviceAccount, IRepositoryAccount repositoryAccount) : FControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetAccounts() => Ok(await repositoryAccount.SelectAccountsAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        var account = await repositoryAccount.SelectAccountByIdAsync(id);
        return account == null ? NotFound() : Ok(account);
    }

    [HttpPost("")]
    [Permission(EPermission.Write)]
    public async Task<IActionResult> CreateAccount([FromBody] FAccount account)
    {
        var id = await serviceAccount.CreateAccountAsync(account);
        return Ok(new { AccountId = id });
    }

    [HttpPut("{id:int}")]
    [Permission(EPermission.Write)]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] FAccount account)
    {
        account.AccountId = id;
        await serviceAccount.UpdateAccountAsync(account);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Permission(EPermission.Write)]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        await serviceAccount.DeleteAccountAsync(id);
        return Ok();
    }
}
