using System;
using System.Threading;
using System.Threading.Tasks;
using FastPekar.Core.Entities;   // ← добавить (для ToDoUser)

namespace FastPekar.Core.DataAccess
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct);
        Task AddAsync(ToDoUser user, CancellationToken ct);
    }
}