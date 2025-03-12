-- Create LineageTypes table
CREATE TABLE dbo.LineageTypes (
    LineageTypeId INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- Insert default lineage types
INSERT INTO dbo.LineageTypes (TypeName, Description)
VALUES 
    ('Raw', 'Direct copy of source data with no transformations'),
    ('Aggregated', 'Data that has been aggregated from the source'),
    ('Computation', 'Data that is computed or derived from the source');

-- Create DataLineage table
CREATE TABLE dbo.DataLineage (
    LineageId INT IDENTITY(1,1) PRIMARY KEY,
    LineageTypeId INT NOT NULL,
    
    -- Source can be at database, table, or column level
    SourceDatabaseId INT NULL,
    SourceTableId INT NULL,
    SourceColumnId INT NULL,
    
    -- Target can be at database, table, or column level
    TargetDatabaseId INT NULL,
    TargetTableId INT NULL,
    TargetColumnId INT NULL,
    
    -- Description of the transformation or relationship
    TransformationDescription NVARCHAR(MAX) NULL,
    
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Foreign key constraints
    CONSTRAINT FK_DataLineage_LineageTypes FOREIGN KEY (LineageTypeId) 
        REFERENCES dbo.LineageTypes (LineageTypeId),
    
    -- Source foreign keys
    CONSTRAINT FK_DataLineage_SourceDatabase FOREIGN KEY (SourceDatabaseId) 
        REFERENCES dbo.Databases (DatabaseId),
    CONSTRAINT FK_DataLineage_SourceTable FOREIGN KEY (SourceTableId) 
        REFERENCES dbo.TableDefinitions (TableId),
    CONSTRAINT FK_DataLineage_SourceColumn FOREIGN KEY (SourceColumnId) 
        REFERENCES dbo.ColumnDefinitions (ColumnId),
    
    -- Target foreign keys
    CONSTRAINT FK_DataLineage_TargetDatabase FOREIGN KEY (TargetDatabaseId) 
        REFERENCES dbo.Databases (DatabaseId),
    CONSTRAINT FK_DataLineage_TargetTable FOREIGN KEY (TargetTableId) 
        REFERENCES dbo.TableDefinitions (TableId),
    CONSTRAINT FK_DataLineage_TargetColumn FOREIGN KEY (TargetColumnId) 
        REFERENCES dbo.ColumnDefinitions (ColumnId),
    
    -- Check constraints to ensure at least one source and one target is specified
    CONSTRAINT CK_DataLineage_Source CHECK (
        SourceDatabaseId IS NOT NULL OR 
        SourceTableId IS NOT NULL OR 
        SourceColumnId IS NOT NULL
    ),
    CONSTRAINT CK_DataLineage_Target CHECK (
        TargetDatabaseId IS NOT NULL OR 
        TargetTableId IS NOT NULL OR 
        TargetColumnId IS NOT NULL
    ),
    
    -- Check constraint to ensure transformation description is provided for certain types
    CONSTRAINT CK_DataLineage_TransformationDescription CHECK (
        LineageTypeId = 1 OR -- Raw type doesn't require description
        (LineageTypeId IN (2, 3) AND TransformationDescription IS NOT NULL) -- Aggregated and Computation require description
    )
);

-- Create indexes for better query performance
CREATE INDEX IX_DataLineage_LineageTypeId ON dbo.DataLineage (LineageTypeId);
CREATE INDEX IX_DataLineage_SourceDatabaseId ON dbo.DataLineage (SourceDatabaseId) WHERE SourceDatabaseId IS NOT NULL;
CREATE INDEX IX_DataLineage_SourceTableId ON dbo.DataLineage (SourceTableId) WHERE SourceTableId IS NOT NULL;
CREATE INDEX IX_DataLineage_SourceColumnId ON dbo.DataLineage (SourceColumnId) WHERE SourceColumnId IS NOT NULL;
CREATE INDEX IX_DataLineage_TargetDatabaseId ON dbo.DataLineage (TargetDatabaseId) WHERE TargetDatabaseId IS NOT NULL;
CREATE INDEX IX_DataLineage_TargetTableId ON dbo.DataLineage (TargetTableId) WHERE TargetTableId IS NOT NULL;
CREATE INDEX IX_DataLineage_TargetColumnId ON dbo.DataLineage (TargetColumnId) WHERE TargetColumnId IS NOT NULL; 