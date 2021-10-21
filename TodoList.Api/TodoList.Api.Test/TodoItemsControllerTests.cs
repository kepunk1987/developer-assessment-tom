using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Api.Controllers;
using TodoList.Api.Data.Repositories;

namespace TodoList.Api.Test
{
    [TestClass]
    public class TodoItemsControllerTests
    {
        [TestMethod]
        public async Task GetTodoListShouldReturnIncompletedOnes()
        {
            //arrange
            var guid = Guid.NewGuid();
            var todoItemList = new List<TodoItem>()
            {
                new TodoItem()
                {
                    Id = guid,
                    Description = "test1",
                    IsCompleted = false
                },
                new TodoItem()
                {
                    Id = Guid.NewGuid(),
                    Description = "test2",
                    IsCompleted = true,
                    CompletedTime = DateTime.Now
                },
            };
            var expectedTodoItemList = new List<TodoItem>()
            {
                new TodoItem()
                {
                    Id = guid,
                    Description = "test1",
                    IsCompleted = false
                }
            };
            var todoItemRepoMock = new Mock<ITodoItemRepo>();
            todoItemRepoMock.Setup(t => t.GetAllTodoItemsAsync()).Returns(Task.Run(() => todoItemList.AsEnumerable()));

            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepoMock.Object, loggerMock.Object);
            //act
            var result = await controller.GetInCompletedTodoItems();
            var okResult = result as OkObjectResult;
            //assert
            Assert.AreEqual(expectedTodoItemList.FirstOrDefault().Id, (okResult.Value as List<TodoItem>).FirstOrDefault().Id);
            Assert.AreEqual(1, (okResult.Value as List<TodoItem>).Count());
        }

