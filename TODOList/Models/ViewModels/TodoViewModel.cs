namespace TODOList.Models.ViewModels
{
    public class TodoViewModel
    {
        public List<TodoModel> TodoList { get; set; }
        public TodoModel Todo { get; set; }

        public TodoViewModel()
        {
            TodoList = new List<TodoModel>();
            Todo = new TodoModel();
        }
    }
}
