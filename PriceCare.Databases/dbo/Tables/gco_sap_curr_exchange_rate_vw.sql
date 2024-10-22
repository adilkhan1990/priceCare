CREATE TABLE [dbo].[gco_sap_curr_exchange_rate_vw] (
    [ID]             INT            NULL,
    [RATE_TYPE]      NVARCHAR (MAX) NULL,
    [FROM_CURR]      NVARCHAR (MAX) NULL,
    [TO_CURR]        NVARCHAR (MAX) NULL,
    [EFFECTIVE_DATE] DATETIME       NULL,
    [EXCHANGE_RATE]  FLOAT (53)     NULL,
    [CREATE_DATE]    DATETIME       NULL,
    [UPDATE_DATE]    DATETIME       NULL
);

