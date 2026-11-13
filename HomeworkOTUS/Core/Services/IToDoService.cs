using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Core.Services;

public interface IToDoService {
	Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default);
	Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
	Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken ct = default);
	Task MarkCompletedAsync(Guid id, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
	Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct = default);
}
