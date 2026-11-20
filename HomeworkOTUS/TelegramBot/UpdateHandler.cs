using System.Text;
using HomeworkOTUS.Core.Entities;
using HomeworkOTUS.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeworkOTUS.TelegramBot;

public class UpdateHandler : IUpdateHandler {
	private static readonly ReplyKeyboardMarkup StartKeyboard = new(
		new KeyboardButton[][] {
			[new KeyboardButton("/start")],
		}) {
		ResizeKeyboard = true,
	};

	private static readonly ReplyKeyboardMarkup RegKeyboard = new(
		new KeyboardButton[][] {
			[new KeyboardButton("/showalltasks"), new KeyboardButton("/showtasks")],
			[new KeyboardButton("/report")],
		}) {
		ResizeKeyboard = true,
	};

	private readonly IUserService _userService;
	private readonly IToDoService _toDoService;
	private readonly IToDoReportService _toDoReportService;

	public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService) {
		_userService = userService;
		_toDoService = toDoService;
		_toDoReportService = toDoReportService;
	}

	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct) {
		if (update.Message is not {Text: {} raw, From: {} from} msg) return;

		var chat = msg.Chat;
		raw = raw.Trim();

		var userId = from.Id;
		var username = from.Username ?? from.FirstName;
		var user = await _userService.GetUserAsync(userId, ct);

		var parts = raw.Split(' ', 2);
		var command = parts[0].TrimStart('/').ToLower();
		var argument = parts.Length > 1 ? parts[1] : "";

		string[] noRegCommands = ["start", "help", "info"];

		try {
			if (user is null && !noRegCommands.Contains(command)) {
				await SendMsgAsync($"Без регистрации вам доступны только команды {string.Join(", ", noRegCommands)}");
				return;
			}

			switch (command) {
				case "start":
					if (user is null) {
						user = await _userService.RegisterUserAsync(userId, username, ct);
						await SendMsgAsync($"Здравствуйте, {user.TelegramUserName}. Теперь вам доступно создание задач.");
					} else
						await SendMsgAsync($"Вы уже зарегистрированы: {user.TelegramUserName}");
					break;
				case "help":
					await SendMsgAsync("""
					                   Список всех команд:
					                   /start => регистрация.
					                   /help => вызывает эту справку.
					                   /info => пишет версию и дату создания этого бота.
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
					await SendMsgAsync("""
					                   Информация о программе:
					                   Версия: 0.5.0
					                   Дата создания: 29.09.2026
					                   Дата обновления: 19.11.2026
					                   """);
					break;
				case "addtask":
					if (string.IsNullOrWhiteSpace(argument)) {
						await SendMsgAsync("Описание задачи не может быть пустым.");
						return;
					}

					var newItem = await _toDoService.AddAsync(user!, argument, ct);
					await SendMsgAsync($"задача \"{newItem.Name}\" добавлена в список ({newItem.Id}).");
					break;
				case "showtasks":
					await RevealTasksAsync(await _toDoService.GetActiveByUserIdAsync(user!.UserId, ct), "активных задач", false);
					break;
				case "showalltasks":
					await RevealTasksAsync(await _toDoService.GetAllByUserIdAsync(user!.UserId, ct), "всех задач", true);
					break;
				case "completetask":
					if (!Guid.TryParse(argument, out var compId)) {
						await SendMsgAsync("Неверный формат Id.");
						return;
					}

					await _toDoService.MarkCompletedAsync(compId, ct);

					await SendMsgAsync($"Задача {compId} выполнена.");
					break;
				case "removetask":
					if (!Guid.TryParse(argument, out var remId)) {
						await SendMsgAsync("Неверный формат Id.");
						return;
					}

					await _toDoService.DeleteAsync(remId, ct);

					await SendMsgAsync($"Задача {remId} удалена.");
					break;
				case "report":
					var (total, completed, active, generatedAt) = await _toDoReportService.GetUserStatsAsync(user!.UserId, ct);
					await SendMsgAsync($"{generatedAt:dd.MM.yyyy HH:mm:ss}. Всего: {total}, завершённые: {completed}, активные: {active}.");
					break;
				case "find":
					if (string.IsNullOrWhiteSpace(argument)) {
						await SendMsgAsync("Префикс не может быть пустым.");
						return;
					}

					var foundTasks = await _toDoService.FindAsync(user!, argument, ct);
					await RevealTasksAsync(foundTasks, $"задач с префиксом {argument}", true);
					break;
				default:
					await SendMsgAsync("Неизвестная команда. Введите /help для получения справки.");
					break;
			}
		} catch (OperationCanceledException) {
			throw;
		} catch (Exception e) {
			await SendMsgAsync($"Ошибка: {e.Message}");
		}
		return;

		Task SendMsgAsync(string text, ParseMode parseMode = ParseMode.None) =>
			botClient.SendMessage(
				chat,
				text,
				parseMode: parseMode,
				replyMarkup: user is null ? StartKeyboard : RegKeyboard,
				cancellationToken: ct);

		async Task RevealTasksAsync(IReadOnlyList<ToDoItem> tasks, string title, bool state) {
			if (tasks.Count == 0) {
				await SendMsgAsync($"Список {title} пуст.");
				return;
			}

			var lines = tasks.Select((t, i) => {
				var statePart = state ? ClearMark($"({t.State}) ") : "";
				var namePart = ClearMark(t.Name);
				var datePart = ClearMark($"{t.CreatedAt:dd.MM.yyyy HH:mm:ss}");
				return $@"{i + 1}\. {statePart}{namePart} \- {datePart} \- `{t.Id}`";
			});

			await SendMsgAsync($"""
			                    Список {ClearMark(title)}:
			                    {string.Join("\n", lines)}
			                    """, ParseMode.MarkdownV2);
		}
	}

	public Task HandleErrorAsync(ITelegramBotClient botClient, Exception e, HandleErrorSource source, CancellationToken ct) {
		var color = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"Ошибка ({source}): {e.GetType().Name} - {e.Message}");
		Console.ForegroundColor = color;
		return Task.CompletedTask;
	}

	private static string ClearMark(string text) {
		ReadOnlySpan<char> specialChars = ['\\', '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'];
		var sb = new StringBuilder(text.Length);

		foreach (var ch in text) {
			if (specialChars.Contains(ch))
				sb.Append('\\');
			sb.Append(ch);
		}

		return sb.ToString();
	}
}
