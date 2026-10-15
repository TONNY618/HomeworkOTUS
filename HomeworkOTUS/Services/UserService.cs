using HomeworkOTUS.Models;

namespace HomeworkOTUS.Services;

public class UserService : IUserService {
	private readonly List<ToDoUser> _users = [];

	public ToDoUser RegisterUser(long telegramUserId, string telegramUserName) {
		var user = GetUser(telegramUserId);
		if (user != null)
			return user;

		user = new ToDoUser(telegramUserId, telegramUserName);
		_users.Add(user);
		return user;
	}

	public ToDoUser? GetUser(long telegramUserId) {
		return _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
	}
}