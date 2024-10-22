﻿CREATE TABLE [dbo].[gco_gprm_cntry_price_ref_vw] (
    [ID]                             INT            NULL,
    [REFING_COUNTRY_NAME]            NVARCHAR (MAX) NULL,
    [REFING_CNTRY_REF_COUNTRY_CNT]   INT            NULL,
    [REFING_CNTRY_FOCUS]             NVARCHAR (MAX) NULL,
    [REFING_CNTRY_FORMULA_USED_GOVT] NVARCHAR (MAX) NULL,
    [REFING_CNTRY_IS_FORMAL]         NVARCHAR (MAX) NULL,
    [REFING_CNTRY_FREQUENCY]         NVARCHAR (MAX) NULL,
    [REFING_CNTRY_MONTHS_BTW_REF]    NVARCHAR (MAX) NULL,
    [REFING_CNTRY_REF_MONTH]         NVARCHAR (MAX) NULL,
    [REFING_CNTRY_UNIT_OF_CMPRSN]    NVARCHAR (MAX) NULL,
    [REFING_CNTRY_BASIS_OF_CMPRSN]   NVARCHAR (MAX) NULL,
    [REFED_CNTRY_COUNTRY_NAME]       NVARCHAR (MAX) NULL,
    [REFED_CNTRY_PRICELIST_TYPE]     NVARCHAR (MAX) NULL,
    [REFED_CNTRY_PRICELIST_CHANNEL]  NVARCHAR (MAX) NULL,
    [REFED_CNTRY_PRICELIST_PUBLIC]   NVARCHAR (MAX) NULL,
    [REFED_CNTRY_PRICELIST_OFFCL]    NVARCHAR (MAX) NULL,
    [REFED_CNTRY_MISSING_PCK_PRICE]  NVARCHAR (MAX) NULL,
    [REFED_CNTRY_DOCUMENT_REQUIRED]  NVARCHAR (MAX) NULL
);
