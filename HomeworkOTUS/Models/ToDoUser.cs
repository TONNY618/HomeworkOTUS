namespace HomeworkOTUS.Models;

public class ToDoUser {
	public Guid UserId {get; init;}
	public long TelegramUserId {get; init;}
	public string TelegramUserName {get; init;}
	public DateTime RegisteredAt {get; init;}

	public ToDoUser(long telegramUserId, string telegramUserName) {
		UserId = Guid.NewGuid();
		TelegramUserId = telegramUserId;
		TelegramUserName = telegramUserName;
		RegisteredAt = DateTime.UtcNow;
	}
}