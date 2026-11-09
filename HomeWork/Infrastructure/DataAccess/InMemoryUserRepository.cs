using FastPekar.Core.DataAccess;
using FastPekar.Core.Entities;

namespace FastPekar.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = new();

    public Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var result = _users.FirstOrDefault(u => u.UserId == userId);
        return Task.FromResult(result);
    }

    public Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
    {
        var result = _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
        return Task.FromResult(result);
    }

    public Task AddAsync(ToDoUser user, CancellationToken ct)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }
}