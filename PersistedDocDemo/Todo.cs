using System;

namespace PersistedDocDemo
{
    [Serializable]
    public class Todo
    {
        private DateTime created = DateTime.UtcNow;
        public int Id { get; set; }
        public string Name { get; set; }
    }
}