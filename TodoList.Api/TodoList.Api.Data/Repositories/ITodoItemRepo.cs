using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Api.Data.Repositories
{
    public interface ITodoItemRepo
    {
        Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync();
        Task<TodoItem> GetTodoItemByIdAsync(Guid id);
        Task CreateTodoItemAsync(TodoItem todoItem);
        Task<bool> SaveChangesAsync();
        void UpdateTodoItem(TodoItem todoItem);
        void DeleteTodoItemById(TodoItem todoItem);
        void UpdateTodoItemProperty(JsonPatchDocument<TodoItem> todoItemUpdates, TodoItem todoItem);
    }
}
