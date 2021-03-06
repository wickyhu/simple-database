
USE [Northwind]
GO

-- Drop tables
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[FK_DbAuditDetail_DbAudit]') AND type = 'F')
ALTER TABLE [dbo].[DbAuditDetail] DROP CONSTRAINT [FK_DbAuditDetail_DbAudit]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[FK_TestEntity_History_BsStatus]') AND type = 'F')
ALTER TABLE [dbo].[TestEntity_Audit] DROP CONSTRAINT [FK_TestEntity_History_BsStatus]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[FK_TestEntity_BsStatus]') AND type = 'F')
ALTER TABLE [dbo].[TestEntity] DROP CONSTRAINT [FK_TestEntity_BsStatus]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[BsStatus]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[BsStatus]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DbAuditDetail]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[DbAuditDetail]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DbAudit]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[DbAudit]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DbIdTable]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[DbIdTable]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[TestEntity_Audit]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[TestEntity_Audit]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[TestEntity]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE [dbo].[TestEntity]
GO

-- Create tables 
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BsStatus](
	[id] [int] NOT NULL,
	[name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_BsStatus] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DbAudit](
	[ID] [bigint] NOT NULL,
	[CREATEDON] [datetime] NOT NULL,
	[USERNAME] [nvarchar](50) NULL,
	[ACTION] [nvarchar](50) NULL,
	[functionid] [int] NULL,
	[TABLENAME] [nvarchar](50) NULL,
	[IPADDRESS] [nvarchar](50) NULL,
	[HOSTNAME] [nvarchar](50) NULL,
	[OSUSER] [nvarchar](50) NULL,
	[remarks] [nvarchar](max) NULL,
 CONSTRAINT [PK_DbAudit1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DbAuditDetail](
	[ID] [bigint] NOT NULL,
	[AUDITID] [bigint] NOT NULL,
	[FIELDNAME] [nvarchar](50) NULL,
	[OLDVALUE] [nvarchar](1000) NULL,
	[NEWVALUE] [nvarchar](1000) NULL,
	[CHANGED] [smallint] NOT NULL,
 CONSTRAINT [PK_DbAuditDetail] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DbIdTable](
	[IdIndex] [int] NOT NULL,
	[NextId] [bigint] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TestEntity](
	[id] [int] NOT NULL,
	[loginname] [varchar](50) NULL,
	[passwd] [varchar](50) NULL,
	[photo] [varbinary](max) NULL,
	[text] [nvarchar](max) NULL,
	[guid] [uniqueidentifier] NULL,
	[status] [int] NULL,
	[createdon] [datetime] NULL,
	[updatedon] [datetime] NULL,
	[createdby] [nvarchar](50) NULL,
	[updatedby] [nvarchar](50) NULL,
	[AddressStreet] [nvarchar](500) NULL,
	[AddressCity] [nvarchar](50) NULL,
	[AddressPostCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_TestEntity] PRIMARY KEY CLUSTERED 
(
	[id] ASC
) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TestEntity_Audit](
	[auditid] [int] NOT NULL,
	[id] [int] NOT NULL,
	[loginname] [varchar](50) NULL,
	[passwd] [varchar](50) NULL,
	[photo] [varbinary](max) NULL,
	[text] [nvarchar](max) NULL,
	[guid] [uniqueidentifier] NULL,
	[status] [int] NULL,
	[createdon] [datetime] NULL,
	[updatedon] [datetime] NULL,
	[createdby] [nvarchar](50) NULL,
	[updatedby] [nvarchar](50) NULL,
	[AddressStreet] [nvarchar](500) NULL,
	[AddressCity] [nvarchar](50) NULL,
	[AddressPostCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_TestEntity_History] PRIMARY KEY CLUSTERED 
(
	[auditid] ASC
) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[DbAuditDetail]  WITH NOCHECK ADD  CONSTRAINT [FK_DbAuditDetail_DbAudit] FOREIGN KEY([AUDITID])
REFERENCES [dbo].[DbAudit] ([ID])
GO
ALTER TABLE [dbo].[DbAuditDetail] CHECK CONSTRAINT [FK_DbAuditDetail_DbAudit]
GO
ALTER TABLE [dbo].[TestEntity]  WITH CHECK ADD  CONSTRAINT [FK_TestEntity_BsStatus] FOREIGN KEY([status])
REFERENCES [dbo].[BsStatus] ([id])
GO
ALTER TABLE [dbo].[TestEntity] CHECK CONSTRAINT [FK_TestEntity_BsStatus]
GO
ALTER TABLE [dbo].[TestEntity_Audit]  WITH CHECK ADD  CONSTRAINT [FK_TestEntity_History_BsStatus] FOREIGN KEY([status])
REFERENCES [dbo].[BsStatus] ([id])
GO
ALTER TABLE [dbo].[TestEntity_Audit] CHECK CONSTRAINT [FK_TestEntity_History_BsStatus]
GO

-- insert records 
insert into DbIdTable (IdIndex,NextId) values (0, 1000)
insert into DbIdTable (IdIndex,NextId) values (1, 1000)
GO

insert into BsStatus (Id, Name) values (0, 'Approved')
insert into BsStatus (Id, Name) values (1, 'Not Approved')
insert into BsStatus (Id, Name) values (2, 'Cancelled')
GO

