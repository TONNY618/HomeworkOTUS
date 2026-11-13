using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Core.Services;

public interface IUserService {
	Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct = default);
	Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct = default);
}
