using TodoList.Api.Controllers;
using Xunit;

namespace TodoList.Api.UnitTests
{
    public class TodoItemControllerTests
    {
        [Fact]
        public void Get_TodoItems_Returns_Incompleted_Ones()
        {
            //arrange
            var controller = new TodoItemsController();
            //act
            //assert
        }
    }
}
