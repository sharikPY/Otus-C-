using System;
using System.Collections.Generic;
using FastPekar.Core.DataAccess;
using FastPekar.Entities;
using FastPekar.Exceptions;

namespace FastPekar.Services
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

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            if (string.IsNullOrWhiteSpace(namePrefix))
                return Array.Empty<ToDoItem>();

            return _toDoRepository.Find(user.UserId,
                t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_toDoRepository.CountActive(user.UserId) >= _maxTaskCount)
                throw new TaskCountLimitException(_maxTaskCount);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя задачи не может быть пустым.");

            string trimmed = name.Trim();

            if (trimmed.Length > _maxTaskLength)
                throw new TaskLengthLimitException(trimmed.Length, _maxTaskLength);

            if (_toDoRepository.ExistsByName(user.UserId, trimmed))
                throw new DuplicateTaskException(trimmed);

            var item = new ToDoItem(user, trimmed);
            _toDoRepository.Add(item);
            return item;
        }

        public void MarkCompleted(Guid id)
        {
            var item = _toDoRepository.Get(id);
            if (item == null)
                return;

            item.State = ToDoItemState.Completed;
            item.StateChangedAt = DateTime.UtcNow;
            _toDoRepository.Update(item);
        }

        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }
    }
}