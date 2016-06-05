using System;
using System.Collections.Generic;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    public class Todo
    {
        private DateTime created = DateTime.UtcNow;

        public Todo()
        {
            ChildTasks = new List<Todo>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        [SqlColumn]
        public string Colour { get; set; }

        public ICollection<Todo> ChildTasks { get; private set; }
    }
}