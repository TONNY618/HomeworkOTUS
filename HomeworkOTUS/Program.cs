using HomeworkOTUS.Core.Services;
using HomeworkOTUS.Infrastructure.DataAccess;
using HomeworkOTUS.TelegramBot;
using Otus.ToDoList.ConsoleBot;

namespace HomeworkOTUS;

internal static class Program {
	private static void Main() {
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
			var botClient = new ConsoleBotClient();

			botClient.StartReceiving(updateHandler, cts.Token);
		} catch (Exception e) {
			Console.WriteLine($"Ошибка: {e.Message}");
		} finally {
			Console.CancelKeyPress -= cancelHandler;
		}
	}
}