        [TestMethod]
        public async Task GetTodoItemShouldReturnTheCorrectOne()
        {
            //arrange
            var guid = Guid.NewGuid();
            var todoItem = new TodoItem()
            {
                Id = guid,
                Description = "test3",
                IsCompleted = false
            };
            var todoItemRepoMock = new Mock<ITodoItemRepo>();
            todoItemRepoMock.Setup(t => t.GetTodoItemByIdAsync(It.IsAny<Guid>())).Returns(Task.Run(() => todoItem));

            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepoMock.Object, loggerMock.Object);
            //act
            var result = await controller.GetTodoItem(guid);
            var okResult = result as OkObjectResult;
            //assert
            Assert.AreEqual(todoItem, okResult.Value as TodoItem);
        }

        [TestMethod]
        public async Task GetTodoItemShouldReturnErrorIfNotFound()
        {
            //arrange
            var guid = Guid.NewGuid();
            var todoItem = new TodoItem()
            {
                Id = guid,
                Description = "test3",
                IsCompleted = false
            };
            var todoItemRepoMock = new Mock<ITodoItemRepo>();
            todoItemRepoMock.Setup(t => t.GetTodoItemByIdAsync(It.IsAny<Guid>())).Returns(Task.Run(() => null as TodoItem));

            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepoMock.Object, loggerMock.Object);
            //act
            var result = await controller.GetTodoItem(guid);
            var notFoundObjectResult = result as NotFoundObjectResult;
            //assert
            Assert.AreEqual($"No TodoItem with id {guid} found.", notFoundObjectResult.Value.ToString());
        }

        [TestMethod]
        public async Task PutTodoItemShouldUpdateItem()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var postResult = await controller.PostTodoItem(new TodoItem { Description = "test1", Id = guid, IsCompleted = false });
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            var todoItem = new TodoItem { Description = "test2", Id = guid, IsCompleted = false };
            var result = await controller.PutTodoItem(guid, todoItem);
            var okResult = result as CreatedAtActionResult;
            //assert
            Assert.AreEqual("test2", (okResult.Value as TodoItem).Description);
        }

        [TestMethod]
        public async Task PutTodoItemShouldReturnBadRequestIfIdNotMatched()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var postResult = await controller.PostTodoItem(new TodoItem { Description = "test1", Id = guid, IsCompleted = false });
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            var todoItem = new TodoItem { Description = "test2", Id = guid, IsCompleted = false };
            var result = await controller.PutTodoItem(Guid.NewGuid(), todoItem);
            //assert
            Assert.IsTrue(result is BadRequestResult);
        }

        [TestMethod]
        public async Task PatchTodoItemShouldUpdateProperty()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var postResult = await controller.PostTodoItem(new TodoItem { Description = "test1", Id = guid, IsCompleted = false });
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            //var todoItem = new TodoItem { Description = "test2", Id = guid, IsCompleted = false };
            var todoItemUpdate = new JsonPatchDocument<TodoItem>();
            todoItemUpdate.Replace(t => t.Description, "test3");
            var result = await controller.PatchTodoItem(guid, todoItemUpdate);
            var okResult = result as CreatedAtActionResult;
            //assert
            Assert.AreEqual("test3", (okResult.Value as TodoItem).Description);
        }

        [TestMethod]
        public async Task PatchTodoItemShouldReturnErrorIfNotFound()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var postResult = await controller.PostTodoItem(new TodoItem { Description = "test1", Id = guid, IsCompleted = false });
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            var todoItemUpdate = new JsonPatchDocument<TodoItem>();
            todoItemUpdate.Replace(t => t.Description, "test3");
            var expectedGuid = Guid.NewGuid();
            var result = await controller.PatchTodoItem(expectedGuid, todoItemUpdate);
            var notFoundObjectResult = result as NotFoundObjectResult;
            //assert
            Assert.AreEqual($"No TodoItem with id {expectedGuid} found.", notFoundObjectResult.Value.ToString());
        }

        [TestMethod]
        public async Task PostTodoItemShouldCreateItem()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var itemToCreate = new TodoItem { Description = "test1", Id = guid, IsCompleted = false };
            var postResult = await controller.PostTodoItem(itemToCreate);
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            var result = await controller.GetTodoItem(trackedTodoItem.Id);
            var expectedItem = (result as OkObjectResult).Value as TodoItem;
            //assert
            Assert.AreEqual(expectedItem.Id, trackedTodoItem.Id);
        }

        [TestMethod]
        public async Task PostTodoItemShouldReturnErrorIfIdAlreadyExist()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var itemToCreate = new TodoItem { Description = "test1", Id = guid, IsCompleted = false };
            var postResult = await controller.PostTodoItem(itemToCreate);
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            var result = await controller.PostTodoItem(itemToCreate);
            //assert
            Assert.AreEqual($"Id {guid} already exists", (result as BadRequestObjectResult).Value.ToString());
        }

        [TestMethod]
        public async Task PostTodoItemShouldReturnErrorIfDescriptionAlreadyExist()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var itemToCreate = new TodoItem { Description = "test1", Id = guid, IsCompleted = false };
            var postResult = await controller.PostTodoItem(itemToCreate);
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            var result = await controller.PostTodoItem(new TodoItem { Description = "test1", Id = Guid.NewGuid(), IsCompleted = false });
            //assert
            Assert.AreEqual("Description test1 already exists", (result as BadRequestObjectResult).Value.ToString());
        }

        [TestMethod]
        public async Task DeleteTodoItemShouldRemoveItemFromDb()
        {
            //arrange
            using var context = new TodoContext(CreateNewContextOptions());
            var todoItemRepo = new TodoItemRepo(context);
            var guid = Guid.NewGuid();
            var loggerMock = new Mock<ILogger<TodoItemsController>>();
            var controller = new TodoItemsController(todoItemRepo, loggerMock.Object);
            var itemToCreate = new TodoItem { Description = "test1", Id = guid, IsCompleted = false };
            var postResult = await controller.PostTodoItem(itemToCreate);
            var trackedTodoItem = (postResult as CreatedAtActionResult).Value as TodoItem;
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            //act
            await controller.DeleteTodoItem(trackedTodoItem.Id);
            context.Entry(trackedTodoItem).State = EntityState.Detached;
            await context.SaveChangesAsync();
            var result = await controller.GetTodoItem(trackedTodoItem.Id);
            //assert
            Assert.IsTrue(result is NotFoundObjectResult);
        }

        private DbContextOptions<TodoContext> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<TodoContext>();
            builder.UseInMemoryDatabase(databaseName: "TestDb").UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }
    }
}
