using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastPekar.Core.DataAccess;
using FastPekar.Core.Entities;
using FastPekar.Core.Exceptions;


namespace FastPekar.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;
        private readonly int _maxTaskCount;
        private readonly int _maxTaskLength;

        public ToDoService(IToDoRepository toDoRepository, int maxTaskCount, int maxTaskLength)
        {
            _toDoRepository = toDoRepository;
            _maxTaskCount = maxTaskCount;
            _maxTaskLength = maxTaskLength;
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return _toDoRepository.GetAllByUserIdAsync(userId, ct);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return _toDoRepository.GetActiveByUserIdAsync(userId, ct);
        }

        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken ct)
        {
            int activeCount = await _toDoRepository.CountActiveAsync(user.UserId, ct);
            if (activeCount >= _maxTaskCount)
                throw new TaskCountLimitException(_maxTaskCount);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя задачи не может быть пустым.");

            string trimmed = name.Trim();

            if (trimmed.Length > _maxTaskLength)
                throw new TaskLengthLimitException(trimmed.Length, _maxTaskLength);

            bool exists = await _toDoRepository.ExistsByNameAsync(user.UserId, trimmed, ct);
            if (exists)
                throw new DuplicateTaskException(trimmed);

            var item = new ToDoItem(user, trimmed);
            await _toDoRepository.AddAsync(item, ct);
            return item;
        }

        public async Task MarkCompletedAsync(Guid id, CancellationToken ct)
        {
            var item = await _toDoRepository.GetAsync(id, ct);
            if (item == null)
                return;

            item.State = ToDoItemState.Completed;
            item.StateChangedAt = DateTime.UtcNow;
            await _toDoRepository.UpdateAsync(item, ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            return _toDoRepository.DeleteAsync(id, ct);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(namePrefix))
            {
                return Task.FromResult<IReadOnlyList<ToDoItem>>(Array.Empty<ToDoItem>());
            }

            return _toDoRepository.FindAsync(user.UserId,
                t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase), ct);
        }
    }
}