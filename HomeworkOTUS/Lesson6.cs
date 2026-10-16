using HomeworkOTUS.Handlers;
using HomeworkOTUS.Services;
using Otus.ToDoList.ConsoleBot;

namespace HomeworkOTUS;

internal static class Lesson6 {
	private static void Main() {
		try {
			var userService = new UserService();
			var toDoService = new ToDoService();

			var updateHandler = new UpdateHandler(userService, toDoService);
			var botClient = new ConsoleBotClient();

			botClient.StartReceiving(updateHandler);
		} catch (Exception e) {
			Console.WriteLine($"Ошибка: {e.Message}");
		}
	}
}