using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastPekar.Core.DataAccess;
using FastPekar.Core.Entities;

namespace FastPekar.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _toDoRepository;

        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(
            Guid userId, CancellationToken ct)
        {
            var all = await _toDoRepository.GetAllByUserIdAsync(userId, ct);

            int total = all.Count;
            int completed = all.Count(t => t.State == ToDoItemState.Completed);
            int active = total - completed;

            return (total, completed, active, DateTime.UtcNow);
        }
    }
}