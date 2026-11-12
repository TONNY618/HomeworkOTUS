using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository {
	private readonly List<ToDoItem> _tasks = [];

	public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult((IReadOnlyList<ToDoItem>) [.. _tasks.Where(t => t.User.UserId == userId)]);
	}

	public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult((IReadOnlyList<ToDoItem>) [.. _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active)]);
	}

	public Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));
	}

	public Task AddAsync(ToDoItem item, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		_tasks.Add(item);
		return Task.CompletedTask;
	}

	public Task UpdateAsync(ToDoItem item, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		var index = _tasks.FindIndex(i => i.Id == item.Id);

		if (index != -1)
			_tasks[index] = item;

		return Task.CompletedTask;
	}

	public Task DeleteAsync(Guid id, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		_tasks.RemoveAll(t => t.Id == id);
		return Task.CompletedTask;
	}

	public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult(_tasks.Any(t => t.User.UserId == userId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));
	}

	public Task<int> CountActiveAsync(Guid userId, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult(_tasks.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active));
	}

	public Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> pred, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult<IReadOnlyList<ToDoItem>>([.. _tasks.Where(t => t.User.UserId == userId && pred(t))]);
	}
}
