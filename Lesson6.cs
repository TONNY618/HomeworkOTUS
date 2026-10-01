using HomeworkOTUS.Exceptions;

namespace HomeworkOTUS;

internal static class Lesson6 {
	private static string _username = "";
	private static readonly List<string> Tasks = [];
	private static int _maxTaskCount;
	private static int _maxTaskLength;

	private static void Main() {
		Console.WriteLine("""
		                  Здравствуйте, это консольный бот.
		                  Доступные команды: /start /help /info /echo /addtask /showtasks /removetask /exit
		                  """);

		while (true) {
			try {
				InitLimits();
				break;
			} catch (ArgumentException e) {
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

		while (true) {
			var prefix = string.IsNullOrEmpty(_username) ? "От bot:" : $"От bot для {_username}:";

			try {
				Console.Write("\nВведите команду: ");
				var raw = Console.ReadLine()?.Trim();
				if (string.IsNullOrEmpty(raw)) continue;

				var parts = raw.Split(' ', 2);

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
						HandleEcho(prefix, parts.Length > 1 ? parts[1] : "");
						break;
					case "addtask":
						HandleAddTask(prefix);
						break;
					case "showtasks":
						ShowTasks(prefix);
						break;
					case "removetask":
						HandleRemoveTask(prefix);
						break;
					case "exit":
						Console.WriteLine($"{prefix} выключаюсь.");
						return;
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
		Console.Write("Введите максимум для количества задач (с 1 по 100): ");
		_maxTaskCount = ParseAndValidateInt(Console.ReadLine(), 1, 100);

		Console.Write("Введите максимум для длины задачи (с 1 по 100): ");
		_maxTaskLength = ParseAndValidateInt(Console.ReadLine(), 1, 100);
	}

	private static void HandleStart(string prefix) {
		Console.Write($"{prefix} введите ваше новое имя: ");
		var input = Console.ReadLine();
		ValidateString(input);

		_username = input!.Trim();
		Console.WriteLine($"От bot: здравствуйте, {_username}. Теперь вам доступна команда /echo");
	}

	private static void HandleHelp(string prefix) {
		Console.WriteLine($"""
		                   {prefix} список всех команд:
		                   /start => запись вашего имени.
		                   /help => вызывает эту справку.
		                   /info => пишет версию и дату создания этой программы.
		                   /echo <text> => выводит введённый текст (можно использовать только после /start).
		                   /addtask => просит описание задачи, а после добавляет её в список.
		                   /showtasks => показывает список всех задач.
		                   /removetask => называет список всех задач и запрашивает порядковый номер задачи для её удаления.
		                   /exit => завершает работу программы.
		                   """);
	}

	private static void HandleInfo(string prefix) {
		Console.WriteLine($"""
		                   {prefix} информация о программе:
		                   Версия: 0.0.3
		                   Дата создания: 29.09.2026
		                   Дата обновления: 01.10.2026
		                   """);
	}

	private static void HandleEcho(string prefix, string text) {
		if (string.IsNullOrEmpty(_username)) {
			Console.WriteLine($"{prefix} команда доступна только после регистрации через /start");
			return;
		}

		ValidateString(text);
		Console.WriteLine(text);
	}

	private static void HandleAddTask(string prefix) {
		if (Tasks.Count >= _maxTaskCount)
			throw new TaskCountLimitException(_maxTaskCount);

		Console.Write($"{prefix} введите описание задачи: ");
		var task = Console.ReadLine();

		ValidateString(task);
		task = task!.Trim();

		if (task.Length > _maxTaskLength)
			throw new TaskLengthLimitException(task.Length, _maxTaskLength);

		if (Tasks.Contains(task, StringComparer.OrdinalIgnoreCase))
			throw new DuplicateTaskException(task);

		Tasks.Add(task);
		Console.WriteLine($"{prefix} задача \"{task}\" добавлена в список.");
	}

	private static void HandleRemoveTask(string prefix) {
		if (!ShowTasks(prefix)) return;
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

			Console.WriteLine($"Задача \"{Tasks[num - 1]}\" удалена.");
			Tasks.RemoveAt(num - 1);
			break;
		}
	}

	private static bool ShowTasks(string prefix) {
		if (Tasks.Count <= 0) {
			Console.WriteLine($"{prefix} список задач пуст.");
			return false;
		}

		Console.WriteLine($"{prefix} список задач:");
		for (var i = 0; i < Tasks.Count; i++)
			Console.WriteLine($"{i + 1}. {Tasks[i]}");

		return true;
	}
}