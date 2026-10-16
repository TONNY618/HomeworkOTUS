using HomeworkOTUS.Exceptions;
using HomeworkOTUS.Models;

namespace HomeworkOTUS.Services;

public class ToDoService : IToDoService {
	private readonly List<ToDoItem> _tasks = [];
	private readonly int _maxTaskCount;
	private readonly int _maxTaskLength;

	public ToDoService(int maxTaskCount = 100, int maxTaskLength = 100) {
		_maxTaskCount = maxTaskCount;
		_maxTaskLength = maxTaskLength;
	}

	public ToDoItem Add(ToDoUser user, string name) {
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Имя не может быть пустым");

		name = name.Trim();

		if (_tasks.Count(t => t.User.UserId == user.UserId) >= _maxTaskCount)
			throw new TaskCountLimitException(_maxTaskCount);

		if (name.Length > _maxTaskLength)
			throw new TaskLengthLimitException(name.Length, _maxTaskLength);

		if (_tasks.Any(t => t.User.UserId == user.UserId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
			throw new DuplicateTaskException(name);

		var task = new ToDoItem(user, name);
		_tasks.Add(task);
		return task;
	}

	public void Delete(Guid id) {
		var task = _tasks.FirstOrDefault(t => t.Id == id);

		if (task is null)
			throw new KeyNotFoundException($"Задача {id} не найдена");

		_tasks.Remove(task);
	}

	public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) {
		return _tasks.Where(t => t.User.UserId == userId).ToList();
	}

	public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) {
		return _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
	}

	public void MarkCompleted(Guid id) {
		var task = _tasks.FirstOrDefault(t => t.Id == id);

		if (task is null)
			throw new KeyNotFoundException($"Задача {id} не найдена");

		if (task.State == ToDoItemState.Completed)
			throw new InvalidOperationException("Задача уже выполнена");

		task.Complete();
	}
}