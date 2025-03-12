-- DataDictionary_SampleData.sql
-- Sample data load script for Data Dictionary database
-- For use with (localdb)\MSSQLLocalDB

USE [DataDict]
GO

-- Clear existing data (if any)
PRINT 'Clearing existing data...'
GO

-- Disable foreign key constraints temporarily
PRINT 'Disabling constraints...'
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

-- Delete data from tables in reverse dependency order
DELETE FROM [dbo].[DataLineageModel];
DELETE FROM [dbo].[TableRelationshipModel];
DELETE FROM [dbo].[ColumnConstraintModel];
DELETE FROM [dbo].[ColumnDefinitionModel];
DELETE FROM [dbo].[TableDefinitionModel];
DELETE FROM [dbo].[DatabaseObjectModel];
DELETE FROM [dbo].[DatabaseModel];
DELETE FROM [dbo].[ServerModel];
DELETE FROM [dbo].[LineageTypeModel];
DELETE FROM [dbo].[RelationshipTypeModel];
DELETE FROM [dbo].[ConstraintTypeModel];
DELETE FROM [dbo].[DataTypeModel];
DELETE FROM [dbo].[ObjectTypeModel];
GO

-- Re-enable constraints
PRINT 'Re-enabling constraints...'
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
GO

-- Insert reference data
PRINT 'Loading reference data...'

-- Object Types
INSERT INTO [dbo].[ObjectTypeModel] ([TypeName], [Description])
VALUES 
('Table', 'Standard database table for storing data'),
('View', 'Virtual table based on the result of a SQL statement'),
('Stored Procedure', 'Prepared SQL code that can be saved and reused'),
('Function', 'Routine that returns a value'),
('Trigger', 'SQL code that executes automatically in response to certain events');
GO
