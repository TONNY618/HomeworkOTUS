using HomeworkOTUS.Core.Entities;
using HomeworkOTUS.Core.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace HomeworkOTUS.TelegramBot;

public class UpdateHandler : IUpdateHandler {
	private readonly IUserService _userService;
	private readonly IToDoService _toDoService;
	private readonly IToDoReportService _toDoReportService;

	public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService) {
		_userService = userService;
		_toDoService = toDoService;
		_toDoReportService = toDoReportService;
	}

	public void HandleUpdateAsync(ITelegramBotClient botClient, Update update) {
		var msg = update.Message;
		var chat = msg.Chat;
		var raw = msg.Text.Trim();

		var userId = msg.From.Id;
		var username = msg.From.Username ?? "Пользователь";
		var user = _userService.GetUser(userId);

		var parts = raw.Split(' ', 2);
		var command = parts[0].TrimStart('/').ToLower();
		var argument = parts.Length > 1 ? parts[1] : "";

		string[] noRegCommands = ["start", "help", "info"];

		try {
			if (user is null && !noRegCommands.Contains(command)) {
				botClient.SendMessage(chat, $"Без регистрации вам доступны только команды {string.Join(", ", noRegCommands)}");
				return;
			}

			switch (command) {
				case "start":
					if (user is null) {
						user = _userService.RegisterUser(userId, username);
						botClient.SendMessage(chat, $"Здравствуйте, {user.TelegramUserName}. Теперь вам доступно создание задач.");
					} else
						botClient.SendMessage(chat, $"Вы уже зарегистрированы: {user.TelegramUserName}");
					break;
				case "help":
					botClient.SendMessage(chat, """
					                            Список всех команд:
					                            /start => регистрация.
					                            /help => вызывает эту справку.
					                            /info => пишет версию и дату создания этой программы.
					                            /addtask <name> => создаёт задачу.
					                            /showtasks => показывает список активных задач.
					                            /showalltasks => отображает список абсолютно всех задач.
					                            /completetask <id> => помечает задачу как выполненную по её Id.
					                            /removetask <id> => удаляет задачу по её Id.
					                            /report => выводит статистику по задачам.
					                            /find <prefix> => найти задачи, которые начинаются с указанного текста.
					                            """);
					break;
				case "info":
					botClient.SendMessage(chat, """
					                            Информация о программе:
					                            Версия: 0.3.0
					                            Дата создания: 29.09.2026
					                            Дата обновления: 25.10.2026
					                            """);
					break;
				case "addtask":
					if (string.IsNullOrWhiteSpace(argument)) {
						botClient.SendMessage(chat, "Описание задачи не может быть пустым.");
						return;
					}

					var newItem = _toDoService.Add(user!, argument);
					botClient.SendMessage(chat, $"задача \"{newItem.Name}\" добавлена в список ({newItem.Id}).");
					break;
				case "showtasks":
					RevealTasks(_toDoService.GetActiveByUserId(user!.UserId), "активных задач");
					break;
				case "showalltasks":
					RevealTasks(_toDoService.GetAllByUserId(user!.UserId), "всех задач", true);
					break;
				case "completetask":
					if (!Guid.TryParse(argument, out var compId)) {
						botClient.SendMessage(chat, "Неверный формат Id.");
						return;
					}

					_toDoService.MarkCompleted(compId);

					botClient.SendMessage(chat, $"Задача {compId} выполнена.");
					break;
				case "removetask":
					if (!Guid.TryParse(argument, out var remId)) {
						botClient.SendMessage(chat, "Неверный формат Id.");
						return;
					}

					_toDoService.Delete(remId);

					botClient.SendMessage(chat, $"Задача {remId} удалена.");
					break;
				case "report":
					var (total, completed, active, generatedAt) = _toDoReportService.GetUserStats(user!.UserId);
					botClient.SendMessage(chat, $"{generatedAt:dd.MM.yyyy HH:mm:ss}. Всего: {total}, завершённые: {completed}, активные: {active}.");
					break;
				case "find":
					if (string.IsNullOrWhiteSpace(argument)) {
						botClient.SendMessage(chat, "Префикс не может быть пустым.");
						return;
					}

					var foundTasks = _toDoService.Find(user!, argument);
					RevealTasks(foundTasks, $"задач с префиксом {argument}", true);
					break;
				default:
					botClient.SendMessage(chat, "Неизвестная команда. Введите /help для получения справки.");
					break;
			}
		} catch (Exception e) {
			botClient.SendMessage(chat, $"Ошибка: {e.Message}");
		}
		return;

		void RevealTasks(IReadOnlyList<ToDoItem> tasks, string title, bool state = false) {
			if (tasks.Count == 0) {
				botClient.SendMessage(chat, $"Список {title} пуст.");
				return;
			}

			botClient.SendMessage(chat, $"""
			                             Список {title}:
			                             {string.Join("\n", tasks.Select((t, i) => $"{i + 1}. {(state ? $"({t.State}) "  : "")}{t.Name} - {t.CreatedAt:dd.MM.yyyy HH:mm:ss} - {t.Id}"))}
			                             """);
		}
	}
}