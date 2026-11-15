using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository {
	private readonly List<ToDoUser> _users = [];

	public Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult(_users.FirstOrDefault(u => u.UserId == userId));
	}

	public Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		return Task.FromResult(_users.FirstOrDefault(u => u.TelegramUserId == telegramUserId));
	}

	public Task AddAsync(ToDoUser user, CancellationToken ct = default) {
		ct.ThrowIfCancellationRequested();
		_users.Add(user);
		return Task.CompletedTask;
	}
}
