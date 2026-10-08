using HomeworkOTUS.Exceptions;
using HomeworkOTUS.Models;

namespace HomeworkOTUS;

internal static class Lesson6 {
	private const int MinLimit = 1;
	private const int MaxLimit = 100;

	private static ToDoUser? _user;
	private static readonly List<ToDoItem> Tasks = [];
	private static int _maxTaskCount;
	private static int _maxTaskLength;

	private static void Main() {
		Console.WriteLine("""
		                  Здравствуйте, это консольный бот.
		                  Доступные команды: /start /help /info /echo /addtask /showtasks /showalltasks /completetask /removetask /exit
		                  """);

		var isRunning = true;
		var hasLimits = false;

		while (isRunning) {
			try {
				if (!hasLimits) {
					InitLimits();
					hasLimits = true;
				}

				var prefix = _user == null ? "От bot:" : $"От bot для {_user.TelegramUserName}:";

				Console.Write("\nВведите команду: ");
				var raw = Console.ReadLine()?.Trim();
				if (string.IsNullOrEmpty(raw)) continue;

				var parts = raw.Split(' ', 2);
				var argument = parts.Length > 1 ? parts[1] : "";

				switch (parts[0].TrimStart('/').ToLower()) {
					case "start":
						HandleStart(prefix);
						break;
					case "help":
						HandleHelp(prefix);
						break;
					case "info":
						HandleInfo(prefix);
						break;
					case "echo":
						HandleEcho(prefix, argument);
						break;
					case "addtask":
						HandleAddTask(prefix);
						break;
					case "showtasks":
						HandleShowTasks(prefix);
						break;
					case "showalltasks":
						HandleShowAllTasks(prefix);
						break;
					case "completetask":
						HandleCompleteTask(prefix, argument);
						break;
					case "removetask":
						HandleRemoveTask(prefix);
						break;
					case "exit":
						Console.WriteLine($"{prefix} выключаюсь.");
						isRunning = false;
						break;
					default:
						Console.WriteLine($"{prefix} неизвестная команда. Введите /help для получения справки.");
						break;
				}
			} catch (ArgumentException e) {
				Console.WriteLine(e.Message);
			} catch (TaskCountLimitException e) {
				Console.WriteLine(e.Message);
			} catch (TaskLengthLimitException e) {
				Console.WriteLine(e.Message);
			} catch (DuplicateTaskException e) {
				Console.WriteLine(e.Message);
			} catch (Exception e) {
				Console.WriteLine($"""
				                   Произошла непредвиденная ошибка:
				                   Type: {e.GetType().FullName}
				                   Message: {e.Message}
				                   StackTrace: {e.StackTrace}
				                   InnerException: {e.InnerException?.Message ?? "отсутствует"}
				                   """);
			}
		}
	}

	private static void ValidateString(string? str) {
		if (string.IsNullOrWhiteSpace(str))
			throw new ArgumentException("Строка не может быть пустой.");
	}

	private static int ParseAndValidateInt(string? str, int min, int max) {
		ValidateString(str);

		if (!int.TryParse(str, out var result))
			throw new ArgumentException($"{str} не является числом.");

		if (result < min || result > max)
			throw new ArgumentException($"{result} находится вне диапазона {min} - {max}");

		return result;
	}

	private static void InitLimits() {
		Console.Write($"Введите максимум для количества задач (с {MinLimit} по {MaxLimit}): ");
		_maxTaskCount = ParseAndValidateInt(Console.ReadLine(), MinLimit, MaxLimit);

		Console.Write($"Введите максимум для длины задачи (с {MinLimit} по {MaxLimit}): ");
		_maxTaskLength = ParseAndValidateInt(Console.ReadLine(), MinLimit, MaxLimit);
	}

	private static void HandleStart(string prefix) {
		Console.Write($"{prefix} введите ваше новое имя: ");
		var input = Console.ReadLine();
		ValidateString(input);

		_user = new ToDoUser(input!.Trim());
		Console.WriteLine($"От bot: здравствуйте, {_user.TelegramUserName}. Теперь вам доступна команда /echo и создание задач.");
	}

