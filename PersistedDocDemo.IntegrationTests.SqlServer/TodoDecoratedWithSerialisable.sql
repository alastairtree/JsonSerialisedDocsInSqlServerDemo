CREATE TABLE [dbo].[TodoDecoratedWithSerialisable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[Colour] VARCHAR(25),
	[Tags] NVARCHAR(MAX) NOT NULL DEFAULT '',
	[Data] NVARCHAR(MAX) NOT NULL DEFAULT ''
)
