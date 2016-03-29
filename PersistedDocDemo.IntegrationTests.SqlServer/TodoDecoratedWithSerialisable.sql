CREATE TABLE [dbo].[TodoDecoratedWithSerialisable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[Colour] VARCHAR(25),
	[Data] NVARCHAR(MAX) NOT NULL DEFAULT ''
)
