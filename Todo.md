# Potential features to add

Handle [SqlColumn] on enumerable types
* pipe seperated values
* or dynamic table?



ID generation

* using something like the hi-lo algorithm
* get a range of keys in batches. kep allocations in another table? 
* ideally keys would be sequntial ints for performance
* could use a string and put typename in the key with an int value
* https://octopus.com/blog/sql-as-document-storehttps://octopus.com/blog/sql-as-document-store
* need way that works for sql and files etc


Support simple where clause generator/query by column name
like this?:
```

var project = transaction.Query<Project>()
    .Where("Name = @name and IsDisabled = 0")
    .Parameter("name", "My Project");
    .First();
var releases = transaction.Query<Release>()
    .Where("ProjectId = @proj")
    .Parameter("proj", project.Id)
    .ToList();var project = transaction.Query<Project>()
    .Where("Name = @name and IsDisabled = 0")
    .Parameter("name", "My Project");
    .First();

var releases = transaction.Query<Release>()
    .Where("ProjectId = @proj")
    .Parameter("proj", project.Id)
    .ToList();

```

Transactions/unit of work
- transaction scope or something custom. reuse transactions from the framework.



Guidelines for migrations/tooling to handle deserialisations errors?



Sample app



Demo using native SQL 2016 JSON support




Searching over the json?
