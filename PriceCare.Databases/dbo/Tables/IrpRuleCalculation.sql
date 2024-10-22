CREATE TABLE [dbo].[IrpRuleCalculation]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [IrpRuleListId] INT NOT NULL, 
    [UpId] INT NULL, 
    [IrpMathId] INT NOT NULL, 
    [Argument] INT NOT NULL
)
