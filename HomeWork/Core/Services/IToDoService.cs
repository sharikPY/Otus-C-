using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastPekar.Core.Entities;

namespace FastPekar.Core.Services
{
    public interface IToDoService
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
        Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken ct);
        Task MarkCompletedAsync(Guid id, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct);
    }
}