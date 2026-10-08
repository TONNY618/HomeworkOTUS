namespace HomeworkOTUS.Models;

public class ToDoItem {
	public Guid Id {get; init;}
	public ToDoUser User {get; init;}
	public string Name {get; init;}
	public DateTime CreatedAt {get; init;}
	public ToDoItemState State {get; private set;}
	public DateTime? StateChangedAt {get; private set;}

	public ToDoItem(ToDoUser user, string name) {
		Id = Guid.NewGuid();
		User = user;
		Name = name;
		CreatedAt = DateTime.UtcNow;
		State = ToDoItemState.Active;
		StateChangedAt = null;
	}

	public void Complete() {
		State = ToDoItemState.Completed;
		StateChangedAt = DateTime.UtcNow;
	}
}