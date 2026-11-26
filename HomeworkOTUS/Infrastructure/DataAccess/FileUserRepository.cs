using System.Text.Json;
using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Infrastructure.DataAccess;

public class FileUserRepository : IUserRepository {
	private static readonly JsonSerializerOptions JsonOptions = new() {WriteIndented = true};
	private readonly string _basePath;

	public FileUserRepository(string basePath) {
		ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
		_basePath = basePath;
		Directory.CreateDirectory(_basePath);
	}

	public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct = default) {
		var path = GetUserPath(userId);
		if (!File.Exists(path))
			return null;

		await using var stream = File.OpenRead(path);
		return await JsonSerializer.DeserializeAsync<ToDoUser>(stream, JsonOptions, ct);
	}

	public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default) {
		foreach (var file in Directory.EnumerateFiles(_basePath, "*.json")) {
			ct.ThrowIfCancellationRequested();

			await using var stream = File.OpenRead(file);
			var user = await JsonSerializer.DeserializeAsync<ToDoUser>(stream, JsonOptions, ct);

			if (user?.TelegramUserId == telegramUserId)
				return user;
		}

		return null;
	}

	public async Task AddAsync(ToDoUser user, CancellationToken ct = default) {
		var path = GetUserPath(user.UserId);
		await using var stream = File.Create(path);
		await JsonSerializer.SerializeAsync(stream, user, JsonOptions, ct);
	}

	private string GetUserPath(Guid userId) =>
		Path.Combine(_basePath, $"{userId}.json");
}
