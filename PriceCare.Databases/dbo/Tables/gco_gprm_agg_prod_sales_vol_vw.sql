CREATE TABLE [dbo].[gco_gprm_agg_prod_sales_vol_vw] (
    [ID]                            INT            NULL,
    [SCENARIO_NAME]                 NVARCHAR (MAX) NULL,
    [VOLUME_TYPE]                   VARCHAR (50)   NULL,
    [MONTH]                         INT            NULL,
    [YEAR]                          INT            NULL,
    [SALES_ORG]                     NVARCHAR (MAX) NULL,
    [COUNTRY_NAME]                  NVARCHAR (MAX) NULL,
    [INDICATION]                    NVARCHAR (MAX) NULL,
    [PRODUCT_FAMILY]                NVARCHAR (MAX) NULL,
    [VOLUME_COM]                    FLOAT (53)     NULL,
    [VOLUME_FREE]                   FLOAT (53)     NULL,
    [CY_ACTUAL_RATE_USD_AMOUNT]     FLOAT (53)     NULL,
    [FUTURE_ACTUAL_RATE_USD_AMOUNT] FLOAT (53)     NULL,
    [CY_BUDGET_RATE_USD_AMOUNT]     FLOAT (53)     NULL,
    [FUTURE_BUDGET_RATE_USD_AMOUNT] FLOAT (53)     NULL
);

