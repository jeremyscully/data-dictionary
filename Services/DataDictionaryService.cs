using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataDictionary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataDictionary.Services
{
    public class DataDictionaryService : IDataDictionaryService
    {
        private readonly string _connectionString;
        private readonly ILogger<DataDictionaryService> _logger;

        public DataDictionaryService(IConfiguration configuration, ILogger<DataDictionaryService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        // Get all servers
        public async Task<IEnumerable<ServerModel>> GetServersAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryAsync<ServerModel>("SELECT * FROM dbo.Servers ORDER BY ServerName");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving servers");
                throw;
            }
        }

        // Get server by ID
        public async Task<ServerModel> GetServerAsync(int serverId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryFirstOrDefaultAsync<ServerModel>(
                        "SELECT * FROM dbo.Servers WHERE ServerId = @ServerId",
                        new { ServerId = serverId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving server with ID {ServerId}", serverId);
                throw;
            }
        }

        // Get all databases
        public async Task<IEnumerable<DatabaseModel>> GetDatabasesAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryAsync<DatabaseModel>(
                        "SELECT d.*, s.ServerName FROM dbo.Databases d " +
                        "JOIN dbo.Servers s ON d.ServerId = s.ServerId " +
                        "ORDER BY s.ServerName, d.DatabaseName");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving databases");
                throw;
            }
        }

        // Get database by ID
        public async Task<DatabaseModel> GetDatabaseAsync(int databaseId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var database = await connection.QueryFirstOrDefaultAsync<DatabaseModel>(
                        "SELECT * FROM dbo.Databases WHERE DatabaseId = @DatabaseId",
                        new { DatabaseId = databaseId });

                    if (database != null)
                    {
                        database.Server = await connection.QueryFirstOrDefaultAsync<ServerModel>(
                            "SELECT * FROM dbo.Servers WHERE ServerId = @ServerId",
                            new { ServerId = database.ServerId });
                    }

                    return database;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database with ID {DatabaseId}", databaseId);
                throw;
            }
        }

        // Get all tables for a database
        public async Task<IEnumerable<TableDefinitionModel>> GetTablesAsync(int databaseId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // First, get all database objects for this database
                    var dbObjectsQuery = @"
                        SELECT do.*, ot.TypeName as ObjectTypeName
                        FROM dbo.DatabaseObjects do
                        JOIN dbo.ObjectTypes ot ON do.ObjectTypeId = ot.ObjectTypeId
                        WHERE do.DatabaseId = @DatabaseId";
                        
                    var dbObjects = await connection.QueryAsync<dynamic>(dbObjectsQuery, new { DatabaseId = databaseId });
                    var dbObjectsDict = dbObjects.ToDictionary(o => (int)o.ObjectId, o => o);
                    
                    // Then get all tables
                    var tablesQuery = @"
                        SELECT td.*
                        FROM dbo.TableDefinitions td
                        JOIN dbo.DatabaseObjects do ON td.ObjectId = do.ObjectId
                        WHERE do.DatabaseId = @DatabaseId
                        ORDER BY do.SchemaName, do.ObjectName";

                    var tables = await connection.QueryAsync<TableDefinitionModel>(tablesQuery, new { DatabaseId = databaseId });
                    
                    foreach (var table in tables)
                    {
                        // Get columns for each table
                        table.Columns = (await GetColumnsAsync(table.TableId)).ToList();
                        
                        // Set the DatabaseObject property
                        if (dbObjectsDict.TryGetValue(table.ObjectId, out var dbObject))
                        {
                            table.DatabaseObject = new DatabaseObjectModel
                            {
                                ObjectId = dbObject.ObjectId,
                                DatabaseId = dbObject.DatabaseId,
                                ObjectTypeId = dbObject.ObjectTypeId,
                                SchemaName = dbObject.SchemaName,
                                ObjectName = dbObject.ObjectName,
                                Description = dbObject.Description,
                                CreatedDate = dbObject.CreatedDate,
                                ModifiedDate = dbObject.ModifiedDate,
                                ObjectType = new ObjectTypeModel
                                {
                                    ObjectTypeId = dbObject.ObjectTypeId,
                                    TypeName = dbObject.ObjectTypeName
                                }
                            };
                        }
                    }
                    
                    return tables;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tables for database ID {DatabaseId}", databaseId);
                throw;
            }
        }

        // Get tables for a database with pagination and view filtering
        public async Task<(IEnumerable<TableDefinitionModel> Tables, int TotalCount)> GetTablesPagedAsync(int databaseId, int pageNumber, int pageSize, bool excludeViews)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // First, get all database objects for this database
                    var dbObjectsQuery = @"
                        SELECT do.*, ot.TypeName as ObjectTypeName
                        FROM dbo.DatabaseObjects do
                        JOIN dbo.ObjectTypes ot ON do.ObjectTypeId = ot.ObjectTypeId
                        WHERE do.DatabaseId = @DatabaseId";
                        
                    var dbObjects = await connection.QueryAsync<dynamic>(dbObjectsQuery, new { DatabaseId = databaseId });
                    var dbObjectsDict = dbObjects.ToDictionary(o => (int)o.ObjectId, o => o);
                    
                    // Get total count for pagination
                    var countQuery = @"
                        SELECT COUNT(*)
                        FROM dbo.TableDefinitions td
                        JOIN dbo.DatabaseObjects do ON td.ObjectId = do.ObjectId
                        JOIN dbo.ObjectTypes ot ON do.ObjectTypeId = ot.ObjectTypeId
                        WHERE do.DatabaseId = @DatabaseId
                        " + (excludeViews ? "AND td.IsView = 0" : "");

                    var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, new { DatabaseId = databaseId });
                    
                    // Then get tables with pagination
                    var tablesQuery = @"
                        SELECT td.*
                        FROM dbo.TableDefinitions td
                        JOIN dbo.DatabaseObjects do ON td.ObjectId = do.ObjectId
                        JOIN dbo.ObjectTypes ot ON do.ObjectTypeId = ot.ObjectTypeId
                        WHERE do.DatabaseId = @DatabaseId
                        " + (excludeViews ? "AND td.IsView = 0" : "") + @"
                        ORDER BY do.SchemaName, do.ObjectName
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

                    var offset = (pageNumber - 1) * pageSize;
                    var tables = await connection.QueryAsync<TableDefinitionModel>(
                        tablesQuery, 
                        new { 
                            DatabaseId = databaseId,
                            Offset = offset,
                            PageSize = pageSize
                        });
                    
                    foreach (var table in tables)
                    {
                        // Get columns for each table
                        table.Columns = (await GetColumnsAsync(table.TableId)).ToList();
                        
                        // Set the DatabaseObject property
                        if (dbObjectsDict.TryGetValue(table.ObjectId, out var dbObject))
                        {
                            table.DatabaseObject = new DatabaseObjectModel
                            {
                                ObjectId = dbObject.ObjectId,
                                DatabaseId = dbObject.DatabaseId,
                                ObjectTypeId = dbObject.ObjectTypeId,
                                SchemaName = dbObject.SchemaName,
                                ObjectName = dbObject.ObjectName,
                                Description = dbObject.Description,
                                CreatedDate = dbObject.CreatedDate,
                                ModifiedDate = dbObject.ModifiedDate,
                                ObjectType = new ObjectTypeModel
                                {
                                    ObjectTypeId = dbObject.ObjectTypeId,
                                    TypeName = dbObject.ObjectTypeName
                                }
                            };
                        }
                    }
                    
                    return (tables, totalCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged tables for database ID {DatabaseId}", databaseId);
                throw;
            }
        }

        // Get table by ID
        public async Task<TableDefinitionModel> GetTableAsync(int tableId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // First, get the table
                    var tableQuery = @"
                        SELECT td.*
                        FROM dbo.TableDefinitions td
                        WHERE td.TableId = @TableId";

                    var table = await connection.QueryFirstOrDefaultAsync<TableDefinitionModel>(tableQuery, new { TableId = tableId });
                    
                    if (table != null)
                    {
                        // Get the database object
                        var dbObjectQuery = @"
                            SELECT do.*, ot.TypeName as ObjectTypeName, db.DatabaseName
                            FROM dbo.DatabaseObjects do
                            JOIN dbo.ObjectTypes ot ON do.ObjectTypeId = ot.ObjectTypeId
                            JOIN dbo.Databases db ON do.DatabaseId = db.DatabaseId
                            WHERE do.ObjectId = @ObjectId";
                            
                        var dbObject = await connection.QueryFirstOrDefaultAsync<dynamic>(dbObjectQuery, new { ObjectId = table.ObjectId });
                        
                        if (dbObject != null)
                        {
                            table.DatabaseObject = new DatabaseObjectModel
                            {
                                ObjectId = dbObject.ObjectId,
                                DatabaseId = dbObject.DatabaseId,
                                ObjectTypeId = dbObject.ObjectTypeId,
                                SchemaName = dbObject.SchemaName,
                                ObjectName = dbObject.ObjectName,
                                Description = dbObject.Description,
                                CreatedDate = dbObject.CreatedDate,
                                ModifiedDate = dbObject.ModifiedDate,
                                ObjectType = new ObjectTypeModel
                                {
                                    ObjectTypeId = dbObject.ObjectTypeId,
                                    TypeName = dbObject.ObjectTypeName
                                },
                                Database = new DatabaseModel
                                {
                                    DatabaseId = dbObject.DatabaseId,
                                    DatabaseName = dbObject.DatabaseName
                                }
                            };
                        }
                        
                        // Get columns for the table
                        table.Columns = (await GetColumnsAsync(table.TableId)).ToList();
                        
                        // Get relationships
                        table.ParentRelationships = (await GetTableRelationshipsAsync(tableId, true)).ToList();
                        table.ChildRelationships = (await GetTableRelationshipsAsync(tableId, false)).ToList();
                    }
                    
                    return table;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving table with ID {TableId}", tableId);
                throw;
            }
        }

        // Get columns for a table
        public async Task<IEnumerable<ColumnDefinitionModel>> GetColumnsAsync(int tableId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"
                        SELECT cd.*, dt.TypeName, dt.Description AS DataTypeDescription, dt.IsNumeric, dt.IsTextual, dt.IsDateTime, dt.IsBinary
                        FROM dbo.ColumnDefinitions cd
                        JOIN dbo.DataTypes dt ON cd.DataTypeId = dt.DataTypeId
                        WHERE cd.TableId = @TableId
                        ORDER BY cd.OrdinalPosition";

                    var columns = await connection.QueryAsync<dynamic>(query, new { TableId = tableId });
                    
                    var result = new List<ColumnDefinitionModel>();
                    foreach (var col in columns)
                    {
                        var column = new ColumnDefinitionModel
                        {
                            ColumnId = col.ColumnId,
                            TableId = col.TableId,
                            ColumnName = col.ColumnName,
                            OrdinalPosition = col.OrdinalPosition,
                            DataTypeId = col.DataTypeId,
                            MaxLength = col.MaxLength,
                            Precision = col.Precision,
                            Scale = col.Scale,
                            IsNullable = col.IsNullable,
                            HasDefault = col.HasDefault,
                            DefaultValue = col.DefaultValue,
                            Description = col.Description,
                            CreatedDate = col.CreatedDate,
                            ModifiedDate = col.ModifiedDate,
                            // Properly populate the DataType navigation property
                            DataType = new DataTypeModel
                            {
                                DataTypeId = col.DataTypeId,
                                TypeName = col.TypeName,
                                Description = col.DataTypeDescription,
                                IsNumeric = col.IsNumeric,
                                IsTextual = col.IsTextual,
                                IsDateTime = col.IsDateTime,
                                IsBinary = col.IsBinary
                            }
                        };
                        
                        // Get constraints for each column
                        column.Constraints = (await GetColumnConstraintsAsync(column.ColumnId)).ToList();
                        result.Add(column);
                    }
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving columns for table ID {TableId}", tableId);
                throw;
            }
        }

        // Get constraints for a column
        public async Task<IEnumerable<ColumnConstraintModel>> GetColumnConstraintsAsync(int columnId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"
                        SELECT cc.*, ct.TypeName, ct.Description AS ConstraintTypeDescription
                        FROM dbo.ColumnConstraints cc
                        JOIN dbo.ConstraintTypes ct ON cc.ConstraintTypeId = ct.ConstraintTypeId
                        WHERE cc.ColumnId = @ColumnId
                        ORDER BY ct.TypeName";

                    return await connection.QueryAsync<ColumnConstraintModel>(query, new { ColumnId = columnId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving constraints for column ID {ColumnId}", columnId);
                throw;
            }
        }

        // Get relationships for a table (as parent or child)
        public async Task<IEnumerable<TableRelationshipModel>> GetTableRelationshipsAsync(int tableId, bool isParent)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = isParent
                        ? @"
                            SELECT tr.*, rt.TypeName, rt.Description AS RelationshipTypeDescription,
                                   parent_do.SchemaName AS ParentSchemaName, parent_do.ObjectName AS ParentTableName,
                                   child_do.SchemaName AS ChildSchemaName, child_do.ObjectName AS ChildTableName,
                                   parent_cd.ColumnName AS ParentColumnName, child_cd.ColumnName AS ChildColumnName
                            FROM dbo.TableRelationships tr
                            JOIN dbo.RelationshipTypes rt ON tr.RelationshipTypeId = rt.RelationshipTypeId
                            JOIN dbo.TableDefinitions parent_td ON tr.ParentTableId = parent_td.TableId
                            JOIN dbo.TableDefinitions child_td ON tr.ChildTableId = child_td.TableId
                            JOIN dbo.DatabaseObjects parent_do ON parent_td.ObjectId = parent_do.ObjectId
                            JOIN dbo.DatabaseObjects child_do ON child_td.ObjectId = child_do.ObjectId
                            JOIN dbo.ColumnDefinitions parent_cd ON tr.ParentColumnId = parent_cd.ColumnId
                            JOIN dbo.ColumnDefinitions child_cd ON tr.ChildColumnId = child_cd.ColumnId
                            WHERE tr.ParentTableId = @TableId
                            ORDER BY child_do.SchemaName, child_do.ObjectName"
                        : @"
                            SELECT tr.*, rt.TypeName, rt.Description AS RelationshipTypeDescription,
                                   parent_do.SchemaName AS ParentSchemaName, parent_do.ObjectName AS ParentTableName,
                                   child_do.SchemaName AS ChildSchemaName, child_do.ObjectName AS ChildTableName,
                                   parent_cd.ColumnName AS ParentColumnName, child_cd.ColumnName AS ChildColumnName
                            FROM dbo.TableRelationships tr
                            JOIN dbo.RelationshipTypes rt ON tr.RelationshipTypeId = rt.RelationshipTypeId
                            JOIN dbo.TableDefinitions parent_td ON tr.ParentTableId = parent_td.TableId
                            JOIN dbo.TableDefinitions child_td ON tr.ChildTableId = child_td.TableId
                            JOIN dbo.DatabaseObjects parent_do ON parent_td.ObjectId = parent_do.ObjectId
                            JOIN dbo.DatabaseObjects child_do ON child_td.ObjectId = child_do.ObjectId
                            JOIN dbo.ColumnDefinitions parent_cd ON tr.ParentColumnId = parent_cd.ColumnId
                            JOIN dbo.ColumnDefinitions child_cd ON tr.ChildColumnId = child_cd.ColumnId
                            WHERE tr.ChildTableId = @TableId
                            ORDER BY parent_do.SchemaName, parent_do.ObjectName";

                    return await connection.QueryAsync<TableRelationshipModel>(query, new { TableId = tableId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving relationships for table ID {TableId} as {Role}", 
                    tableId, isParent ? "parent" : "child");
                throw;
            }
        }

        // Get all data types
        public async Task<IEnumerable<DataTypeModel>> GetDataTypesAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryAsync<DataTypeModel>(
                        "SELECT * FROM dbo.DataTypes ORDER BY TypeName");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data types");
                throw;
            }
        }

        // Get all constraint types
        public async Task<IEnumerable<ConstraintTypeModel>> GetConstraintTypesAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryAsync<ConstraintTypeModel>(
                        "SELECT * FROM dbo.ConstraintTypes ORDER BY TypeName");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving constraint types");
                throw;
            }
        }

        // Get all relationship types
        public async Task<IEnumerable<RelationshipTypeModel>> GetRelationshipTypesAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryAsync<RelationshipTypeModel>(
                        "SELECT * FROM dbo.RelationshipTypes ORDER BY TypeName");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving relationship types");
                throw;
            }
        }

        // Add or get server
        public async Task<int> AddOrGetServerAsync(string serverName, string description = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Check if server already exists
                    var existingServer = await connection.QueryFirstOrDefaultAsync<ServerModel>(
                        "SELECT * FROM dbo.Servers WHERE ServerName = @ServerName",
                        new { ServerName = serverName });
                    
                    if (existingServer != null)
                    {
                        return existingServer.ServerId;
                    }
                    
                    // Insert new server
                    var serverId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO dbo.Servers (ServerName, Description, CreatedDate, ModifiedDate)
                          VALUES (@ServerName, @Description, GETDATE(), GETDATE());
                          SELECT SCOPE_IDENTITY();",
                        new { ServerName = serverName, Description = description });
                    
                    return serverId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or getting server {ServerName}", serverName);
                throw;
            }
        }

        // Add or get database
        public async Task<int> AddOrGetDatabaseAsync(int serverId, string databaseName, string description = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Check if database already exists
                    var existingDb = await connection.QueryFirstOrDefaultAsync<DatabaseModel>(
                        "SELECT * FROM dbo.Databases WHERE ServerId = @ServerId AND DatabaseName = @DatabaseName",
                        new { ServerId = serverId, DatabaseName = databaseName });
                    
                    if (existingDb != null)
                    {
                        return existingDb.DatabaseId;
                    }
                    
                    // Insert new database
                    var databaseId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO dbo.Databases (ServerId, DatabaseName, Description, CreatedDate, ModifiedDate)
                          VALUES (@ServerId, @DatabaseName, @Description, GETDATE(), GETDATE());
                          SELECT SCOPE_IDENTITY();",
                        new { ServerId = serverId, DatabaseName = databaseName, Description = description });
                    
                    return databaseId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or getting database {DatabaseName} for server ID {ServerId}", databaseName, serverId);
                throw;
            }
        }

        // Add or get database object
        public async Task<int> AddOrGetDatabaseObjectAsync(int databaseId, int objectTypeId, string schemaName, string objectName, string description = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Check if object already exists
                    var existingObject = await connection.QueryFirstOrDefaultAsync<DatabaseObjectModel>(
                        @"SELECT * FROM dbo.DatabaseObjects 
                          WHERE DatabaseId = @DatabaseId AND SchemaName = @SchemaName AND ObjectName = @ObjectName",
                        new { DatabaseId = databaseId, SchemaName = schemaName, ObjectName = objectName });
                    
                    if (existingObject != null)
                    {
                        return existingObject.ObjectId;
                    }
                    
                    // Insert new object
                    var objectId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO dbo.DatabaseObjects (DatabaseId, ObjectTypeId, SchemaName, ObjectName, Description, CreatedDate, ModifiedDate)
                          VALUES (@DatabaseId, @ObjectTypeId, @SchemaName, @ObjectName, @Description, GETDATE(), GETDATE());
                          SELECT SCOPE_IDENTITY();",
                        new { DatabaseId = databaseId, ObjectTypeId = objectTypeId, SchemaName = schemaName, ObjectName = objectName, Description = description });
                    
                    return objectId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or getting database object {Schema}.{Object} for database ID {DatabaseId}", 
                    schemaName, objectName, databaseId);
                throw;
            }
        }

        // Add or get table definition
        public async Task<int> AddOrGetTableDefinitionAsync(int objectId, bool isView, long? rowCount = null)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Check if table definition already exists
                    var existingTable = await connection.QueryFirstOrDefaultAsync<TableDefinitionModel>(
                        "SELECT * FROM dbo.TableDefinitions WHERE ObjectId = @ObjectId",
                        new { ObjectId = objectId });
                    
                    if (existingTable != null)
                    {
                        // Update row count if provided
                        if (rowCount.HasValue)
                        {
                            await connection.ExecuteAsync(
                                "UPDATE dbo.TableDefinitions SET [RowCount] = @RowCount, ModifiedDate = GETDATE() WHERE TableId = @TableId",
                                new { TableId = existingTable.TableId, RowCount = rowCount });
                        }
                        
                        return existingTable.TableId;
                    }
                    
                    // Insert new table definition
                    var tableId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO dbo.TableDefinitions (ObjectId, IsView, [RowCount], HasPrimaryKey, HasClusteredIndex, CreatedDate, ModifiedDate)
                          VALUES (@ObjectId, @IsView, @RowCount, 0, 0, GETDATE(), GETDATE());
                          SELECT SCOPE_IDENTITY();",
                        new { ObjectId = objectId, IsView = isView, RowCount = rowCount });
                    
                    return tableId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or getting table definition for object ID {ObjectId}", objectId);
                throw;
            }
        }

        // Add or get column definition
        public async Task<int> AddOrGetColumnDefinitionAsync(int tableId, string columnName, int ordinalPosition, 
            int dataTypeId, int? maxLength, int? precision, int? scale, bool isNullable, bool hasDefault, 
            string defaultValue, string description = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Check if column already exists
                    var existingColumn = await connection.QueryFirstOrDefaultAsync<ColumnDefinitionModel>(
                        "SELECT * FROM dbo.ColumnDefinitions WHERE TableId = @TableId AND ColumnName = @ColumnName",
                        new { TableId = tableId, ColumnName = columnName });
                    
                    if (existingColumn != null)
                    {
                        // Update column if needed
                        await connection.ExecuteAsync(
                            @"UPDATE dbo.ColumnDefinitions 
                              SET OrdinalPosition = @OrdinalPosition, 
                                  DataTypeId = @DataTypeId,
                                  MaxLength = @MaxLength,
                                  Precision = @Precision,
                                  Scale = @Scale,
                                  IsNullable = @IsNullable,
                                  HasDefault = @HasDefault,
                                  DefaultValue = @DefaultValue,
                                  Description = @Description,
                                  ModifiedDate = GETDATE()
                              WHERE ColumnId = @ColumnId",
                            new { 
                                ColumnId = existingColumn.ColumnId,
                                OrdinalPosition = ordinalPosition,
                                DataTypeId = dataTypeId,
                                MaxLength = maxLength,
                                Precision = precision,
                                Scale = scale,
                                IsNullable = isNullable,
                                HasDefault = hasDefault,
                                DefaultValue = defaultValue,
                                Description = description
                            });
                        
                        return existingColumn.ColumnId;
                    }
                    
                    // Insert new column
                    var columnId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO dbo.ColumnDefinitions (
                              TableId, ColumnName, OrdinalPosition, DataTypeId, MaxLength, 
                              Precision, Scale, IsNullable, HasDefault, DefaultValue, 
                              Description, CreatedDate, ModifiedDate)
                        VALUES (
                            @TableId, @ColumnName, @OrdinalPosition, @DataTypeId, @MaxLength,
                            @Precision, @Scale, @IsNullable, @HasDefault, @DefaultValue,
                            @Description, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();",
                        new { 
                            TableId = tableId,
                            ColumnName = columnName,
                            OrdinalPosition = ordinalPosition,
                            DataTypeId = dataTypeId,
                            MaxLength = maxLength,
                            Precision = precision,
                            Scale = scale,
                            IsNullable = isNullable,
                            HasDefault = hasDefault,
                            DefaultValue = defaultValue,
                            Description = description
                        });
                        
                    return columnId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or getting column {ColumnName} for table ID {TableId}", columnName, tableId);
                throw;
            }
        }

        // Add or get table relationship
        public async Task<int> AddOrGetTableRelationshipAsync(int parentTableId, int childTableId, 
            int parentColumnId, int childColumnId, int relationshipTypeId, string constraintName, string description = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Check if relationship already exists
                    var existingRelationship = await connection.QueryFirstOrDefaultAsync<TableRelationshipModel>(
                        @"SELECT * FROM dbo.TableRelationships 
                          WHERE ParentTableId = @ParentTableId AND ChildTableId = @ChildTableId 
                            AND ParentColumnId = @ParentColumnId AND ChildColumnId = @ChildColumnId",
                        new { 
                            ParentTableId = parentTableId, 
                            ChildTableId = childTableId,
                            ParentColumnId = parentColumnId,
                            ChildColumnId = childColumnId
                        });
                    
                    if (existingRelationship != null)
                    {
                        return existingRelationship.RelationshipId;
                    }
                    
                    // Insert new relationship
                    var relationshipId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO dbo.TableRelationships (
                              ParentTableId, ChildTableId, ParentColumnId, ChildColumnId,
                              RelationshipTypeId, ConstraintName, Description, CreatedDate, ModifiedDate)
                        VALUES (
                            @ParentTableId, @ChildTableId, @ParentColumnId, @ChildColumnId,
                            @RelationshipTypeId, @ConstraintName, @Description, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();",
                        new { 
                            ParentTableId = parentTableId,
                            ChildTableId = childTableId,
                            ParentColumnId = parentColumnId,
                            ChildColumnId = childColumnId,
                            RelationshipTypeId = relationshipTypeId,
                            ConstraintName = constraintName,
                            Description = description
                        });
                    
                    return relationshipId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or getting relationship between tables {ParentTableId} and {ChildTableId}", 
                    parentTableId, childTableId);
                throw;
            }
        }

        // Get or add data type ID
        public async Task<int> GetOrAddDataTypeIdAsync(string typeName, string description = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Check if data type already exists
                    var existingType = await connection.QueryFirstOrDefaultAsync<DataTypeModel>(
                        "SELECT * FROM dbo.DataTypes WHERE TypeName = @TypeName",
                        new { TypeName = typeName });
                    
                    if (existingType != null)
                    {
                        return existingType.DataTypeId;
                    }
                    
                    // Determine type properties
                    bool isNumeric = IsNumericType(typeName);
                    bool isTextual = IsTextualType(typeName);
                    bool isDateTime = IsDateTimeType(typeName);
                    bool isBinary = IsBinaryType(typeName);
                    
                    // Insert new data type
                    var dataTypeId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO dbo.DataTypes (
                              TypeName, Description, IsNumeric, IsTextual, IsDateTime, IsBinary, CreatedDate, ModifiedDate)
                        VALUES (
                            @TypeName, @Description, @IsNumeric, @IsTextual, @IsDateTime, @IsBinary, GETDATE(), GETDATE());
                            SELECT SCOPE_IDENTITY();",
                        new { 
                            TypeName = typeName,
                            Description = description,
                            IsNumeric = isNumeric,
                            IsTextual = isTextual,
                            IsDateTime = isDateTime,
                            IsBinary = isBinary
                        });
                    
                    return dataTypeId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or adding data type {TypeName}", typeName);
                throw;
            }
        }

        // Get table ID by schema and name
        public async Task<int> GetTableIdBySchemaAndNameAsync(int databaseId, string schema, string tableName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT td.TableId
                        FROM dbo.TableDefinitions td
                        JOIN dbo.DatabaseObjects do ON td.ObjectId = do.ObjectId
                        WHERE do.DatabaseId = @DatabaseId
                          AND do.SchemaName = @Schema
                          AND do.ObjectName = @TableName";
                    
                    var tableId = await connection.ExecuteScalarAsync<int?>(query, 
                        new { DatabaseId = databaseId, Schema = schema, TableName = tableName }) ?? 0;
                    
                    return tableId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table ID for {Schema}.{TableName} in database {DatabaseId}", 
                    schema, tableName, databaseId);
                throw;
            }
        }

        // Get column ID by table and name
        public async Task<int> GetColumnIdByTableAndNameAsync(int tableId, string columnName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT ColumnId
                        FROM dbo.ColumnDefinitions
                        WHERE TableId = @TableId
                          AND ColumnName = @ColumnName";
                    
                    var columnId = await connection.ExecuteScalarAsync<int?>(query, 
                        new { TableId = tableId, ColumnName = columnName }) ?? 0;
                    
                    return columnId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting column ID for {ColumnName} in table {TableId}", 
                    columnName, tableId);
                throw;
            }
        }

        // Helper methods for data type classification
        private bool IsNumericType(string typeName)
        {
            var numericTypes = new[] { 
                "int", "bigint", "smallint", "tinyint", "decimal", "numeric", 
                "float", "real", "money", "smallmoney" 
            };
            return numericTypes.Contains(typeName.ToLower());
        }

        private bool IsTextualType(string typeName)
        {
            var textTypes = new[] { 
                "char", "varchar", "text", "nchar", "nvarchar", "ntext", 
                "xml", "sysname" 
            };
            return textTypes.Contains(typeName.ToLower());
        }

        private bool IsDateTimeType(string typeName)
        {
            var dateTypes = new[] { 
                "date", "time", "datetime", "datetime2", "smalldatetime", 
                "datetimeoffset", "timestamp" 
            };
            return dateTypes.Contains(typeName.ToLower());
        }

        private bool IsBinaryType(string typeName)
        {
            var binaryTypes = new[] { 
                "binary", "varbinary", "image", "rowversion" 
            };
            return binaryTypes.Contains(typeName.ToLower());
        }

        // Get all lineage types
        public async Task<IEnumerable<LineageTypeModel>> GetLineageTypesAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryAsync<LineageTypeModel>(
                        "SELECT * FROM dbo.LineageTypes ORDER BY TypeName");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lineage types");
                throw;
            }
        }

        // Get lineage type by ID
        public async Task<LineageTypeModel> GetLineageTypeAsync(int lineageTypeId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryFirstOrDefaultAsync<LineageTypeModel>(
                        "SELECT * FROM dbo.LineageTypes WHERE LineageTypeId = @LineageTypeId",
                        new { LineageTypeId = lineageTypeId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lineage type with ID {LineageTypeId}", lineageTypeId);
                throw;
            }
        }

        // Get all data lineage relationships
        public async Task<IEnumerable<DataLineageModel>> GetDataLineageRelationshipsAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"
                        SELECT dl.*, lt.TypeName, lt.Description AS LineageTypeDescription
                        FROM dbo.DataLineage dl
                        JOIN dbo.LineageTypes lt ON dl.LineageTypeId = lt.LineageTypeId
                        ORDER BY dl.LineageId";

                    var lineageRelationships = await connection.QueryAsync<dynamic>(query);
                    var result = new List<DataLineageModel>();

                    foreach (var lr in lineageRelationships)
                    {
                        var lineage = new DataLineageModel
                        {
                            LineageId = lr.LineageId,
                            LineageTypeId = lr.LineageTypeId,
                            SourceDatabaseId = lr.SourceDatabaseId,
                            SourceTableId = lr.SourceTableId,
                            SourceColumnId = lr.SourceColumnId,
                            TargetDatabaseId = lr.TargetDatabaseId,
                            TargetTableId = lr.TargetTableId,
                            TargetColumnId = lr.TargetColumnId,
                            TransformationDescription = lr.TransformationDescription,
                            CreatedDate = lr.CreatedDate,
                            ModifiedDate = lr.ModifiedDate,
                            LineageType = new LineageTypeModel
                            {
                                LineageTypeId = lr.LineageTypeId,
                                TypeName = lr.TypeName,
                                Description = lr.LineageTypeDescription
                            }
                        };

                        // Load related entities if they exist
                        if (lr.SourceDatabaseId != null)
                            lineage.SourceDatabase = await GetDatabaseAsync((int)lr.SourceDatabaseId);
                        
                        if (lr.SourceTableId != null)
                            lineage.SourceTable = await GetTableAsync((int)lr.SourceTableId);
                        
                        if (lr.SourceColumnId != null)
                            lineage.SourceColumn = await GetColumnAsync((int)lr.SourceColumnId);
                        
                        if (lr.TargetDatabaseId != null)
                            lineage.TargetDatabase = await GetDatabaseAsync((int)lr.TargetDatabaseId);
                        
                        if (lr.TargetTableId != null)
                            lineage.TargetTable = await GetTableAsync((int)lr.TargetTableId);
                        
                        if (lr.TargetColumnId != null)
                            lineage.TargetColumn = await GetColumnAsync((int)lr.TargetColumnId);

                        result.Add(lineage);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data lineage relationships");
                throw;
            }
        }

        // Get data lineage relationship by ID
        public async Task<DataLineageModel> GetDataLineageRelationshipAsync(int lineageId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"
                        SELECT dl.*, lt.TypeName, lt.Description AS LineageTypeDescription
                        FROM dbo.DataLineage dl
                        JOIN dbo.LineageTypes lt ON dl.LineageTypeId = lt.LineageTypeId
                        WHERE dl.LineageId = @LineageId";

                    var lr = await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { LineageId = lineageId });
                    
                    if (lr == null)
                        return null;

                    var lineage = new DataLineageModel
                    {
                        LineageId = lr.LineageId,
                        LineageTypeId = lr.LineageTypeId,
                        SourceDatabaseId = lr.SourceDatabaseId,
                        SourceTableId = lr.SourceTableId,
                        SourceColumnId = lr.SourceColumnId,
                        TargetDatabaseId = lr.TargetDatabaseId,
                        TargetTableId = lr.TargetTableId,
                        TargetColumnId = lr.TargetColumnId,
                        TransformationDescription = lr.TransformationDescription,
                        CreatedDate = lr.CreatedDate,
                        ModifiedDate = lr.ModifiedDate,
                        LineageType = new LineageTypeModel
                        {
                            LineageTypeId = lr.LineageTypeId,
                            TypeName = lr.TypeName,
                            Description = lr.LineageTypeDescription
                        }
                    };

                    // Load related entities if they exist
                    if (lr.SourceDatabaseId != null)
                        lineage.SourceDatabase = await GetDatabaseAsync((int)lr.SourceDatabaseId);
                    
                    if (lr.SourceTableId != null)
                        lineage.SourceTable = await GetTableAsync((int)lr.SourceTableId);
                    
                    if (lr.SourceColumnId != null)
                        lineage.SourceColumn = await GetColumnAsync((int)lr.SourceColumnId);
                    
                    if (lr.TargetDatabaseId != null)
                        lineage.TargetDatabase = await GetDatabaseAsync((int)lr.TargetDatabaseId);
                    
                    if (lr.TargetTableId != null)
                        lineage.TargetTable = await GetTableAsync((int)lr.TargetTableId);
                    
                    if (lr.TargetColumnId != null)
                        lineage.TargetColumn = await GetColumnAsync((int)lr.TargetColumnId);

                    return lineage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data lineage relationship with ID {LineageId}", lineageId);
                throw;
            }
        }

        // Get column by ID
        public async Task<ColumnDefinitionModel> GetColumnAsync(int columnId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"
                        SELECT cd.*, dt.TypeName, dt.Description AS DataTypeDescription, dt.IsNumeric, dt.IsTextual, dt.IsDateTime, dt.IsBinary
                        FROM dbo.ColumnDefinitions cd
                        JOIN dbo.DataTypes dt ON cd.DataTypeId = dt.DataTypeId
                        WHERE cd.ColumnId = @ColumnId";

                    var col = await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { ColumnId = columnId });
                    
                    if (col == null)
                        return null;

                    var column = new ColumnDefinitionModel
                    {
                        ColumnId = col.ColumnId,
                        TableId = col.TableId,
                        ColumnName = col.ColumnName,
                        OrdinalPosition = col.OrdinalPosition,
                        DataTypeId = col.DataTypeId,
                        MaxLength = col.MaxLength,
                        Precision = col.Precision,
                        Scale = col.Scale,
                        IsNullable = col.IsNullable,
                        HasDefault = col.HasDefault,
                        DefaultValue = col.DefaultValue,
                        Description = col.Description,
                        CreatedDate = col.CreatedDate,
                        ModifiedDate = col.ModifiedDate,
                        DataType = new DataTypeModel
                        {
                            DataTypeId = col.DataTypeId,
                            TypeName = col.TypeName,
                            Description = col.DataTypeDescription,
                            IsNumeric = col.IsNumeric,
                            IsTextual = col.IsTextual,
                            IsDateTime = col.IsDateTime,
                            IsBinary = col.IsBinary
                        }
                    };

                    // Get table for the column
                    column.Table = await GetTableAsync(column.TableId);

                    return column;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving column with ID {ColumnId}", columnId);
                throw;
            }
        }

        // Get lineage relationships for a database
        public async Task<IEnumerable<DataLineageModel>> GetDatabaseLineageAsync(int databaseId, bool isSource)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = isSource
                        ? "SELECT * FROM dbo.DataLineage WHERE SourceDatabaseId = @DatabaseId"
                        : "SELECT * FROM dbo.DataLineage WHERE TargetDatabaseId = @DatabaseId";

                    var lineageIds = await connection.QueryAsync<int>(
                        $"{query} ORDER BY LineageId",
                        new { DatabaseId = databaseId },
                        commandType: CommandType.Text);

                    var result = new List<DataLineageModel>();
                    foreach (var id in lineageIds)
                    {
                        var lineage = await GetDataLineageRelationshipAsync(id);
                        if (lineage != null)
                            result.Add(lineage);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lineage for database ID {DatabaseId} as {Role}", 
                    databaseId, isSource ? "source" : "target");
                throw;
            }
        }

        // Get lineage relationships for a table
        public async Task<IEnumerable<DataLineageModel>> GetTableLineageAsync(int tableId, bool isSource)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = isSource
                        ? "SELECT * FROM dbo.DataLineage WHERE SourceTableId = @TableId"
                        : "SELECT * FROM dbo.DataLineage WHERE TargetTableId = @TableId";

                    var lineageIds = await connection.QueryAsync<int>(
                        $"{query} ORDER BY LineageId",
                        new { TableId = tableId },
                        commandType: CommandType.Text);

                    var result = new List<DataLineageModel>();
                    foreach (var id in lineageIds)
                    {
                        var lineage = await GetDataLineageRelationshipAsync(id);
                        if (lineage != null)
                            result.Add(lineage);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lineage for table ID {TableId} as {Role}", 
                    tableId, isSource ? "source" : "target");
                throw;
            }
        }

        // Get lineage relationships for a column
        public async Task<IEnumerable<DataLineageModel>> GetColumnLineageAsync(int columnId, bool isSource)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = isSource
                        ? "SELECT * FROM dbo.DataLineage WHERE SourceColumnId = @ColumnId"
                        : "SELECT * FROM dbo.DataLineage WHERE TargetColumnId = @ColumnId";

                    var lineageIds = await connection.QueryAsync<int>(
                        $"{query} ORDER BY LineageId",
                        new { ColumnId = columnId },
                        commandType: CommandType.Text);

                    var result = new List<DataLineageModel>();
                    foreach (var id in lineageIds)
                    {
                        var lineage = await GetDataLineageRelationshipAsync(id);
                        if (lineage != null)
                            result.Add(lineage);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lineage for column ID {ColumnId} as {Role}", 
                    columnId, isSource ? "source" : "target");
                throw;
            }
        }

        // Add a new data lineage relationship
        public async Task<int> AddDataLineageAsync(
            int lineageTypeId, 
            int? sourceDatabaseId, int? sourceTableId, int? sourceColumnId,
            int? targetDatabaseId, int? targetTableId, int? targetColumnId,
            string transformationDescription)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Validate that at least one source and one target is specified
                    if ((sourceDatabaseId == null && sourceTableId == null && sourceColumnId == null) ||
                        (targetDatabaseId == null && targetTableId == null && targetColumnId == null))
                    {
                        throw new ArgumentException("At least one source and one target must be specified");
                    }
                    
                    // Validate that transformation description is provided for Aggregated and Computation types
                    if ((lineageTypeId == 2 || lineageTypeId == 3) && string.IsNullOrWhiteSpace(transformationDescription))
                    {
                        throw new ArgumentException("Transformation description is required for Aggregated and Computation lineage types");
                    }
                    
                    var query = @"
                        INSERT INTO dbo.DataLineage (
                            LineageTypeId, 
                            SourceDatabaseId, SourceTableId, SourceColumnId,
                            TargetDatabaseId, TargetTableId, TargetColumnId,
                            TransformationDescription,
                            CreatedDate, ModifiedDate)
                        VALUES (
                            @LineageTypeId, 
                            @SourceDatabaseId, @SourceTableId, @SourceColumnId,
                            @TargetDatabaseId, @TargetTableId, @TargetColumnId,
                            @TransformationDescription,
                            GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();";
                    
                    var lineageId = await connection.ExecuteScalarAsync<int>(query, new { 
                        LineageTypeId = lineageTypeId, 
                        SourceDatabaseId = sourceDatabaseId, 
                        SourceTableId = sourceTableId, 
                        SourceColumnId = sourceColumnId,
                        TargetDatabaseId = targetDatabaseId, 
                        TargetTableId = targetTableId, 
                        TargetColumnId = targetColumnId,
                        TransformationDescription = transformationDescription
                    });
                    
                    return lineageId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding data lineage relationship");
                throw;
            }
        }

        // Update an existing data lineage relationship
        public async Task UpdateDataLineageAsync(
            int lineageId,
            int lineageTypeId, 
            int? sourceDatabaseId, int? sourceTableId, int? sourceColumnId,
            int? targetDatabaseId, int? targetTableId, int? targetColumnId,
            string transformationDescription)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Validate that at least one source and one target is specified
                    if ((sourceDatabaseId == null && sourceTableId == null && sourceColumnId == null) ||
                        (targetDatabaseId == null && targetTableId == null && targetColumnId == null))
                    {
                        throw new ArgumentException("At least one source and one target must be specified");
                    }
                    
                    // Validate that transformation description is provided for Aggregated and Computation types
                    if ((lineageTypeId == 2 || lineageTypeId == 3) && string.IsNullOrWhiteSpace(transformationDescription))
                    {
                        throw new ArgumentException("Transformation description is required for Aggregated and Computation lineage types");
                    }
                    
                    var query = @"
                        UPDATE dbo.DataLineage SET
                            LineageTypeId = @LineageTypeId, 
                            SourceDatabaseId = @SourceDatabaseId, 
                            SourceTableId = @SourceTableId, 
                            SourceColumnId = @SourceColumnId,
                            TargetDatabaseId = @TargetDatabaseId, 
                            TargetTableId = @TargetTableId, 
                            TargetColumnId = @TargetColumnId,
                            TransformationDescription = @TransformationDescription,
                            ModifiedDate = GETDATE()
                        WHERE LineageId = @LineageId";
                    
                    await connection.ExecuteAsync(query, new { 
                        LineageId = lineageId,
                        LineageTypeId = lineageTypeId, 
                        SourceDatabaseId = sourceDatabaseId, 
                        SourceTableId = sourceTableId, 
                        SourceColumnId = sourceColumnId,
                        TargetDatabaseId = targetDatabaseId, 
                        TargetTableId = targetTableId, 
                        TargetColumnId = targetColumnId,
                        TransformationDescription = transformationDescription
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating data lineage relationship with ID {LineageId}", lineageId);
                throw;
            }
        }

        // Delete a data lineage relationship
        public async Task DeleteDataLineageAsync(int lineageId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(
                        "DELETE FROM dbo.DataLineage WHERE LineageId = @LineageId",
                        new { LineageId = lineageId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting data lineage relationship with ID {LineageId}", lineageId);
                throw;
            }
        }
    }
} 