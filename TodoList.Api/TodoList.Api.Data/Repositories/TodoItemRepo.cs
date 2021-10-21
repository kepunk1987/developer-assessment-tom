using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Api.Data.Repositories
{
    public class TodoItemRepo : ITodoItemRepo
    {
        private readonly TodoContext _todoContext;

        public TodoItemRepo(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }

        public async Task CreateTodoItemAsync(TodoItem todoItem)
        {
            if (todoItem == null)
            {
                throw new ArgumentNullException(nameof(todoItem));
            }
            await _todoContext.AddAsync(todoItem);
        }

        public void DeleteTodoItemById(TodoItem todoItem)
        {
            if (todoItem == null)
            {
                throw new ArgumentNullException(nameof(todoItem));
            }
            _todoContext.Entry(todoItem).State = EntityState.Deleted;
        }

        public async Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync()
        {
            return await _todoContext.TodoItems.ToListAsync();
        }

        public async Task<TodoItem> GetTodoItemByIdAsync(Guid id)
        {
            return await _todoContext.TodoItems.FirstOrDefaultAsync(t => t.Id.Equals(id));
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _todoContext.SaveChangesAsync() >= 0;
        }

        public void UpdateTodoItem(TodoItem todoItem)
        {
            if (todoItem == null)
            {
                throw new ArgumentNullException(nameof(todoItem));
            }
            _todoContext.Entry(todoItem).State = EntityState.Modified;
        }

        public void UpdateTodoItemProperty(JsonPatchDocument<TodoItem> todoItemUpdates, TodoItem todoItem)
        {
            if (todoItem == null)
            {
                throw new ArgumentNullException(nameof(todoItem));
            }
            if (todoItemUpdates == null)
            {
                throw new ArgumentNullException(nameof(todoItemUpdates));
            }
            todoItemUpdates.ApplyTo(todoItem);
        }
    }
}
