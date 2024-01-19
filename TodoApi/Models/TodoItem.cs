using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
namespace TodoApi.Models;

public class TodoItem
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}


public static class TodoItemEndpoints
{
	public static void MapTodoItemEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/TodoItem").WithTags(nameof(TodoItem));

        group.MapGet("/", async (TodoContext db) =>
        {
            return await db.TodoItems.ToListAsync();
        })
        .WithName("GetAllTodoItems")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<TodoItem>, NotFound>> (long id, TodoContext db) =>
        {
            return await db.TodoItems.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is TodoItem model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetTodoItemById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (long id, TodoItem todoItem, TodoContext db) =>
        {
            var affected = await db.TodoItems
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                  .SetProperty(m => m.Id, todoItem.Id)
                  .SetProperty(m => m.Name, todoItem.Name)
                  .SetProperty(m => m.IsComplete, todoItem.IsComplete)
                  );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateTodoItem")
        .WithOpenApi();

        group.MapPost("/", async (TodoItem todoItem, TodoContext db) =>
        {
            db.TodoItems.Add(todoItem);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/TodoItem/{todoItem.Id}",todoItem);
        })
        .WithName("CreateTodoItem")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (long id, TodoContext db) =>
        {
            var affected = await db.TodoItems
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteTodoItem")
        .WithOpenApi();
    }
}