using Budget.Application.Contracts.Enums;
using Budget.Shared.User.Model;

namespace Budget.Shared.User.Repository;

public interface IRepositoryUser
{
    Task<FUser?> SelectUserByUsernameAsync(string username);
    Task<FUser?> SelectUserByIdAsync(int userId);
    Task<int> InsertUserAsync(FUser user);
    Task<IEnumerable<EPermission>> SelectPermissionsAsync(int userId);
    Task InsertPermissionAsync(int userId, EPermission permission);
}
