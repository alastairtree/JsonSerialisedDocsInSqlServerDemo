#New features to add (in no particular order)

<del>Handle [SqlColumn] on enumerable types as pipe seperated values</del> Done 


**ID generation**

* allow clients to generate IDs
* Faster/moe flexible that than tsql IDENTITY()
* could using something like the hi-lo algorithm or bulk key allocation
* get a range of keys in batches. kep allocations in another table? 
* ideally keys would be sequntial ints for performance
* could use a string and put typename in the key with an int value
* https://octopus.com/blog/sql-as-document-storehttps://octopus.com/blog/sql-as-document-store
* need way that works for sql and files etc


**[SqlColumn] on enumerable supports database table**
* as an option. enable consumers to swap between behaviours.
* must be better than pipe delimited way as of now?
* triggers? or generate extra  SQL in a transactional manner to create join/index tables


Simple querying API
* by ID
* by some column name
* Like dapper? actually use dapper? 
* octopus deploy did it like :

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
    .ToList()
```

Transactions/unit of work to allow users to save several classes at once
- transaction scope or something custom
* could reuse transactions from the framework


Generate create table statements using SqlBuilder based on the metatdata used to query the database.


Guidelines for migrations/tooling to handle deserialisations errors?


Sample app
* maybe use the MS music store sample


Demo using native SQL 2016 JSON support


Searching over the json?


.net caspnet core support