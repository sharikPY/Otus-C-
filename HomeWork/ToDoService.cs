using System;
using System.Collections.Generic;
using System.Linq;

namespace FastPekar
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _tasks = new();
        private readonly int _maxTaskCount;
        private readonly int _maxTaskLength;

        public ToDoService(int maxTaskCount, int maxTaskLength)
        {
            _maxTaskCount = maxTaskCount;
            _maxTaskLength = maxTaskLength;
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId).ToList();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks
                .Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active)
                .ToList();
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_tasks.Count >= _maxTaskCount)
                throw new TaskCountLimitException(_maxTaskCount);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя задачи не может быть пустым.");

            string trimmed = name.Trim();

            if (trimmed.Length > _maxTaskLength)
                throw new TaskLengthLimitException(trimmed.Length, _maxTaskLength);

            if (_tasks.Any(t => t.User.UserId == user.UserId && t.Name == trimmed))
                throw new DuplicateTaskException(trimmed);

            var item = new ToDoItem(user, trimmed);
            _tasks.Add(item);
            return item;
        }

        public void MarkCompleted(Guid id)
        {
            var item = _tasks.FirstOrDefault(t => t.Id == id);
            if (item == null)
                return;

            item.State = ToDoItemState.Completed;
            item.StateChangedAt = DateTime.UtcNow;
        }

        public void Delete(Guid id)
        {
            _tasks.RemoveAll(t => t.Id == id);
        }
    }
}