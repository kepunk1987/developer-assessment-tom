using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Api.Data.Repositories;

namespace TodoList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemRepo _todoItemRepo;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(ITodoItemRepo todoItemRepo, ILogger<TodoItemsController> logger)
        {
            _todoItemRepo = todoItemRepo;
            _logger = logger;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<IActionResult> GetInCompletedTodoItems()
        {
            var todoItems = await _todoItemRepo.GetAllTodoItemsAsync();
            var results = todoItems.Where(x => !x.IsCompleted).ToList();
            if (!results.Any())
            {
                return NotFound("No TodoItems found.");
            }
            return Ok(results);
        }

        // GET: api/TodoItems/...
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItem([FromRoute] Guid id)
        {
            var result = await _todoItemRepo.GetTodoItemByIdAsync(id);

            if (result == null)
            {
                return NotFound($"No TodoItem with id {id} found.");
            }

            return Ok(result);
        }

        // PUT: api/TodoItems/... 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem([FromRoute] Guid id, [FromBody] TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            try
            {
                _todoItemRepo.UpdateTodoItem(todoItem);
                await _todoItemRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (await TodoItemIdExistsAsync(id) == false)
                {
                    return NotFound($"No TodoItem with id {id} found.");
                }
                else
                {
                    _logger.LogError($"Error while updating TodoItems with id {id}", ex);
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        //PATCH: api/TodoItems/... 
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTodoItem([FromRoute] Guid id, JsonPatchDocument<TodoItem> todoItemUpdates)
        {
            var todoItem = await _todoItemRepo.GetTodoItemByIdAsync(id);

            if (todoItem == null)
            {
                return NotFound($"No TodoItem with id {id} found.");
            }
            try
            {
                _todoItemRepo.UpdateTodoItemProperty(todoItemUpdates, todoItem);
                await _todoItemRepo.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (await TodoItemIdExistsAsync(id) == false)
                {
                    return NotFound($"No TodoItem with id {id} found.");
                }
                else
                {
                    _logger.LogError($"Error while updating TodoItems with id {id}", ex);
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // POST: api/TodoItems 
        [HttpPost]
        public async Task<IActionResult> PostTodoItem([FromBody] TodoItem todoItem)
        {
            if (await TodoItemIdExistsAsync(todoItem.Id))
            {
                return BadRequest($"Id {todoItem.Id} already exists");
            }
            if (await IncompletedTodoItemDescriptionExistsAsync(todoItem.Description))
            {
                return BadRequest($"Description {todoItem.Description} already exists");
            }

            try
            {
                await _todoItemRepo.CreateTodoItemAsync(todoItem);
                await _todoItemRepo.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"Error while creating TodoItems with id {todoItem.Id}", ex);
                throw;
            }

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        //DELETE: api/TodoItems/... 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem([FromRoute] Guid id)
        {
            var todoItem = await _todoItemRepo.GetTodoItemByIdAsync(id);

            if (todoItem == null)
            {
                return NotFound($"No TodoItem with id {id} found.");
            }

            try
            {
                _todoItemRepo.DeleteTodoItemById(todoItem);
                await _todoItemRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (await TodoItemIdExistsAsync(id) == false)
                {
                    return NotFound($"No TodoItem with id {id} found.");
                }
                else
                {
                    _logger.LogError($"Error while deletng TodoItems with id {id}", ex);
                    throw;
                }
            }

            return Ok();
        }

        private async Task<bool> TodoItemIdExistsAsync(Guid id)
        {
            var todoItems = await _todoItemRepo.GetAllTodoItemsAsync();
            return todoItems.Any(x => x.Id == id);
        }

        private async Task<bool> IncompletedTodoItemDescriptionExistsAsync(string description)
        {
            var todoItems = await _todoItemRepo.GetAllTodoItemsAsync();
            return todoItems
                   .Any(x => x.Description.ToLowerInvariant() == description.ToLowerInvariant() && !x.IsCompleted);
        }
    }
}
