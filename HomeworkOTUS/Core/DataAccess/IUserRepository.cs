using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Core.DataAccess;

public interface IUserRepository {
	Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct = default);
	Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default);
	Task AddAsync(ToDoUser user, CancellationToken ct = default);
}
