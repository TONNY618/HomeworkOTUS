var username = "";
var tasks = new List<string>();

Console.WriteLine("""
                  Здравствуйте, это консольный бот.
                  Доступные команды: /start /help /info /echo /addtask /showtasks /removetask /exit
                  """);

while (true) {
	var prefix = string.IsNullOrEmpty(username) ? "От bot:" : $"От bot для {username}:";

	Console.Write("\nВведите команду: ");
	var raw = Console.ReadLine()?.Trim();
	if (string.IsNullOrEmpty(raw)) continue;

	var parts = raw.Split(' ', 2);

	switch (parts[0].TrimStart('/').ToLower()) {
		case "start":
			Console.Write($"{prefix} введите ваше новое имя: ");
			var temp = Console.ReadLine()?.Trim();
			if (string.IsNullOrEmpty(temp)) continue;
			username = temp;
			Console.WriteLine($"От bot: здравствуйте, {username}. Теперь вам доступна команда /echo");
			break;
		case "help":
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
			break;
		case "info":
			Console.WriteLine($"""
			                  {prefix} информация о программе:
			                  Версия: 0.0.2
			                  Дата создания: 29.09.2026
			                  Дата обновления: 30.09.2026
			                  """);
			break;
		case "echo":
			if (string.IsNullOrEmpty(username))
				Console.WriteLine($"{prefix} команда доступна только после регистрации через /start");
			else if (parts.Length < 2 || string.IsNullOrEmpty(parts[1]))
				Console.WriteLine($"{prefix} укажите текст после команды.");
			else
				Console.WriteLine(parts[1]);
			break;
		case "addtask":
			Console.Write($"{prefix} введите описание задачи: ");
			var task = Console.ReadLine()?.Trim();
			if (string.IsNullOrEmpty(task)) {
				Console.WriteLine("Описание задачи не может быть пустым.");
				continue;
			}
			tasks.Add(task);
			Console.WriteLine($"{prefix} задача \"{task}\" добавлена в список.");
			break;
		case "showtasks":
			ShowTasks(prefix);
			break;
		case "removetask":
			if (!ShowTasks(prefix)) continue;
			while (true) {
				Console.Write("Напишите порядковый номер задачи для удаления (напишите 0 для выхода): ");
				if (!int.TryParse(Console.ReadLine()?.Trim(), out var num)) {
					Console.WriteLine("Недопустимый порядковый номер.");
					continue;
				}
				if (num > 0 && num <= tasks.Count) {
					Console.WriteLine($"Задача \"{tasks[num - 1]}\" удалена.");
					tasks.RemoveAt(num - 1);
					break;
				}
				if (num == 0) {
					break;
				}

				Console.WriteLine("Недопустимый порядковый номер.");
			}
			break;
		case "exit":
			Console.WriteLine($"{prefix} выключаюсь.");
			return;
		default:
			Console.WriteLine($"{prefix} неизвестная команда. Введите /help для получения справки.");
			break;
	}
}

bool ShowTasks(string prefix) {
	if (tasks.Count <= 0) {
		Console.WriteLine($"{prefix} список задач пуст.");
		return false;
	}

	Console.WriteLine($"{prefix} список задач:");
	for (var i = 0; i < tasks.Count; i++) {
		Console.WriteLine($"{i + 1}. {tasks[i]}");
	}
	return true;
}