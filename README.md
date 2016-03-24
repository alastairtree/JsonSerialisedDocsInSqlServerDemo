# Sample APP to demonstrate complex c# objects Json serialised In Sql Server/the file system/in memory using a consistent interface
Sample app demonstrating storing serialised domain objects in sql server, files, memory as json, using the repository pattern. Capable of storing large complex object graphs in one table.

## Example

Given you want to persist some todo objects

    // Only need [Serializable] if private fields need to be persisted.
    [Serializable] 
    public class Todo //Aggregate root
    {
        public Todo()
        {
            ChildTasks = new List<Todo>();
        }
        
        // Will be saved because we used [Serializable]
        private DateTime created = DateTime.UtcNow;
        
        // Public properties are always serialised and persisted
        public int Id { get; set; } //naming conventions used to guess the key field
        public string Name { get; set; }
        
        // Complex graph of child objects get persisted fine
        public ICollection<Todo> ChildTasks { get; private set; }
    }


You can use then use any one of the implmentations provided to the IRepository interface
 
    public interface IRepository<T>
    {
        T Get(object id);
        ICollection<T> GetAll();
        void Save(T item);
        bool Delete(object id);
        bool Delete(T item);
        bool DeleteAll();
    }

### Saving in SQL Server

Given I have this simple table

    CREATE TABLE [dbo].[Todo]
    (
	    [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	    [Data] NVARCHAR(MAX) NOT NULL DEFAULT ''
    )

Then I can save my complex entity as json inside the [Data] column

    var conxString = ConfigurationManager.ConnectionStrings["PersistedDb"].ConnectionString
    IRepository<Todo> todoRepository = new SqlServerRepository<Todo>(conxString);
    var item = new Todo();
    todoRepository.Save(item);

### Saving to the file system

    IRepository<Todo> todoRepository = new FileRepository<Todo>();
    var item = new Todo();
    todoRepository.Save(item);

### Saving in memory (use in testing/cache etc)

    IRepository<Todo> todoRepository = new InMemoryRepository<Todo>();
    var item = new Todo();
    todoRepository.Save(item);

## More examples
 See the Integration Tests.
