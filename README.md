# Sample .net code to demonstrate serialising any complex c# object to test
and using Json.to.serialseserialised In Sql Server/the file system/in memory using a consistent interface

Domonstration of using SQL server as a JSON document store for .net domain objects that are aggregate roots.

Sample app demonstrating storing serialised domain objects in sql server (and files, memory) as json, using the repository pattern. Capable of storing large complex object graphs in one table.

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


### Properties decorated with an attribute [SqlColumn] will get saved as columns

To allow you to query on some fields you can annotate your model and the repository will store those fields as proper sql table columns.

So create your domain model

```
public class Todo{
	public int Id { get; set;}
	[SqlColumn]
	public DateTime DueDate { get; set; }

	/// any other public properties and complex objects get saved as JSON
}
```

And setup a table in SQL like so

	CREATE TABLE [dbo].[Todo]
	(
		[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
		[DueDate] DATETIME NOT NULL,
		[Data] NVARCHAR(MAX) NOT NULL DEFAULT ''
	)


And then save it as usual. The structure of the under lying table is inferred by naming convention and metatdata alone - zero config.

```
IRepository<Todo> todoRepository = new SqlServerRepository<Todo>(conxString);
var item = new Todo();
todoRepository.Save(item);

```


### List and collection properties decorated with an attribute [SqlColumn] will get saved as a pipe delimited text column

To allow you to query and index lists we pipe delimit the contents into a nvarchar(max) column for you. This allows you to serach with the SQL LIKE operator without having to parse all the JSON or load all records into memory.

```
public class Todo{ //my aggregate root

	// other stufff

	[SqlColumn]
	public List<string> Tags { get; set; }

	/// any other public properties and complex objects get saved as JSON
}
```

This class would be persisted and queryable like below. Yes it it denormalised and not optimal but consider it fast enough in most situations

```

-- tags column looks like "|Red|Blue|Green|" allowing us to query by tag. 


SELECT *
FROM [Todo]
WHERE [Tags] LIKE '%|' + @value + '|%'
```


### Or save to the file system

Allows you to swap out the underlying data store and persist your domain model on disk as json. 
Well suited to dependency inject.

So to save to a file. Defaults to the temp folder but configurable.

	IRepository<Todo> todoRepository = new FileRepository<Todo>();
	var item = new Todo();
	todoRepository.Save(item);

### Also supports saving in memory (use in testing/cache etc)

	IRepository<Todo> todoRepository = new InMemoryRepository<Todo>();
	var item = new Todo();
	todoRepository.Save(item);


## Even More examples
 See the Integration Tests.
