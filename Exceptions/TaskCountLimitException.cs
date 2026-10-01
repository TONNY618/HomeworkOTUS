namespace HomeworkOTUS.Exceptions;

public class TaskCountLimitException(int taskCountLimit)
	: Exception($"Количество задач больше максимума, равного {taskCountLimit}");