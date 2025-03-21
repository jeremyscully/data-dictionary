USE [master]
GO
/****** Object:  Database [datadict]    Script Date: 3/11/2025 6:47:29 PM ******/
CREATE DATABASE [datadict]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'datadict', FILENAME = N'D:\databases\datadict.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'datadict_log', FILENAME = N'D:\databases\datadict_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [datadict] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [datadict].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [datadict] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [datadict] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [datadict] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [datadict] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [datadict] SET ARITHABORT OFF 
GO
ALTER DATABASE [datadict] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [datadict] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [datadict] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [datadict] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [datadict] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [datadict] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [datadict] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [datadict] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [datadict] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [datadict] SET  DISABLE_BROKER 
GO
ALTER DATABASE [datadict] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [datadict] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [datadict] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [datadict] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [datadict] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [datadict] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [datadict] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [datadict] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [datadict] SET  MULTI_USER 
GO
ALTER DATABASE [datadict] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [datadict] SET DB_CHAINING OFF 
GO
ALTER DATABASE [datadict] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [datadict] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [datadict] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [datadict] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [datadict] SET QUERY_STORE = OFF
GO
USE [datadict]
GO
/****** Object:  Table [dbo].[ColumnConstraints]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ColumnConstraints](
	[ConstraintId] [int] IDENTITY(1,1) NOT NULL,
	[ColumnId] [int] NOT NULL,
	[ConstraintTypeId] [int] NOT NULL,
	[ConstraintName] [nvarchar](128) NOT NULL,
	[Definition] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ConstraintId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ColumnDefinitions]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ColumnDefinitions](
	[ColumnId] [int] IDENTITY(1,1) NOT NULL,
	[TableId] [int] NOT NULL,
	[ColumnName] [nvarchar](128) NOT NULL,
	[OrdinalPosition] [int] NOT NULL,
	[DataTypeId] [int] NOT NULL,
	[MaxLength] [int] NULL,
	[Precision] [int] NULL,
	[Scale] [int] NULL,
	[IsNullable] [bit] NOT NULL,
	[HasDefault] [bit] NOT NULL,
	[DefaultValue] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ColumnId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ColumnDefinitions_TableColumn] UNIQUE NONCLUSTERED 
(
	[TableId] ASC,
	[ColumnName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ConstraintTypes]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConstraintTypes](
	[ConstraintTypeId] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ConstraintTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ConstraintTypes_TypeName] UNIQUE NONCLUSTERED 
(
	[TypeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DatabaseObjects]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DatabaseObjects](
	[ObjectId] [int] IDENTITY(1,1) NOT NULL,
	[DatabaseId] [int] NOT NULL,
	[ObjectTypeId] [int] NOT NULL,
	[SchemaName] [nvarchar](128) NOT NULL,
	[ObjectName] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ObjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DatabaseObjects_FullName] UNIQUE NONCLUSTERED 
(
	[DatabaseId] ASC,
	[ObjectTypeId] ASC,
	[SchemaName] ASC,
	[ObjectName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Databases]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Databases](
	[DatabaseId] [int] IDENTITY(1,1) NOT NULL,
	[ServerId] [int] NOT NULL,
	[DatabaseName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DatabaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Databases_ServerDatabase] UNIQUE NONCLUSTERED 
(
	[ServerId] ASC,
	[DatabaseName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataLineage]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataLineage](
	[LineageId] [int] IDENTITY(1,1) NOT NULL,
	[LineageTypeId] [int] NOT NULL,
	[SourceDatabaseId] [int] NULL,
	[SourceTableId] [int] NULL,
	[SourceColumnId] [int] NULL,
	[TargetDatabaseId] [int] NULL,
	[TargetTableId] [int] NULL,
	[TargetColumnId] [int] NULL,
	[TransformationDescription] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[LineageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataTypes]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataTypes](
	[DataTypeId] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[IsNumeric] [bit] NOT NULL,
	[IsTextual] [bit] NOT NULL,
	[IsDateTime] [bit] NOT NULL,
	[IsBinary] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DataTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DataTypes_TypeName] UNIQUE NONCLUSTERED 
(
	[TypeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LineageTypes]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LineageTypes](
	[LineageTypeId] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[LineageTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ObjectTypes]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ObjectTypes](
	[ObjectTypeId] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ObjectTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ObjectTypes_TypeName] UNIQUE NONCLUSTERED 
(
	[TypeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RelationshipTypes]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RelationshipTypes](
	[RelationshipTypeId] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[RelationshipTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RelationshipTypes_TypeName] UNIQUE NONCLUSTERED 
(
	[TypeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Servers]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Servers](
	[ServerId] [int] IDENTITY(1,1) NOT NULL,
	[ServerName] [nvarchar](255) NOT NULL,
	[ServerType] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[ConnectionString] [nvarchar](1000) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ServerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Servers_ServerName] UNIQUE NONCLUSTERED 
(
	[ServerName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TableDefinitions]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TableDefinitions](
	[TableId] [int] IDENTITY(1,1) NOT NULL,
	[ObjectId] [int] NOT NULL,
	[IsView] [bit] NOT NULL,
	[RowCount] [bigint] NULL,
	[HasPrimaryKey] [bit] NOT NULL,
	[HasClusteredIndex] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TableRelationships]    Script Date: 3/11/2025 6:47:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TableRelationships](
	[RelationshipId] [int] IDENTITY(1,1) NOT NULL,
	[RelationshipTypeId] [int] NOT NULL,
	[ParentTableId] [int] NOT NULL,
	[ChildTableId] [int] NOT NULL,
	[ParentColumnId] [int] NOT NULL,
	[ChildColumnId] [int] NOT NULL,
	[ConstraintName] [nvarchar](128) NULL,
	[IsEnforced] [bit] NOT NULL,
	[DeleteAction] [nvarchar](20) NULL,
	[UpdateAction] [nvarchar](20) NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RelationshipId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_DataLineage_LineageTypeId]    Script Date: 3/11/2025 6:47:29 PM ******/
