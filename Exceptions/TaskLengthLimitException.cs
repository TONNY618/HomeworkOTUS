namespace HomeworkOTUS.Exceptions;

public class TaskLengthLimitException(int taskLength, int taskLengthLimit)
	: Exception($"Описание задачи длиной {taskLength} больше максимально допустимого значения {taskLengthLimit}");