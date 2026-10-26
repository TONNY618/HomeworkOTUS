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

	public ToDoItem Add(ToDoUser user, string name) {
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Имя не может быть пустым");

		name = name.Trim();

		if (_toDoRepository.CountActive(user.UserId) >= _maxTaskCount)
			throw new TaskCountLimitException(_maxTaskCount);

		if (name.Length > _maxTaskLength)
			throw new TaskLengthLimitException(name.Length, _maxTaskLength);

		if (_toDoRepository.ExistsByName(user.UserId, name))
			throw new DuplicateTaskException(name);

		var task = new ToDoItem(user, name);
		_toDoRepository.Add(task);
		return task;
	}

	public void Delete(Guid id) {
		var task = _toDoRepository.Get(id);

		if (task is null)
			throw new KeyNotFoundException($"Задача {id} не найдена");

		_toDoRepository.Delete(id);
	}

	public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) {
		return _toDoRepository.GetAllByUserId(userId);
	}

	public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) {
		return _toDoRepository.GetActiveByUserId(userId);
	}

	public void MarkCompleted(Guid id) {
		var task = _toDoRepository.Get(id);

		if (task is null)
			throw new KeyNotFoundException($"Задача {id} не найдена");

		if (task.State == ToDoItemState.Completed)
			throw new InvalidOperationException("Задача уже выполнена");

		task.Complete();
		_toDoRepository.Update(task);
	}

	public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix) {
		return _toDoRepository.Find(user.UserId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
	}
}