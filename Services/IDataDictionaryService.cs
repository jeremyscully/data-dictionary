using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataDictionary.Models;

namespace DataDictionary.Services
{
    /// <summary>
    /// Interface for the Data Dictionary Service
    /// Defines all operations for managing data dictionary information
    /// </summary>
    public interface IDataDictionaryService
    {
        // Server operations
        Task<IEnumerable<ServerModel>> GetServersAsync();
        Task<ServerModel> GetServerAsync(int serverId);
        Task<int> AddOrGetServerAsync(string serverName, string description = "");
        
        // Database operations
        Task<IEnumerable<DatabaseModel>> GetDatabasesAsync();
        Task<DatabaseModel> GetDatabaseAsync(int databaseId);
        Task<int> AddOrGetDatabaseAsync(int serverId, string databaseName, string description = "");
        
        // Table operations
        Task<IEnumerable<TableDefinitionModel>> GetTablesAsync(int databaseId);
        Task<(IEnumerable<TableDefinitionModel> Tables, int TotalCount)> GetTablesPagedAsync(int databaseId, int pageNumber, int pageSize, bool excludeViews);
        Task<TableDefinitionModel> GetTableAsync(int tableId);
        Task<int> AddOrGetTableDefinitionAsync(int objectId, bool isView, long? rowCount = null);
        Task<int> GetTableIdBySchemaAndNameAsync(int databaseId, string schema, string tableName);
        
        // Column operations
        Task<IEnumerable<ColumnDefinitionModel>> GetColumnsAsync(int tableId);
        Task<ColumnDefinitionModel> GetColumnAsync(int columnId);
        Task<int> GetColumnIdByTableAndNameAsync(int tableId, string columnName);
        Task<int> AddOrGetColumnDefinitionAsync(int tableId, string columnName, int ordinalPosition, 
            int dataTypeId, int? maxLength, int? precision, int? scale, bool isNullable, bool hasDefault, 
            string defaultValue, string description = "");
        Task<IEnumerable<ColumnConstraintModel>> GetColumnConstraintsAsync(int columnId);
        
        // Object operations
        Task<int> AddOrGetDatabaseObjectAsync(int databaseId, int objectTypeId, string schemaName, string objectName, string description = "");
        
        // Data type operations
        Task<IEnumerable<DataTypeModel>> GetDataTypesAsync();
        Task<int> GetOrAddDataTypeIdAsync(string typeName, string description = "");
        Task<IEnumerable<ConstraintTypeModel>> GetConstraintTypesAsync();
        
        // Relationship operations
        Task<IEnumerable<TableRelationshipModel>> GetTableRelationshipsAsync(int tableId, bool isParent);
        Task<IEnumerable<RelationshipTypeModel>> GetRelationshipTypesAsync();
        Task<int> AddOrGetTableRelationshipAsync(int parentTableId, int childTableId, 
            int parentColumnId, int childColumnId, int relationshipTypeId, string constraintName, string description = "");
        
        // Lineage operations
        Task<IEnumerable<LineageTypeModel>> GetLineageTypesAsync();
        Task<LineageTypeModel> GetLineageTypeAsync(int lineageTypeId);
        Task<IEnumerable<DataLineageModel>> GetDataLineageRelationshipsAsync();
        Task<DataLineageModel> GetDataLineageRelationshipAsync(int lineageId);
        Task<IEnumerable<DataLineageModel>> GetDatabaseLineageAsync(int databaseId, bool isSource);
        Task<IEnumerable<DataLineageModel>> GetTableLineageAsync(int tableId, bool isSource);
        Task<IEnumerable<DataLineageModel>> GetColumnLineageAsync(int columnId, bool isSource);
        Task<int> AddDataLineageAsync(int lineageTypeId, int? sourceDatabaseId, int? sourceTableId, int? sourceColumnId,
            int? targetDatabaseId, int? targetTableId, int? targetColumnId, string transformationDescription);
        Task UpdateDataLineageAsync(int lineageId, int lineageTypeId, int? sourceDatabaseId, int? sourceTableId, int? sourceColumnId,
            int? targetDatabaseId, int? targetTableId, int? targetColumnId, string transformationDescription);
        Task DeleteDataLineageAsync(int lineageId);
    }
    
    // Extension methods for IDataDictionaryService
    public static class DataDictionaryServiceExtensions
    {
        /// <summary>
        /// Gets parent relationships for a table (where the table is a child)
        /// </summary>
        public static Task<IEnumerable<TableRelationshipModel>> GetParentRelationshipsAsync(
            this IDataDictionaryService service, int tableId)
        {
            return service.GetTableRelationshipsAsync(tableId, false);
        }
        
        /// <summary>
        /// Gets child relationships for a table (where the table is a parent)
        /// </summary>
        public static Task<IEnumerable<TableRelationshipModel>> GetChildRelationshipsAsync(
            this IDataDictionaryService service, int tableId)
        {
            return service.GetTableRelationshipsAsync(tableId, true);
        }
    }
} 