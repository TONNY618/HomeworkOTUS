using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository {
	private readonly List<ToDoUser> _users = [];

	public ToDoUser? GetUser(Guid userId) {
		return _users.FirstOrDefault(u => u.UserId == userId);
	}

	public ToDoUser? GetUserByTelegramUserId(long telegramUserId) {
		return _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
	}

	public void Add(ToDoUser user) {
		_users.Add(user);
	}
}