using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.Api
{
    public class TodoItem
    {
        [Required]
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedTime { get; set; }
    }
}
