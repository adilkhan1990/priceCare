CREATE TABLE [dbo].[gco_submn_country_vw] (
    [ID]                     INT            NULL,
    [SOURCE_SYSTEM_NAME]     NVARCHAR (MAX) NULL,
    [COUNTRY_ID]             INT            NULL,
    [COUNTRY_NAME]           NVARCHAR (MAX) NULL,
    [COUNTRY_CODE]           NVARCHAR (MAX) NULL,
    [IS_ACTIVE]              NVARCHAR (MAX) NULL,
    [IS_DELETE]              NVARCHAR (MAX) NULL,
    [SOURCE_SYS_CREATED_BY]  NVARCHAR (MAX) NULL,
    [SOURCE_SYS_CREATED_DT]  DATETIME       NULL,
    [SOURCE_SYS_LAST_UPD_BY] NVARCHAR (MAX) NULL,
    [SOURCE_SYS_LAST_UPD_DT] DATETIME       NULL,
    [ODS_CREATE_BY]          NVARCHAR (MAX) NULL,
    [ODS_CREATE_DT]          DATETIME       NULL,
    [ODS_LAST_UPD_BY]        NVARCHAR (MAX) NULL,
    [ODS_LAST_UPD_DT]        DATETIME       NULL
);

