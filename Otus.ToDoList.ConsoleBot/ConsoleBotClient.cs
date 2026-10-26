using Otus.ToDoList.ConsoleBot.Types;

namespace Otus.ToDoList.ConsoleBot;

/// <summary>
/// Консольный клиент для бота
/// </summary>
public class ConsoleBotClient : ITelegramBotClient {
	private readonly Chat _chat;
	private readonly User _user;

	public ConsoleBotClient() {
		_chat = new Chat {Id = Random.Shared.Next()}; // Генерируем чат с рандомным Id-шником
		_user = new User {Id = Random.Shared.Next(), Username = $"ConsoleUser_{Guid.NewGuid()}"}; // Генерируем пользователя который общается с ботом
	}

	public void SendMessage(Chat chat, string text) {
		ArgumentNullException.ThrowIfNull(chat, nameof(chat));
		ArgumentNullException.ThrowIfNull(text, nameof(text));
		if (_chat.Id != chat.Id)
			throw new ArgumentException($"Invalid chat.Id. Support {_chat.Id}, but was {chat.Id}");

		WriteLineColor($"Бот: {text}", ConsoleColor.Blue);
	}

	public void StartReceiving(IUpdateHandler handler) {
		ArgumentNullException.ThrowIfNull(handler, nameof(handler));

		var cts = new CancellationTokenSource(); // Дает возможность при завершении работы прервать запущенные асинхронные операции
		ConsoleCancelEventHandler cancelHandler = (sender, e) => // Обработчик выхода из консоли(по нажатию Ctrl+C)
		{
			cts.Cancel(); // Прерываем запущенные синхронные операции
			e.Cancel = true;
		};
		Console.CancelKeyPress += cancelHandler; // Добавляем обработчик выхода из консоли

		try {
			WriteLineColor("Бот запущен. Введите сообщение", ConsoleColor.Magenta);
			var counter = 0; // Счетчик полученных сообщений, который мы используем, как идентификатор для получаемого сообщения

			while (cts.IsCancellationRequested is false) // Пока не cts.Cancel(); (пока не нажали Ctrl+C)
			{
				var input = Console.ReadLine(); // Вводим в консоль - считываем сообщение в консольный бот
				if (input is null)
					break;

				var update = new Update // Обновление - которое генерирует консольный бот
				{
					Message = new Message // Сообщение - содержимое генерируемого обновления
					{
						Id = Interlocked.Increment(ref counter),
						Text = input,
						Chat = _chat,
						From = _user
					}
				};

				handler.HandleUpdateAsync(this, update); // Обрабатываем сгенерированное сообщение
			}
		} finally {
			Console.CancelKeyPress -= cancelHandler; // Очистка ресурсов, отписываемся от обработчика
			cts.Dispose(); // Очистка ресурсов
			WriteLineColor("Бот остановлен", ConsoleColor.Magenta);
		}
	}

	/// <summary>
	/// Вывод в консоль
	/// </summary>
	/// <param name="text">Текст , который нужно вывести</param>
	/// <param name="color">Цвет текста</param>
	private static void WriteLineColor(string text, ConsoleColor color) {
		var currentColor = Console.ForegroundColor;
		Console.ForegroundColor = color;
		Console.WriteLine(text);
		Console.ForegroundColor = currentColor;
	}
}