	private static void HandleHelp(string prefix) {
		Console.WriteLine($"""
		                   {prefix} список всех команд:
		                   /start => запись вашего имени.
		                   /help => вызывает эту справку.
		                   /info => пишет версию и дату создания этой программы.
		                   /echo <text> => выводит введённый текст (можно использовать только после /start).
		                   /addtask => просит описание задачи, а после добавляет её в список.
		                   /showtasks => показывает список активных задач.
		                   /showalltasks => отображает список абсолютно всех задач.
		                   /completetask <id> => помечает задачу как выполненную по её Id.
		                   /removetask => называет список всех задач и запрашивает порядковый номер задачи для её удаления.
		                   /exit => завершает работу программы.
		                   """);
	}

	private static void HandleInfo(string prefix) {
		Console.WriteLine($"""
		                   {prefix} информация о программе:
		                   Версия: 0.1.0
		                   Дата создания: 29.09.2026
		                   Дата обновления: 07.10.2026
		                   """);
	}

	private static void HandleEcho(string prefix, string text) {
		if (_user == null) {
			Console.WriteLine($"{prefix} команда доступна только после регистрации через /start");
			return;
		}

		ValidateString(text);
		Console.WriteLine(text);
	}

	private static void HandleAddTask(string prefix) {
		if (_user == null) {
			Console.WriteLine($"{prefix} команда доступна только после регистрации через /start");
			return;
		}

		if (Tasks.Count >= _maxTaskCount)
			throw new TaskCountLimitException(_maxTaskCount);

		Console.Write($"{prefix} введите описание задачи: ");
		var task = Console.ReadLine();

		ValidateString(task);
		task = task!.Trim();

		if (task.Length > _maxTaskLength)
			throw new TaskLengthLimitException(task.Length, _maxTaskLength);

		if (Tasks.Any(t => string.Equals(t.Name, task, StringComparison.OrdinalIgnoreCase)))
			throw new DuplicateTaskException(task);

		var newItem = new ToDoItem(_user, task);
		Tasks.Add(newItem);
		Console.WriteLine($"{prefix} задача \"{newItem.Name}\" добавлена в список ({newItem.Id}).");
	}

	private static void HandleShowTasks(string prefix) {
		var active = Tasks.Where(t => t.State == ToDoItemState.Active).ToList();

		if (active.Count == 0) {
			Console.WriteLine($"{prefix} список активных задач пуст.");
			return;
		}

		Console.WriteLine($"{prefix} список активных задач:");
		for (var i = 0; i < active.Count; i++)
			Console.WriteLine($"{i + 1}. {active[i].Name} - {active[i].CreatedAt:dd.MM.yyyy HH:mm:ss} - {active[i].Id}");
	}

	private static bool HandleShowAllTasks(string prefix) {
		if (Tasks.Count == 0) {
			Console.WriteLine($"{prefix} список задач пуст.");
			return false;
		}

		Console.WriteLine($"{prefix} список всех задач:");
		for (var i = 0; i < Tasks.Count; i++)
			Console.WriteLine($"{i + 1}. ({Tasks[i].State}) {Tasks[i].Name} - {Tasks[i].CreatedAt:dd.MM.yyyy HH:mm:ss} - {Tasks[i].Id}");

		return true;
	}

	private static void HandleCompleteTask(string prefix, string argument) {
		ValidateString(argument);

		if (!Guid.TryParse(argument, out var id)) {
			Console.WriteLine($"{prefix} неверный формат Id.");
			return;
		}

		var task = Tasks.FirstOrDefault(t => t.Id == id);
		if (task == null) {
			Console.WriteLine($"{prefix} задача с таким Id не найдена.");
			return;
		}

		if (task.State == ToDoItemState.Completed) {
			Console.WriteLine($"{prefix} задача уже выполнена.");
			return;
		}

		task.Complete();

		Console.WriteLine($"{prefix} задача \"{task.Name}\" выполнена.");
	}

	private static void HandleRemoveTask(string prefix) {
		if (!HandleShowAllTasks(prefix)) return;

		while (true) {
			Console.Write("Напишите порядковый номер задачи для удаления (напишите 0 для выхода): ");
			var input = Console.ReadLine();

			int num;
			try {
				num = ParseAndValidateInt(input, 0, Tasks.Count);
			} catch (ArgumentException) {
				Console.WriteLine("Недопустимый порядковый номер.");
				continue;
			}

			if (num == 0) return;

			Console.WriteLine($"Задача \"{Tasks[num - 1].Name}\" удалена.");
			Tasks.RemoveAt(num - 1);
			break;
		}
	}
}