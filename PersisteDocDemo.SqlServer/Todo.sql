﻿CREATE TABLE [dbo].[Todo]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Data] NVARCHAR(MAX) NOT NULL DEFAULT ''
)