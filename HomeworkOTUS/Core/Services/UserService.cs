using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Core.Services;

public class UserService : IUserService {
	private readonly IUserRepository _userRepository;

	public UserService(IUserRepository userRepository) {
		_userRepository = userRepository;
	}

	public ToDoUser RegisterUser(long telegramUserId, string telegramUserName) {
		var user = _userRepository.GetUserByTelegramUserId(telegramUserId);
		if (user is not null)
			return user;

		user = new ToDoUser(telegramUserId, telegramUserName);
		_userRepository.Add(user);
		return user;
	}

	public ToDoUser? GetUser(long telegramUserId) {
		return _userRepository.GetUserByTelegramUserId(telegramUserId);
	}
}