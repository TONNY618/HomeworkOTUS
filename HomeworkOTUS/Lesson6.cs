using HomeworkOTUS.Core.Services;
using HomeworkOTUS.Infrastructure.DataAccess;
using HomeworkOTUS.TelegramBot;
using Otus.ToDoList.ConsoleBot;

namespace HomeworkOTUS;

internal static class Lesson6 {
	private static void Main() {
		try {
			var userRepository = new InMemoryUserRepository();
			var toDoRepository = new InMemoryToDoRepository();

			var userService = new UserService(userRepository);
			var toDoService = new ToDoService(toDoRepository);
			var reportService = new ToDoReportService(toDoRepository);

			var updateHandler = new UpdateHandler(userService, toDoService, reportService);
			var botClient = new ConsoleBotClient();

			botClient.StartReceiving(updateHandler);
		} catch (Exception e) {
			Console.WriteLine($"Ошибка: {e.Message}");
		}
	}
}