var username = "";

Console.WriteLine("""
                  Здравствуйте, это консольный бот.
                  Доступные команды: /start /help /info /exit
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
			                   /exit => завершает работу программы.
			                   """);
			break;
		case "info":
			Console.WriteLine($"""
			                  {prefix} информация о программе:
			                  Версия: 0.0.1
			                  Дата создания: 29.09.2026
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
		case "exit":
			Console.WriteLine($"{prefix} выключаюсь.");
			return;
		default:
			Console.WriteLine($"{prefix} неизвестная команда. Введите /help для получения справки.");
			break;
	}
}