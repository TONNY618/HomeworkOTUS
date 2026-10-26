namespace HomeworkOTUS.Core.Exceptions;

public class DuplicateTaskException(string task)
	: Exception($"Задача {task} уже существует");