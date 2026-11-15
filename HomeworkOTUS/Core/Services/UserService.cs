using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Core.Services;

public class UserService : IUserService {
	private readonly IUserRepository _userRepository;

	public UserService(IUserRepository userRepository) {
		_userRepository = userRepository;
	}

	public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct = default) {
		var user = await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
		if (user is not null)
			return user;

		user = new ToDoUser(telegramUserId, telegramUserName);
		await _userRepository.AddAsync(user, ct);
		return user;
	}

	public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct = default) {
		return await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
	}
}
