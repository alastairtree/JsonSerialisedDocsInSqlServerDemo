using System;
using System.Collections.Generic;

namespace PersistedDocDemo.IntegrationTests
{
    public class Todo
    {
        public Todo()
        {
            ChildTasks = new List<Todo>();
        }
        private DateTime created = DateTime.UtcNow;
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Todo> ChildTasks { get; private set; }
    }
}