CREATE NONCLUSTERED INDEX [IX_DataLineage_LineageTypeId] ON [dbo].[DataLineage]
(
	[LineageTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ColumnConstraints] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ColumnConstraints] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[ColumnDefinitions] ADD  DEFAULT ((0)) FOR [HasDefault]
GO
ALTER TABLE [dbo].[ColumnDefinitions] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ColumnDefinitions] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[DatabaseObjects] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[DatabaseObjects] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[Databases] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Databases] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[DataLineage] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[DataLineage] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[DataTypes] ADD  DEFAULT ((0)) FOR [IsNumeric]
GO
ALTER TABLE [dbo].[DataTypes] ADD  DEFAULT ((0)) FOR [IsTextual]
GO
ALTER TABLE [dbo].[DataTypes] ADD  DEFAULT ((0)) FOR [IsDateTime]
GO
ALTER TABLE [dbo].[DataTypes] ADD  DEFAULT ((0)) FOR [IsBinary]
GO
ALTER TABLE [dbo].[LineageTypes] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[LineageTypes] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[Servers] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Servers] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[TableDefinitions] ADD  DEFAULT ((0)) FOR [IsView]
GO
ALTER TABLE [dbo].[TableDefinitions] ADD  DEFAULT ((0)) FOR [HasPrimaryKey]
GO
ALTER TABLE [dbo].[TableDefinitions] ADD  DEFAULT ((0)) FOR [HasClusteredIndex]
GO
ALTER TABLE [dbo].[TableDefinitions] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[TableDefinitions] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[TableRelationships] ADD  DEFAULT ((1)) FOR [IsEnforced]
GO
ALTER TABLE [dbo].[TableRelationships] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[TableRelationships] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[ColumnConstraints]  WITH CHECK ADD  CONSTRAINT [FK_ColumnConstraints_ColumnDefinitions] FOREIGN KEY([ColumnId])
REFERENCES [dbo].[ColumnDefinitions] ([ColumnId])
GO
ALTER TABLE [dbo].[ColumnConstraints] CHECK CONSTRAINT [FK_ColumnConstraints_ColumnDefinitions]
GO
ALTER TABLE [dbo].[ColumnConstraints]  WITH CHECK ADD  CONSTRAINT [FK_ColumnConstraints_ConstraintTypes] FOREIGN KEY([ConstraintTypeId])
REFERENCES [dbo].[ConstraintTypes] ([ConstraintTypeId])
GO
ALTER TABLE [dbo].[ColumnConstraints] CHECK CONSTRAINT [FK_ColumnConstraints_ConstraintTypes]
GO
ALTER TABLE [dbo].[ColumnDefinitions]  WITH CHECK ADD  CONSTRAINT [FK_ColumnDefinitions_DataTypes] FOREIGN KEY([DataTypeId])
REFERENCES [dbo].[DataTypes] ([DataTypeId])
GO
ALTER TABLE [dbo].[ColumnDefinitions] CHECK CONSTRAINT [FK_ColumnDefinitions_DataTypes]
GO
ALTER TABLE [dbo].[ColumnDefinitions]  WITH CHECK ADD  CONSTRAINT [FK_ColumnDefinitions_TableDefinitions] FOREIGN KEY([TableId])
REFERENCES [dbo].[TableDefinitions] ([TableId])
GO
ALTER TABLE [dbo].[ColumnDefinitions] CHECK CONSTRAINT [FK_ColumnDefinitions_TableDefinitions]
GO
ALTER TABLE [dbo].[DatabaseObjects]  WITH CHECK ADD  CONSTRAINT [FK_DatabaseObjects_Databases] FOREIGN KEY([DatabaseId])
REFERENCES [dbo].[Databases] ([DatabaseId])
GO
ALTER TABLE [dbo].[DatabaseObjects] CHECK CONSTRAINT [FK_DatabaseObjects_Databases]
GO
ALTER TABLE [dbo].[DatabaseObjects]  WITH CHECK ADD  CONSTRAINT [FK_DatabaseObjects_ObjectTypes] FOREIGN KEY([ObjectTypeId])
REFERENCES [dbo].[ObjectTypes] ([ObjectTypeId])
GO
ALTER TABLE [dbo].[DatabaseObjects] CHECK CONSTRAINT [FK_DatabaseObjects_ObjectTypes]
GO
ALTER TABLE [dbo].[Databases]  WITH CHECK ADD  CONSTRAINT [FK_Databases_Servers] FOREIGN KEY([ServerId])
REFERENCES [dbo].[Servers] ([ServerId])
GO
ALTER TABLE [dbo].[Databases] CHECK CONSTRAINT [FK_Databases_Servers]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [FK_DataLineage_LineageTypes] FOREIGN KEY([LineageTypeId])
REFERENCES [dbo].[LineageTypes] ([LineageTypeId])
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [FK_DataLineage_LineageTypes]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [FK_DataLineage_SourceColumn] FOREIGN KEY([SourceColumnId])
REFERENCES [dbo].[ColumnDefinitions] ([ColumnId])
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [FK_DataLineage_SourceColumn]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [FK_DataLineage_SourceDatabase] FOREIGN KEY([SourceDatabaseId])
REFERENCES [dbo].[Databases] ([DatabaseId])
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [FK_DataLineage_SourceDatabase]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [FK_DataLineage_SourceTable] FOREIGN KEY([SourceTableId])
REFERENCES [dbo].[TableDefinitions] ([TableId])
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [FK_DataLineage_SourceTable]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [FK_DataLineage_TargetColumn] FOREIGN KEY([TargetColumnId])
REFERENCES [dbo].[ColumnDefinitions] ([ColumnId])
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [FK_DataLineage_TargetColumn]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [FK_DataLineage_TargetDatabase] FOREIGN KEY([TargetDatabaseId])
REFERENCES [dbo].[Databases] ([DatabaseId])
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [FK_DataLineage_TargetDatabase]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [FK_DataLineage_TargetTable] FOREIGN KEY([TargetTableId])
REFERENCES [dbo].[TableDefinitions] ([TableId])
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [FK_DataLineage_TargetTable]
GO
ALTER TABLE [dbo].[TableDefinitions]  WITH CHECK ADD  CONSTRAINT [FK_TableDefinitions_DatabaseObjects] FOREIGN KEY([ObjectId])
REFERENCES [dbo].[DatabaseObjects] ([ObjectId])
GO
ALTER TABLE [dbo].[TableDefinitions] CHECK CONSTRAINT [FK_TableDefinitions_DatabaseObjects]
GO
ALTER TABLE [dbo].[TableRelationships]  WITH CHECK ADD  CONSTRAINT [FK_TableRelationships_ChildColumn] FOREIGN KEY([ChildColumnId])
REFERENCES [dbo].[ColumnDefinitions] ([ColumnId])
GO
ALTER TABLE [dbo].[TableRelationships] CHECK CONSTRAINT [FK_TableRelationships_ChildColumn]
GO
ALTER TABLE [dbo].[TableRelationships]  WITH CHECK ADD  CONSTRAINT [FK_TableRelationships_ChildTable] FOREIGN KEY([ChildTableId])
REFERENCES [dbo].[TableDefinitions] ([TableId])
GO
ALTER TABLE [dbo].[TableRelationships] CHECK CONSTRAINT [FK_TableRelationships_ChildTable]
GO
ALTER TABLE [dbo].[TableRelationships]  WITH CHECK ADD  CONSTRAINT [FK_TableRelationships_ParentColumn] FOREIGN KEY([ParentColumnId])
REFERENCES [dbo].[ColumnDefinitions] ([ColumnId])
GO
ALTER TABLE [dbo].[TableRelationships] CHECK CONSTRAINT [FK_TableRelationships_ParentColumn]
GO
ALTER TABLE [dbo].[TableRelationships]  WITH CHECK ADD  CONSTRAINT [FK_TableRelationships_ParentTable] FOREIGN KEY([ParentTableId])
REFERENCES [dbo].[TableDefinitions] ([TableId])
GO
ALTER TABLE [dbo].[TableRelationships] CHECK CONSTRAINT [FK_TableRelationships_ParentTable]
GO
ALTER TABLE [dbo].[TableRelationships]  WITH CHECK ADD  CONSTRAINT [FK_TableRelationships_RelationshipTypes] FOREIGN KEY([RelationshipTypeId])
REFERENCES [dbo].[RelationshipTypes] ([RelationshipTypeId])
GO
ALTER TABLE [dbo].[TableRelationships] CHECK CONSTRAINT [FK_TableRelationships_RelationshipTypes]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [CK_DataLineage_Source] CHECK  (([SourceDatabaseId] IS NOT NULL OR [SourceTableId] IS NOT NULL OR [SourceColumnId] IS NOT NULL))
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [CK_DataLineage_Source]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [CK_DataLineage_Target] CHECK  (([TargetDatabaseId] IS NOT NULL OR [TargetTableId] IS NOT NULL OR [TargetColumnId] IS NOT NULL))
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [CK_DataLineage_Target]
GO
ALTER TABLE [dbo].[DataLineage]  WITH CHECK ADD  CONSTRAINT [CK_DataLineage_TransformationDescription] CHECK  (([LineageTypeId]=(1) OR ([LineageTypeId]=(3) OR [LineageTypeId]=(2)) AND [TransformationDescription] IS NOT NULL))
GO
ALTER TABLE [dbo].[DataLineage] CHECK CONSTRAINT [CK_DataLineage_TransformationDescription]
GO
USE [master]
GO
ALTER DATABASE [datadict] SET  READ_WRITE 
GO
