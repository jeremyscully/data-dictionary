using System;
using System.Collections.Generic;

namespace DataDictionary.Models
{
    // Server model
    public class ServerModel
    {
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public string ServerType { get; set; }
        public string Description { get; set; }
        public string ConnectionString { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public List<DatabaseModel> Databases { get; set; } = new List<DatabaseModel>();
    }
    
    // Database model
    public class DatabaseModel
    {
        public int DatabaseId { get; set; }
        public int ServerId { get; set; }
        public string DatabaseName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public ServerModel Server { get; set; }
        public List<DatabaseObjectModel> Objects { get; set; } = new List<DatabaseObjectModel>();
        
        // Lineage navigation properties
        public List<DataLineageModel> SourceLineages { get; set; } = new List<DataLineageModel>();
        public List<DataLineageModel> TargetLineages { get; set; } = new List<DataLineageModel>();
    }
    
    // Object type model
    public class ObjectTypeModel
    {
        public int ObjectTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        
        // Navigation properties
        public List<DatabaseObjectModel> Objects { get; set; } = new List<DatabaseObjectModel>();
    }
    
    // Data type model
    public class DataTypeModel
    {
        public int DataTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public bool IsNumeric { get; set; }
        public bool IsTextual { get; set; }
        public bool IsDateTime { get; set; }
        public bool IsBinary { get; set; }
        
        // Navigation properties
        public List<ColumnDefinitionModel> Columns { get; set; } = new List<ColumnDefinitionModel>();
    }
    
    // Constraint type model
    public class ConstraintTypeModel
    {
        public int ConstraintTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        
        // Navigation properties
        public List<ColumnConstraintModel> Constraints { get; set; } = new List<ColumnConstraintModel>();
    }
    
    // Relationship type model
    public class RelationshipTypeModel
    {
        public int RelationshipTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        
        // Navigation properties
        public List<TableRelationshipModel> Relationships { get; set; } = new List<TableRelationshipModel>();
    }
    
    // Database object model
    public class DatabaseObjectModel
    {
        public int ObjectId { get; set; }
        public int DatabaseId { get; set; }
        public int ObjectTypeId { get; set; }
        public string SchemaName { get; set; }
        public string ObjectName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public DatabaseModel Database { get; set; }
        public ObjectTypeModel ObjectType { get; set; }
        public TableDefinitionModel Table { get; set; }
    }
    
    // Table definition model
    public class TableDefinitionModel
    {
        public int TableId { get; set; }
        public int ObjectId { get; set; }
        public bool IsView { get; set; }
        public long? RowCount { get; set; }
        public bool HasPrimaryKey { get; set; }
        public bool HasClusteredIndex { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public DatabaseObjectModel DatabaseObject { get; set; }
        
        // Computed property to get the table name from the DatabaseObject
        public string TableName => DatabaseObject?.ObjectName ?? $"Table_{TableId}";
        
        // Navigation properties
        public List<ColumnDefinitionModel> Columns { get; set; } = new List<ColumnDefinitionModel>();
        public List<TableRelationshipModel> ParentRelationships { get; set; } = new List<TableRelationshipModel>();
        public List<TableRelationshipModel> ChildRelationships { get; set; } = new List<TableRelationshipModel>();
        
        // Lineage navigation properties
        public List<DataLineageModel> SourceLineages { get; set; } = new List<DataLineageModel>();
        public List<DataLineageModel> TargetLineages { get; set; } = new List<DataLineageModel>();
    }
    
    // Column definition model
    public class ColumnDefinitionModel
    {
        public int ColumnId { get; set; }
        public int TableId { get; set; }
        public string ColumnName { get; set; }
        public int OrdinalPosition { get; set; }
        public int DataTypeId { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool HasDefault { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public TableDefinitionModel Table { get; set; }
        public DataTypeModel DataType { get; set; }
        public List<ColumnConstraintModel> Constraints { get; set; } = new List<ColumnConstraintModel>();
        public List<TableRelationshipModel> ParentRelationships { get; set; } = new List<TableRelationshipModel>();
        public List<TableRelationshipModel> ChildRelationships { get; set; } = new List<TableRelationshipModel>();
        
        // Lineage navigation properties
        public List<DataLineageModel> SourceLineages { get; set; } = new List<DataLineageModel>();
        public List<DataLineageModel> TargetLineages { get; set; } = new List<DataLineageModel>();
    }
    
    // Column constraint model
    public class ColumnConstraintModel
    {
        public int ConstraintId { get; set; }
        public int ColumnId { get; set; }
        public int ConstraintTypeId { get; set; }
        public string ConstraintName { get; set; }
        public string Definition { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public ColumnDefinitionModel Column { get; set; }
        public ConstraintTypeModel ConstraintType { get; set; }
    }
    
    // Table relationship model
    public class TableRelationshipModel
    {
        public int RelationshipId { get; set; }
        public int RelationshipTypeId { get; set; }
        public int ParentTableId { get; set; }
        public int ChildTableId { get; set; }
        public int ParentColumnId { get; set; }
        public int ChildColumnId { get; set; }
        public string ConstraintName { get; set; }
        public bool IsEnforced { get; set; }
        public string DeleteAction { get; set; }
        public string UpdateAction { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public RelationshipTypeModel RelationshipType { get; set; }
        public TableDefinitionModel ParentTable { get; set; }
        public TableDefinitionModel ChildTable { get; set; }
        public ColumnDefinitionModel ParentColumn { get; set; }
        public ColumnDefinitionModel ChildColumn { get; set; }
    }
    
    // Lineage relationship type model
    public class LineageTypeModel
    {
        public int LineageTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        
        // Navigation properties
        public List<DataLineageModel> LineageRelationships { get; set; } = new List<DataLineageModel>();
    }
    
    // Data lineage model - tracks source-target relationships between database objects
    public class DataLineageModel
    {
        public int LineageId { get; set; }
        public int LineageTypeId { get; set; }
        
        // Source can be at database, table, or column level
        public int? SourceDatabaseId { get; set; }
        public int? SourceTableId { get; set; }
        public int? SourceColumnId { get; set; }
        
        // Target can be at database, table, or column level
        public int? TargetDatabaseId { get; set; }
        public int? TargetTableId { get; set; }
        public int? TargetColumnId { get; set; }
        
        // Description of the transformation or relationship
        public string TransformationDescription { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // Navigation properties
        public LineageTypeModel LineageType { get; set; }
        
        // Source navigation properties
        public DatabaseModel SourceDatabase { get; set; }
        public TableDefinitionModel SourceTable { get; set; }
        public ColumnDefinitionModel SourceColumn { get; set; }
        
        // Target navigation properties
        public DatabaseModel TargetDatabase { get; set; }
        public TableDefinitionModel TargetTable { get; set; }
        public ColumnDefinitionModel TargetColumn { get; set; }
    }
} 