using System.Text;
using Microsoft.Data.Sqlite;
using TODOList.Models;
using TODOList.Models.ViewModels;
namespace TODOList.Helpers;

public class TodoControllerHelpers
{
    private const string ConnectionString = "Data Source=todolistdb.sqlite";
    private readonly ILogger<TodoControllerHelpers> _logger;

    public TodoControllerHelpers(ILogger<TodoControllerHelpers> logger)
    {
        _logger = logger;
    }

    public async Task CreateAsync(TodoModel todo)
    {
        // Ensure table exists
        await EnsureTableExistsAsync();

        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = "INSERT INTO TodoModel (Title, Body, IsDone, CreatedAt) VALUES (@Title, @Body, @IsDone, @CreatedAt)";
        tableCmd.Parameters.AddWithValue("@Title", todo.Title);
        tableCmd.Parameters.AddWithValue("@Body", todo.Body ?? "");
        tableCmd.Parameters.AddWithValue("@IsDone", false);
        tableCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
        try
        {
            await tableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create a todo item");
        }
    }

    private async Task EnsureTableExistsAsync()
    {
        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS TodoModel (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Body TEXT,
                    IsDone INTEGER,
                    CreatedAt DATETIME NOT NULL,
                    UpdatedAt DATETIME,
                    CompletedAt DATETIME
                )";

        try
        {
            await tableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure TodoModel table exists");
        }
    }

    public async Task<TodoViewModel> GetAllTodoItems()
    {
        List<TodoModel> todoList = new();

        // Ensure table exists
        if (!await TableExistsAsync())
        {
            _logger.LogError("TodoModel table does not exist");
            return new TodoViewModel
            {
                TodoList = new List<TodoModel>()
            };
        }

        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = "SELECT * FROM TodoModel";

        using var reader = await tableCmd.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                todoList.Add(new TodoModel
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Body = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    IsDone = !reader.IsDBNull(3) && reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    CompletedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                });
            }
        }

        con.Close();

        var sortedTodoList = todoList
            .OrderBy(todo => todo.IsDone)
            .ThenBy(todo => todo.CompletedAt)
            .ThenBy(todo => todo.UpdatedAt)
            .ThenBy(todo => todo.CreatedAt)
            .ToList();

        return new TodoViewModel
        {
            TodoList = sortedTodoList
        };
    }

    private async Task<bool> TableExistsAsync()
    {
        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TodoModel'";

        using var reader = await tableCmd.ExecuteReaderAsync();
        return reader.HasRows;
    }

    public async Task<TodoModel> GetByIdAsync(int id)
    {
        TodoModel todo = new();

        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = $"SELECT * FROM TodoModel Where Id = '{id}'";

        using var reader = await tableCmd.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            await reader.ReadAsync();
            todo.Id = reader.GetInt32(0);
            todo.Title = reader.GetString(1);
            todo.Body = reader.IsDBNull(2) ? "" : reader.GetString(2);
            todo.IsDone = !reader.IsDBNull(3) && reader.GetBoolean(3);
            todo.CreatedAt = reader.GetDateTime(4);
            todo.UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5);
            todo.CompletedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6);
        }

        con.Close();

        return todo;
    }

    public async Task UpdateAsync(TodoModel todo)
    {
        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();

        StringBuilder queryBuilder = new("UPDATE TodoModel SET ");
        List<SqliteParameter> parameters = new();

        if (!string.IsNullOrEmpty(todo.Title))
        {
            queryBuilder.Append("Title = @Title, ");
            parameters.Add(new SqliteParameter("@Title", todo.Title));
        }

        if (!string.IsNullOrEmpty(todo.Body))
        {
            queryBuilder.Append("Body = @Body, ");
            parameters.Add(new SqliteParameter("@Body", todo.Body));
        }

        queryBuilder.Append("UpdatedAt = @UpdatedAt ");
        parameters.Add(new SqliteParameter("@UpdatedAt", DateTime.Now));
        queryBuilder.Append("WHERE Id = @Id");
        parameters.Add(new SqliteParameter("@Id", todo.Id));

        tableCmd.CommandText = queryBuilder.ToString();

        foreach (var parameter in parameters)
        {
            tableCmd.Parameters.Add(parameter);
        }

        try
        {
            await tableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update a todo item: ");
        }

        con.Close();
    }

    public async Task DeleteAsync(int id)
    {
        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = $"DELETE from TodoModel WHERE Id = '{id}'";
        try
        {
            await tableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete a todo item: ");
        }

        con.Close();
    }

    public async Task SetDoneAsync(int id, bool isDone)
    {
        using var con = GetOpenConnection();

        using var tableCmd = con.CreateCommand();
        tableCmd.CommandText = $"UPDATE TodoModel SET IsDone = @IsDone, CompletedAt = @CompletedAt WHERE Id = @Id";
        tableCmd.Parameters.AddWithValue("@IsDone", isDone);
        tableCmd.Parameters.AddWithValue("@CompletedAt", isDone ? (object)DateTime.Now : DBNull.Value);
        tableCmd.Parameters.AddWithValue("@Id", id);
        try
        {
            await tableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set a todo item as done: ");
        }
        finally
        {
            con.Close();
        }
    }

    private static SqliteConnection GetOpenConnection()
    {
        var con = new SqliteConnection(ConnectionString);
        con.Open();
        return con;
    }
}

