using FastPekar.Core.DataAccess;
using FastPekar.Core.Entities;

namespace FastPekar.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _tasks = new();

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> result = _tasks.Where(t => t.User.UserId == userId).ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> result = _tasks
                .Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            var result = _tasks.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(result);
        }

        public Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            _tasks.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            var index = _tasks.FindIndex(t => t.Id == item.Id);
            if (index >= 0) _tasks[index] = item;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            _tasks.RemoveAll(t => t.Id == id);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            var result = _tasks.Any(t => t.User.UserId == userId && t.Name == name);
            return Task.FromResult(result);
        }

        public Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            var result = _tasks.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> result = _tasks
                .Where(t => t.User.UserId == userId && predicate(t))
                .ToList();
            return Task.FromResult(result);
        }
    }
}