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

	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct) {
		var msg = update.Message;
		var chat = msg.Chat;
		var raw = msg.Text.Trim();

		var userId = msg.From.Id;
		var username = msg.From.Username ?? "Пользователь";
		var user = await _userService.GetUserAsync(userId, ct);

		var parts = raw.Split(' ', 2);
		var command = parts[0].TrimStart('/').ToLower();
		var argument = parts.Length > 1 ? parts[1] : "";

		string[] noRegCommands = ["start", "help", "info"];

		try {
			if (user is null && !noRegCommands.Contains(command)) {
				await botClient.SendMessage(chat, $"Без регистрации вам доступны только команды {string.Join(", ", noRegCommands)}", ct);
				return;
			}

			switch (command) {
				case "start":
					if (user is null) {
						user = await _userService.RegisterUserAsync(userId, username, ct);
						await botClient.SendMessage(chat, $"Здравствуйте, {user.TelegramUserName}. Теперь вам доступно создание задач.", ct);
					} else
						await botClient.SendMessage(chat, $"Вы уже зарегистрированы: {user.TelegramUserName}", ct);
					break;
				case "help":
					await botClient.SendMessage(chat, """
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
					                            """, ct);
					break;
				case "info":
					await botClient.SendMessage(chat, """
					                            Информация о программе:
					                            Версия: 0.4.0
					                            Дата создания: 29.09.2026
					                            Дата обновления: 13.11.2026
					                            """, ct);
					break;
				case "addtask":
					if (string.IsNullOrWhiteSpace(argument)) {
						await botClient.SendMessage(chat, "Описание задачи не может быть пустым.", ct);
						return;
					}

					var newItem = await _toDoService.AddAsync(user!, argument, ct);
					await botClient.SendMessage(chat, $"задача \"{newItem.Name}\" добавлена в список ({newItem.Id}).", ct);
					break;
				case "showtasks":
					await RevealTasksAsync(await _toDoService.GetActiveByUserIdAsync(user!.UserId, ct), "активных задач", false);
					break;
				case "showalltasks":
					await RevealTasksAsync(await _toDoService.GetAllByUserIdAsync(user!.UserId, ct), "всех задач", true);
					break;
				case "completetask":
					if (!Guid.TryParse(argument, out var compId)) {
						await botClient.SendMessage(chat, "Неверный формат Id.", ct);
						return;
					}

					await _toDoService.MarkCompletedAsync(compId, ct);

					await botClient.SendMessage(chat, $"Задача {compId} выполнена.", ct);
					break;
				case "removetask":
					if (!Guid.TryParse(argument, out var remId)) {
						await botClient.SendMessage(chat, "Неверный формат Id.", ct);
						return;
					}

					await _toDoService.DeleteAsync(remId, ct);

					await botClient.SendMessage(chat, $"Задача {remId} удалена.", ct);
					break;
				case "report":
					var (total, completed, active, generatedAt) = await _toDoReportService.GetUserStatsAsync(user!.UserId, ct);
					await botClient.SendMessage(chat, $"{generatedAt:dd.MM.yyyy HH:mm:ss}. Всего: {total}, завершённые: {completed}, активные: {active}.", ct);
					break;
				case "find":
					if (string.IsNullOrWhiteSpace(argument)) {
						await botClient.SendMessage(chat, "Префикс не может быть пустым.", ct);
						return;
					}

					var foundTasks = await _toDoService.FindAsync(user!, argument, ct);
					await RevealTasksAsync(foundTasks, $"задач с префиксом {argument}", true);
					break;
				default:
					await botClient.SendMessage(chat, "Неизвестная команда. Введите /help для получения справки.", ct);
					break;
			}
		} catch (OperationCanceledException) {
			throw;
		} catch (Exception e) {
			await botClient.SendMessage(chat, $"Ошибка: {e.Message}", ct);
		}
		return;

		async Task RevealTasksAsync(IReadOnlyList<ToDoItem> tasks, string title, bool state) {
			if (tasks.Count == 0) {
				await botClient.SendMessage(chat, $"Список {title} пуст.", ct);
				return;
			}

			await botClient.SendMessage(chat, $"""
			                             Список {title}:
			                             {string.Join("\n", tasks.Select((t, i) => $"{i + 1}. {(state ? $"({t.State}) " : "")}{t.Name} - {t.CreatedAt:dd.MM.yyyy HH:mm:ss} - {t.Id}"))}
			                             """, ct);
		}
	}

	public Task HandleErrorAsync(ITelegramBotClient botClient, Exception e, CancellationToken ct) {
		var color = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"Ошибка: {e.GetType().Name} - {e.Message}");
		Console.ForegroundColor = color;
		return Task.CompletedTask;
	}
}
