using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Core.Services;

public class ToDoReportService : IToDoReportService {
	private readonly IToDoRepository _toDoRepository;

	public ToDoReportService(IToDoRepository toDoRepository) {
		_toDoRepository = toDoRepository;
	}

	public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken ct = default) {
		var tasks = await _toDoRepository.GetAllByUserIdAsync(userId, ct);

		return (total: tasks.Count,
			completed: tasks.Count(t => t.State == ToDoItemState.Completed),
			active: tasks.Count(t => t.State == ToDoItemState.Active),
			generatedAt: DateTime.UtcNow);
	}
}
