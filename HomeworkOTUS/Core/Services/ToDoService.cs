using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;
using HomeworkOTUS.Core.Exceptions;

namespace HomeworkOTUS.Core.Services;

public class ToDoService : IToDoService {
	private readonly IToDoRepository _toDoRepository;
	private readonly int _maxTaskCount;
	private readonly int _maxTaskLength;

	public ToDoService(IToDoRepository toDoRepository, int maxTaskCount = 100, int maxTaskLength = 100) {
		_toDoRepository = toDoRepository;
		_maxTaskCount = maxTaskCount;
		_maxTaskLength = maxTaskLength;
	}

	public async Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken ct = default) {
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Имя не может быть пустым");

		name = name.Trim();

		if (await _toDoRepository.CountActiveAsync(user.UserId, ct) >= _maxTaskCount)
			throw new TaskCountLimitException(_maxTaskCount);

		if (name.Length > _maxTaskLength)
			throw new TaskLengthLimitException(name.Length, _maxTaskLength);

		if (await _toDoRepository.ExistsByNameAsync(user.UserId, name, ct))
			throw new DuplicateTaskException(name);

		var task = new ToDoItem(user, name);
		await _toDoRepository.AddAsync(task, ct);
		return task;
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default) {
		var task = await _toDoRepository.GetAsync(id, ct);

		if (task is null)
			throw new KeyNotFoundException($"Задача {id} не найдена");

		await _toDoRepository.DeleteAsync(id, ct);
	}

	public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default) {
		return await _toDoRepository.GetAllByUserIdAsync(userId, ct);
	}

	public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default) {
		return await _toDoRepository.GetActiveByUserIdAsync(userId, ct);
	}

	public async Task MarkCompletedAsync(Guid id, CancellationToken ct = default) {
		var task = await _toDoRepository.GetAsync(id, ct);

		if (task is null)
			throw new KeyNotFoundException($"Задача {id} не найдена");

		if (task.State == ToDoItemState.Completed)
			throw new InvalidOperationException("Задача уже выполнена");

		task.Complete();
		await _toDoRepository.UpdateAsync(task, ct);
	}

	public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct = default) {
		return await _toDoRepository.FindAsync(user.UserId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase), ct);
	}
}
