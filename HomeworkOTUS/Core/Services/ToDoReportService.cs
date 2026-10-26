using HomeworkOTUS.Core.DataAccess;
using HomeworkOTUS.Core.Entities;

namespace HomeworkOTUS.Core.Services;

public class ToDoReportService : IToDoReportService {
	private readonly IToDoRepository _toDoRepository;

	public ToDoReportService(IToDoRepository toDoRepository) {
		_toDoRepository = toDoRepository;
	}

	public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId) {
		var tasks = _toDoRepository.GetAllByUserId(userId);

		return (total: tasks.Count,
			completed: tasks.Count(t => t.State == ToDoItemState.Completed),
			active: tasks.Count(t => t.State == ToDoItemState.Active),
			generatedAt: DateTime.UtcNow);
	}
}