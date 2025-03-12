using DataDictionary.Models;
using DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataDictionary.Pages
{
    public class TablesModel : PageModel
    {
        private readonly IDataDictionaryService _dataDictionaryService;
        private readonly ILogger<TablesModel> _logger;

        public TablesModel(IDataDictionaryService dataDictionaryService, ILogger<TablesModel> logger)
        {
            _dataDictionaryService = dataDictionaryService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int DatabaseId { get; set; }

        public Models.DatabaseModel CurrentDatabase { get; set; }
        public List<Models.DatabaseModel> Databases { get; set; } = new List<Models.DatabaseModel>();
        public List<Models.TableDefinitionModel> Tables { get; set; } = new List<Models.TableDefinitionModel>();
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get all databases for the dropdown
                var databases = await _dataDictionaryService.GetDatabasesAsync();
                if (databases != null)
                {
                    Databases = databases.ToList();
                }

                // If a database is selected, get its details and tables
                if (DatabaseId > 0)
                {
                    // Get the current database
                    CurrentDatabase = await _dataDictionaryService.GetDatabaseAsync(DatabaseId);

                    // Get tables for the selected database
                    var tables = await _dataDictionaryService.GetTablesAsync(DatabaseId);
                    if (tables != null)
                    {
                        Tables = tables.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tables for database {DatabaseId}", DatabaseId);
                ErrorMessage = $"Error retrieving tables: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostGetExternalDatabaseAsync(string serverName, string databaseName, 
            bool useIntegratedSecurity, string username, string password, string description)
        {
            try
            {
                // Build connection string
                string connectionString;
                if (useIntegratedSecurity)
                {
                    connectionString = $"Server={serverName};Database={databaseName};Integrated Security=True;";
                }
                else
                {
                    connectionString = $"Server={serverName};Database={databaseName};User Id={username};Password={password};";
                }

                // Test the connection
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // 1. Add or get server record
                    var serverId = await _dataDictionaryService.AddOrGetServerAsync(serverName, description);
                    
                    // 2. Add or get database record
                    var dbId = await _dataDictionaryService.AddOrGetDatabaseAsync(serverId, databaseName, description);
                    
                    // 3. Get database schema information
                    await ImportDatabaseSchemaAsync(connection, dbId);
                    
                    // Redirect to the tables page for the newly imported database
                    SuccessMessage = $"Successfully imported schema from {databaseName} on {serverName}";
                    return RedirectToPage("/Tables", new { databaseId = dbId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to external database {Server}.{Database}", serverName, databaseName);
                ErrorMessage = $"Error connecting to database: {ex.Message}";
                return Page();
            }
        }

        private async Task ImportDatabaseSchemaAsync(SqlConnection connection, int databaseId)
        {
            // Get all tables and views
            var tables = await GetTablesAndViewsAsync(connection);
            
            foreach (var table in tables)
            {
                // Add database object record
                var objectTypeId = table.IsView ? 2 : 1; // 1 for tables, 2 for views
                var objectId = await _dataDictionaryService.AddOrGetDatabaseObjectAsync(
                    databaseId, objectTypeId, table.Schema, table.Name, table.Description);
                
                // Add table definition record
                var tableId = await _dataDictionaryService.AddOrGetTableDefinitionAsync(
                    objectId, table.IsView, table.RowCount);
                
                // Get columns for this table
                var columns = await GetColumnsAsync(connection, table.Schema, table.Name);
                
                foreach (var column in columns)
                {
                    // Add column definition record
                    await _dataDictionaryService.AddOrGetColumnDefinitionAsync(
                        tableId, column.Name, column.OrdinalPosition, column.DataTypeId,
                        column.MaxLength, column.Precision, column.Scale, 
                        column.IsNullable, column.HasDefault, column.DefaultValue, column.Description);
                }
                
                // Get relationships for this table (foreign keys)
                if (!table.IsView)
                {
                    var relationships = await GetRelationshipsAsync(connection, table.Schema, table.Name);
                    
                    foreach (var rel in relationships)
                    {
                        // Get parent and child table IDs
                        var parentTableId = await _dataDictionaryService.GetTableIdBySchemaAndNameAsync(
                            databaseId, rel.ParentSchema, rel.ParentTable);
                        
                        var childTableId = await _dataDictionaryService.GetTableIdBySchemaAndNameAsync(
                            databaseId, rel.ChildSchema, rel.ChildTable);
                        
                        if (parentTableId > 0 && childTableId > 0)
                        {
                            // Get parent and child column IDs
                            var parentColumnId = await _dataDictionaryService.GetColumnIdByTableAndNameAsync(
                                parentTableId, rel.ParentColumn);
                            
                            var childColumnId = await _dataDictionaryService.GetColumnIdByTableAndNameAsync(
                                childTableId, rel.ChildColumn);
                            
                            if (parentColumnId > 0 && childColumnId > 0)
                            {
                                // Add relationship record
                                await _dataDictionaryService.AddOrGetTableRelationshipAsync(
                                    parentTableId, childTableId, parentColumnId, childColumnId,
                                    1, // 1 for foreign key relationship
                                    rel.ConstraintName, rel.Description);
                            }
                        }
                    }
                }
            }
        }

        private async Task<List<(string Schema, string Name, string Description, bool IsView, long RowCount)>> GetTablesAndViewsAsync(SqlConnection connection)
        {
            var result = new List<(string Schema, string Name, string Description, bool IsView, long RowCount)>();
            
            // Query to get tables and views
            var query = @"
                SELECT 
                    s.name AS SchemaName,
                    t.name AS TableName,
                    ISNULL(ep.value, '') AS Description,
                    CASE WHEN t.type = 'V' THEN 1 ELSE 0 END AS IsView,
                    CASE WHEN t.type = 'V' THEN 0 ELSE p.rows END AS [RowCount]
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                LEFT JOIN sys.extended_properties ep ON ep.major_id = t.object_id AND ep.minor_id = 0 AND ep.name = 'MS_Description'
                LEFT JOIN sys.indexes i ON t.object_id = i.object_id AND i.index_id < 2
                LEFT JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
                
                UNION ALL
                
                SELECT 
                    s.name AS SchemaName,
                    v.name AS TableName,
                    ISNULL(ep.value, '') AS Description,
                    1 AS IsView,
                    0 AS [RowCount]
                FROM sys.views v
                INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
                LEFT JOIN sys.extended_properties ep ON ep.major_id = v.object_id AND ep.minor_id = 0 AND ep.name = 'MS_Description'
                
                ORDER BY SchemaName, TableName";
            
            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add((
                            Schema: reader.GetString(0),
                            Name: reader.GetString(1),
                            Description: reader.GetString(2),
                            IsView: reader.GetInt32(3) == 1,
                            RowCount: reader.GetInt64(4)
                        ));
                    }
                }
            }
            
            return result;
        }

        private async Task<List<(string Name, int OrdinalPosition, int DataTypeId, int? MaxLength, int? Precision, int? Scale, bool IsNullable, bool HasDefault, string DefaultValue, string Description)>> GetColumnsAsync(SqlConnection connection, string schema, string tableName)
        {
            var result = new List<(string Name, int OrdinalPosition, int DataTypeId, int? MaxLength, int? Precision, int? Scale, bool IsNullable, bool HasDefault, string DefaultValue, string Description)>();
            
            // Query to get columns
            var query = @"
                SELECT 
                    c.name AS ColumnName,
                    c.column_id AS OrdinalPosition,
                    t.name AS DataTypeName,
                    c.max_length AS MaxLength,
                    c.precision AS Precision,
                    c.scale AS Scale,
                    c.is_nullable AS IsNullable,
                    CASE WHEN c.default_object_id = 0 THEN 0 ELSE 1 END AS HasDefault,
                    ISNULL(d.definition, '') AS DefaultValue,
                    ISNULL(ep.value, '') AS Description
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                INNER JOIN sys.objects o ON c.object_id = o.object_id
                INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
                LEFT JOIN sys.default_constraints d ON c.default_object_id = d.object_id
                LEFT JOIN sys.extended_properties ep ON ep.major_id = c.object_id AND ep.minor_id = c.column_id AND ep.name = 'MS_Description'
                WHERE s.name = @Schema AND o.name = @TableName
                ORDER BY c.column_id";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Schema", schema);
                command.Parameters.AddWithValue("@TableName", tableName);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Map SQL Server data type to our data type ID
                        var dataTypeName = reader.GetString(2);
                        var dataTypeId = await _dataDictionaryService.GetOrAddDataTypeIdAsync(dataTypeName);
                        
                        result.Add((
                            Name: reader.GetString(0),
                            OrdinalPosition: reader.GetInt32(1),
                            DataTypeId: dataTypeId,
                            MaxLength: reader.IsDBNull(3) ? null : (int?)reader.GetInt16(3),
                            Precision: reader.IsDBNull(4) ? null : (int?)reader.GetByte(4),
                            Scale: reader.IsDBNull(5) ? null : (int?)reader.GetByte(5),
                            IsNullable: reader.GetBoolean(6),
                            HasDefault: reader.GetInt32(7) == 1,
                            DefaultValue: reader.GetString(8),
                            Description: reader.GetString(9)
                        ));
                    }
                }
            }
            
            return result;
        }

        private async Task<List<(string ConstraintName, string ParentSchema, string ParentTable, string ParentColumn, string ChildSchema, string ChildTable, string ChildColumn, string Description)>> GetRelationshipsAsync(SqlConnection connection, string schema, string tableName)
        {
            var result = new List<(string ConstraintName, string ParentSchema, string ParentTable, string ParentColumn, string ChildSchema, string ChildTable, string ChildColumn, string Description)>();
            
            // Query to get foreign key relationships
            var query = @"
                -- Get relationships where this table is the parent
                SELECT 
                    fk.name AS ConstraintName,
                    ps.name AS ParentSchema,
                    pt.name AS ParentTable,
                    pc.name AS ParentColumn,
                    cs.name AS ChildSchema,
                    ct.name AS ChildTable,
                    cc.name AS ChildColumn,
                    ISNULL(ep.value, '') AS Description
                FROM sys.foreign_keys fk
                INNER JOIN sys.tables pt ON fk.parent_object_id = pt.object_id
                INNER JOIN sys.schemas ps ON pt.schema_id = ps.schema_id
                INNER JOIN sys.tables ct ON fk.referenced_object_id = ct.object_id
                INNER JOIN sys.schemas cs ON ct.schema_id = cs.schema_id
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns pc ON fkc.parent_column_id = pc.column_id AND fkc.parent_object_id = pc.object_id
                INNER JOIN sys.columns cc ON fkc.referenced_column_id = cc.column_id AND fkc.referenced_object_id = cc.object_id
                LEFT JOIN sys.extended_properties ep ON ep.major_id = fk.object_id AND ep.minor_id = 0 AND ep.name = 'MS_Description'
                WHERE ps.name = @Schema AND pt.name = @TableName
                
                UNION ALL
                
                -- Get relationships where this table is the child
                SELECT 
                    fk.name AS ConstraintName,
                    ps.name AS ParentSchema,
                    pt.name AS ParentTable,
                    pc.name AS ParentColumn,
                    cs.name AS ChildSchema,
                    ct.name AS ChildTable,
                    cc.name AS ChildColumn,
                    ISNULL(ep.value, '') AS Description
                FROM sys.foreign_keys fk
                INNER JOIN sys.tables pt ON fk.referenced_object_id = pt.object_id
                INNER JOIN sys.schemas ps ON pt.schema_id = ps.schema_id
                INNER JOIN sys.tables ct ON fk.parent_object_id = ct.object_id
                INNER JOIN sys.schemas cs ON ct.schema_id = cs.schema_id
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns pc ON fkc.referenced_column_id = pc.column_id AND fkc.referenced_object_id = pc.object_id
                INNER JOIN sys.columns cc ON fkc.parent_column_id = cc.column_id AND fkc.parent_object_id = cc.object_id
                LEFT JOIN sys.extended_properties ep ON ep.major_id = fk.object_id AND ep.minor_id = 0 AND ep.name = 'MS_Description'
                WHERE cs.name = @Schema AND ct.name = @TableName";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Schema", schema);
                command.Parameters.AddWithValue("@TableName", tableName);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add((
                            ConstraintName: reader.GetString(0),
                            ParentSchema: reader.GetString(1),
                            ParentTable: reader.GetString(2),
                            ParentColumn: reader.GetString(3),
                            ChildSchema: reader.GetString(4),
                            ChildTable: reader.GetString(5),
                            ChildColumn: reader.GetString(6),
                            Description: reader.GetString(7)
                        ));
                    }
                }
            }
            
            return result;
        }
    }
} 