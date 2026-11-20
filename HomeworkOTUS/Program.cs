using HomeworkOTUS.Core.Services;
using HomeworkOTUS.Infrastructure.DataAccess;
using HomeworkOTUS.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeworkOTUS;

internal static class Program {
	private static async Task Main() {
		var token = Environment.GetEnvironmentVariable("OTUS_HOMEWORK_TONNY_TOKEN");
		if (token is null) {
			Console.WriteLine("Токен бота не найден.");
			return;
		}

		using var cts = new CancellationTokenSource();

		ConsoleCancelEventHandler cancelHandler = (_, e) => {
			e.Cancel = true;
			// ReSharper disable once AccessToDisposedClosure
			cts.Cancel();
		};

		Console.CancelKeyPress += cancelHandler;

		try {
			var userRepository = new InMemoryUserRepository();
			var toDoRepository = new InMemoryToDoRepository();

			var userService = new UserService(userRepository);
			var toDoService = new ToDoService(toDoRepository);
			var reportService = new ToDoReportService(toDoRepository);

			var updateHandler = new UpdateHandler(userService, toDoService, reportService);
			var botClient = new TelegramBotClient(token);

			BotCommand[] commands = [
				new() {Command = "start", Description = "Регистрация"},
				new() {Command = "help", Description = "Справка по командам"},
				new() {Command = "info", Description = "Информация о боте"},
				new() {Command = "addtask", Description = "Создать задачу"},
				new() {Command = "showtasks", Description = "Показать активные задачи"},
				new() {Command = "showalltasks", Description = "Показать все задачи"},
				new() {Command = "completetask", Description = "Выполнить задачу по Id"},
				new() {Command = "removetask", Description = "Удалить задачу по Id"},
				new() {Command = "report", Description = "Статистика по задачам"},
				new() {Command = "find", Description = "Найти задачи с префиксом"},
			];
			await botClient.SetMyCommands(commands, cancellationToken: cts.Token);

			var receiver = new ReceiverOptions {
				AllowedUpdates = [UpdateType.Message],
				DropPendingUpdates = true,
			};

			botClient.StartReceiving(updateHandler, receiver, cts.Token);

			var me = await botClient.GetMe(cts.Token);
			Console.WriteLine($"{me.FirstName} запущен!");

			while (!cts.IsCancellationRequested) {
				Console.WriteLine("Нажмите клавишу A для выхода");
				var key = Console.ReadKey(intercept: true);

				if (key.Key == ConsoleKey.A) {
					await cts.CancelAsync();
					break;
				}

				me = await botClient.GetMe(cts.Token);
				Console.WriteLine($"Бот: Id = {me.Id}, Имя = {me.FirstName}, Username = @{me.Username}");
			}
		} catch (Exception e) {
			Console.WriteLine($"Ошибка: {e.Message}");
		} finally {
			Console.CancelKeyPress -= cancelHandler;
		}
	}
}
