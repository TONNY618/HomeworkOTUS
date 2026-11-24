using System.Text.Json;
using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Infrastructure.DataAccess;

public class FileToDoRepository : IToDoRepository {
	private static readonly JsonSerializerOptions JsonOptions = new() {WriteIndented = true};
	private readonly string _basePath;
	private readonly string _indexPath;

	public FileToDoRepository(string basePath) {
		ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
		_basePath = basePath;
		_indexPath = Path.Combine(_basePath, "index.json");

		Directory.CreateDirectory(_basePath);
	}

	public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default) {
		var userDir = GetUserDir(userId);
		if (!Directory.Exists(userDir))
			return [];

		List<ToDoItem> items = [];

		foreach (var file in Directory.EnumerateFiles(userDir, "*.json")) {
			ct.ThrowIfCancellationRequested();

			await using var stream = File.OpenRead(file);
			var item = await JsonSerializer.DeserializeAsync<ToDoItem>(stream, JsonOptions, ct);

			if (item is not null)
				items.Add(item);
		}

		return items;
	}

	public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default) {
		var items = await GetAllByUserIdAsync(userId, ct);

		return [.. items.Where(t => t.State == ToDoItemState.Active)];
	}

	public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct = default) {
		var index = await LoadIndexAsync(ct);
		if (!index.TryGetValue(id, out var userId))
			return null;

		var path = GetItemPath(userId, id);
		if (!File.Exists(path))
			return null;

		await using var stream = File.OpenRead(path);
		return await JsonSerializer.DeserializeAsync<ToDoItem>(stream, JsonOptions, ct);
	}

	public async Task AddAsync(ToDoItem item, CancellationToken ct = default) {
		Directory.CreateDirectory(GetUserDir(item.User.UserId));

		var path = GetItemPath(item.User.UserId, item.Id);
		await using (var stream = File.Create(path)) {
			await JsonSerializer.SerializeAsync(stream, item, JsonOptions, ct);
		}

		var index = await LoadIndexAsync(ct);
		index[item.Id] = item.User.UserId;
		await SaveIndexAsync(index, ct);
	}

	public async Task UpdateAsync(ToDoItem item, CancellationToken ct = default) {
		var path = GetItemPath(item.User.UserId, item.Id);
		if (!File.Exists(path))
			return;

		await using var stream = File.Create(path);
		await JsonSerializer.SerializeAsync(stream, item, JsonOptions, ct);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default) {
		var index = await LoadIndexAsync(ct);
		if (!index.TryGetValue(id, out var userId))
			return;

		var path = GetItemPath(userId, id);
		if (File.Exists(path))
			File.Delete(path);

		index.Remove(id);
		await SaveIndexAsync(index, ct);
	}

	public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct = default) {
		var items = await GetAllByUserIdAsync(userId, ct);

		return items.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
	}

	public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct = default) {
		var items = await GetAllByUserIdAsync(userId, ct);

		return items.Count(t => t.State == ToDoItemState.Active);
	}

	public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> pred, CancellationToken ct = default) {
		var items = await GetAllByUserIdAsync(userId, ct);

		return [.. items.Where(pred)];
	}

	private string GetUserDir(Guid userId) =>
		Path.Combine(_basePath, userId.ToString());

	private string GetItemPath(Guid userId, Guid itemId) =>
		Path.Combine(GetUserDir(userId), $"{itemId}.json");

	private async Task<Dictionary<Guid, Guid>> RebuildIndexAsync(CancellationToken ct) {
		Dictionary<Guid, Guid> index = [];

		foreach (var dir in Directory.EnumerateDirectories(_basePath)) {
			foreach (var file in Directory.EnumerateFiles(dir, "*.json")) {
				ct.ThrowIfCancellationRequested();

				await using var stream = File.OpenRead(file);
				var item = await JsonSerializer.DeserializeAsync<ToDoItem>(stream, JsonOptions, ct);

				if (item is not null)
					index[item.Id] = item.User.UserId;
			}
		}

		await SaveIndexAsync(index, ct);
		return index;
	}

	private async Task<Dictionary<Guid, Guid>> LoadIndexAsync(CancellationToken ct) {
		if (!File.Exists(_indexPath))
			return await RebuildIndexAsync(ct);

		await using var stream = File.OpenRead(_indexPath);
		return await JsonSerializer.DeserializeAsync<Dictionary<Guid, Guid>>(stream, JsonOptions, ct) ?? [];
	}

	private async Task SaveIndexAsync(Dictionary<Guid, Guid> index, CancellationToken ct) {
		await using var stream = File.Create(_indexPath);
		await JsonSerializer.SerializeAsync(stream, index, JsonOptions, ct);
	}
}
