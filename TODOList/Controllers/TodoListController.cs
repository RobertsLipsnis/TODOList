using Microsoft.AspNetCore.Mvc;

namespace TODOList.Controllers;

public class TodoListController : Controller
{
    private readonly ILogger<TodoListController> _logger;

    public TodoListController(ILogger<TodoListController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}
