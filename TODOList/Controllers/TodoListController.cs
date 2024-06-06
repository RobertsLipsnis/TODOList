using Microsoft.AspNetCore.Mvc;
using TODOList.Helpers;
using TODOList.Models;
using TODOList.Models.ViewModels;

namespace TODOList.Controllers;

public class TodoListController : Controller
{
    private readonly TodoControllerHelpers _helper;

    public TodoListController(TodoControllerHelpers helper)
    {
        _helper = helper;
    }

    public IActionResult Index()
    {
        var todoViewModel = GetAllTodoItems();
        return View(todoViewModel);
    }

    public RedirectResult Create(TodoModel todo)
    {
        _ = _helper.CreateAsync(todo);
        return Redirect(Request.Headers["Referer"].ToString());
    }

    internal TodoViewModel GetAllTodoItems()
    {
        return _helper.GetAllTodoItems().Result;
    }

    internal TodoModel GetById(int id)
    {
        return _helper.GetByIdAsync(id).Result;
    }

    public RedirectResult Update(TodoModel todo)
    {
        _ = _helper.UpdateAsync(todo);
        return Redirect(Request.Headers["Referer"].ToString());
    }

    [HttpPost]
    public JsonResult Delete(int id)
    {
        _ = _helper.DeleteAsync(id);
        return Json(new { });
    }

    [HttpGet]
    public JsonResult PopulateForm(int id)
    {
        var todo = GetById(id);
        return Json(todo);
    }

    [HttpPost]
    public JsonResult SetDone(int id, bool isDone)
    {
        _ = _helper.SetDoneAsync(id, isDone);
        return Json(new { });
    }
}
