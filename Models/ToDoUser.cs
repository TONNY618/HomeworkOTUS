namespace HomeworkOTUS.Models;

public class ToDoUser {
	public Guid UserId {get; init;}
	public string TelegramUserName {get; init;}
	public DateTime RegisteredAt {get; init;}

	public ToDoUser(string telegramUserName) {
		UserId = Guid.NewGuid();
		TelegramUserName = telegramUserName;
		RegisteredAt = DateTime.UtcNow;
	}
}