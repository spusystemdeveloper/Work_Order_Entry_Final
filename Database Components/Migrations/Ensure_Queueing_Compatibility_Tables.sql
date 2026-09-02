/*
    Work Order Entry queue compatibility migration

    Purpose
      The deployed SOD_sp_InsertQoute and SOD_sp_InsertQouteEntry procedures
      differ between branches in how they write Queueing/QueueingItems.
      Queueing headers identify the OPIS recall owner for both Work Orders and
      Sales Quotations. QueueingItems are retained only when the deployed
      branch enables picking/preparation processing.

    Run once in each branch RMS database before deploying the unified build.
    This script is idempotent: existing tables and data are preserved.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Queueing', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Queueing
        (
            id bigint IDENTITY(1,1) NOT NULL,
            OrderID bigint NULL,
            Status int NULL,
            GroupTo int NULL,
            OPIS varchar(max) NULL,
            CONSTRAINT PK_Queueing PRIMARY KEY CLUSTERED (id)
        );
    END;

    IF OBJECT_ID(N'dbo.QueueingItems', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.QueueingItems
        (
            id bigint IDENTITY(1,1) NOT NULL,
            QueueingID bigint NULL,
            ItemID int NULL,
            QtyPre int NULL,
            Picker nvarchar(50) NULL,
            Status varchar(50) NULL,
            PickLoc varchar(max) NULL,
            CONSTRAINT PK_QueueingItems PRIMARY KEY CLUSTERED (id)
        );
    END;

    IF COL_LENGTH(N'dbo.Queueing', N'id') IS NULL
       OR COL_LENGTH(N'dbo.Queueing', N'OrderID') IS NULL
       OR COL_LENGTH(N'dbo.Queueing', N'Status') IS NULL
       OR COL_LENGTH(N'dbo.Queueing', N'GroupTo') IS NULL
       OR COL_LENGTH(N'dbo.Queueing', N'OPIS') IS NULL
       OR COL_LENGTH(N'dbo.QueueingItems', N'id') IS NULL
       OR COL_LENGTH(N'dbo.QueueingItems', N'QueueingID') IS NULL
       OR COL_LENGTH(N'dbo.QueueingItems', N'ItemID') IS NULL
       OR COL_LENGTH(N'dbo.QueueingItems', N'QtyPre') IS NULL
       OR COL_LENGTH(N'dbo.QueueingItems', N'Picker') IS NULL
       OR COL_LENGTH(N'dbo.QueueingItems', N'Status') IS NULL
       OR COL_LENGTH(N'dbo.QueueingItems', N'PickLoc') IS NULL
    BEGIN
        RAISERROR(
            'Existing Queueing tables do not match the required Work Order schema. Review them manually; no tables were replaced.',
            16,
            1);
    END;

    COMMIT TRANSACTION;
    PRINT 'Queueing compatibility tables are ready.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage nvarchar(4000);
    SELECT @ErrorMessage = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
