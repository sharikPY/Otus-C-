using System.Threading;
using System.Threading.Tasks;
using FastPekar.Core.Entities;

namespace FastPekar.Core.Services
{
    public interface IUserService
    {
        Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct);
        Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct);
    }
}