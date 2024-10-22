CREATE TABLE [dbo].[gco_gprm_cntry_price_cal_vw] (
    [ID]                     INT            NULL,
    [YEAR]                   INT            NULL,
    [MONTH]                  INT            NULL,
    [TYPE_OF_CUT]            NVARCHAR (MAX) NULL,
    [CUSTOM_TYPE_OF_CUT]     NVARCHAR (MAX) NULL,
    [SIZE_OF_CUT_PCT]        FLOAT (53)     NULL,
    [PROBABILITY_OF_CUT_PCT] FLOAT (53)     NULL,
    [PRODUCT_FAMILY]         NVARCHAR (MAX) NULL,
    [COUNTRY]                NVARCHAR (MAX) NULL,
    [ISO_COUNTRY_2_CHAR]     NVARCHAR (MAX) NULL,
    [COMMENTS]               NVARCHAR (MAX) NULL
);

