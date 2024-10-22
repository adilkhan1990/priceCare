define(['pricecare/listToSales/module'], function (module) {
    'use strict';
    module.controller('ListToSalesController', ['$scope', 'helperService', 'productService', 'countryService', 'versionService', '$q', 'listToSalesService', '$location',
            'userService', 'validate', '$routeParams', 'loadService', '$modal', 'excelService','tableService',
        function ($scope, helperService, productService, countryService, versionService, $q, listToSalesService, $location,
            userService, validate, $routeParams, loadService, $modal, excelService, tableService) {

            $scope.loadId = $routeParams.loadId;

            $scope.validate = validate;
            $scope.listToSales = [];
            var usableProductsAndGeographyIds;

            $scope.cellsToValidateSmartResolution1 = 0;
            $scope.cellsToValidateSmartResolution2 = 0;

            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage,
                countriesId: [],
                productsId: [],
                validate: validate
            };

            var countryFilter = {
                type: helperService.FieldTypes.DynamicPop,
                name: 'countries',
                properties:
                {
                    class: 't-custom-select-boxed',
                    directive: 'op-selection-tree-popup-limited',
                    items: [],
                    getText: function () {
                        if (countryFilter.properties.items.length > 0) {
                            var upItemSelected = _.find(countryFilter.properties.items, function(upItem) {
                                return upItem.value != null;
                            });
                            var subItemSelected = _.where(upItemSelected.items, { value: true });
                            var result = upItemSelected.text + "(" + subItemSelected.length + ")";
                        } else {
                            return "";
                        }
                        
                        return result;
                    }
                }
            };

            var versionFilter = {
                type: helperService.FieldTypes.Select,
                name: "versions",
                properties: {
                    class: 't-custom-select-text',
                    items:[]
                }
            };

            $scope.firstVersionSelected = function () {
                if (versionFilter.properties.items && versionFilter.properties.items.length > 0) {
                    return versionFilter.properties.items[0].selected == true;
                }

                return false;
            }

            $scope.smartResolutionOptions = {
                predescription: 'rows vary by',
                postdescription: 'from previous version',
                actionText: 'Accept',
                actionRejectText: 'Reject',
                displayReject: true,
                affectedCells: 0,
                rulesData: [
                    {
                        type: 5,
                        value: 0
                    }
                ],
                callback: function() {
                    calculateSmartResolution();
                },
                apply: function() {
                    applySmartResolution(applyLoadOnCell);
                },
                reject: function() {
                    applySmartResolution(rejectLoadOnCell);
                }
        };

            var calculateSmartResolution = function () {

                if ($scope.smartResolutionOptions.rulesData.length == 0) {
                    $scope.smartResolutionOptions.affectedCells = 0;
                } else {
                    $scope.cellsToValidateSmartResolution1 = 0;
                    tableService.foreachRows($scope.table, function (row) {
                        if (row.listToSales && (row.listToSales.tag == helperService.LoadTag.Edited || row.listToSales.tag == helperService.LoadTag.Loaded)) {
                            var canResolve = true;

                            for (var i = 0; i < $scope.smartResolutionOptions.rulesData.length; i++) {
                                var rule = $scope.smartResolutionOptions.rulesData[i];
                                var baseRule = _.find(helperService.RuleTypes, function (r) { return r.id == rule.type; });

                                if (!baseRule.check(row.listToSales.percentageVariationFromX, rule.value / 100)) {
                                    canResolve = false;
                                    break;
                                }
                            }

                            if (canResolve) {
                                $scope.cellsToValidateSmartResolution1++;
                            }
                        }
                    });

                    $scope.smartResolutionOptions.affectedCells = $scope.cellsToValidateSmartResolution1;
                }
            };

            var applySmartResolution = function (applyOnChanges) {
                if ($scope.smartResolutionOptions.rulesData.length == 0) {
                    return;
                } else {
                    tableService.foreachRows($scope.table, function (row) {
                        if (row.listToSales && (row.listToSales.tag == helperService.LoadTag.Edited || row.listToSales.tag == helperService.LoadTag.Loaded)) {
                            var canResolve = true;

                            for (var i = 0; i < $scope.smartResolutionOptions.rulesData.length; i++) {
                                var rule = $scope.smartResolutionOptions.rulesData[i];
                                var baseRule = _.find(helperService.RuleTypes, function (r) { return r.id == rule.type; });

                                if (!baseRule.check(row.listToSales.percentageVariationFromX, rule.value / 100)) {
                                    canResolve = false;
                                    break;
                                }
                            }
                            if (canResolve) {
                                applyOnChanges(null, row);
                            }
                        }
                    });

                    calculateSmartResolution();
                }
            };

            $scope.smartDeleteResolutionOptions = {
                predescription: 'rows correspond to previous version with no new values',
                actionText: 'Keep',
                actionRejectText: 'Reject',
                displayReject: true,
                hideToggle: true,
                affectedCells: 0,
                callback: function () {
                    calculateDeleteSmartResolution();
                },
                apply: function () {
                    applyDeleteSmartResolution(applyLoadOnCell);
                },
                reject: function() {
                    applyDeleteSmartResolution(rejectLoadOnCell);
                }
            };

            var calculateDeleteSmartResolution = function () {
                $scope.cellsToValidateSmartResolution2 = 0;
                tableService.foreachRows($scope.table, function (row) {
                    if (row.listToSales && row.listToSales.tag == helperService.LoadTag.Deleted) {
                        $scope.cellsToValidateSmartResolution2++;
                    }
                });

                $scope.smartDeleteResolutionOptions.affectedCells = $scope.cellsToValidateSmartResolution2;

            };

            var applyDeleteSmartResolution = function (applyChanges) {

                tableService.foreachRows($scope.table, function (row) {
                    if (row.listToSales && row.listToSales.tag == helperService.LoadTag.Deleted) {                        
                        applyChanges(null, row);
                    }
                });

                calculateDeleteSmartResolution();
            };

            var productFilter = {
                type: helperService.FieldTypes.Select,
                name: 'products',
                properties: {
                    class: 't-custom-select-text',
                }
            };

            var prepareListToSales = function(listToSales) {
                var countries = [];

                var listToSalesTmp = _.groupBy(listToSales, 'geographyName');
                for (var geographyName in listToSalesTmp) {
                    var newCountry = {
                        id: listToSalesTmp[geographyName][0].geographyId,
                        name: geographyName,
                        items: []
                    };
                    for (var i = 0; i < listToSalesTmp[geographyName].length; i++) {
                        newCountry.items.push(listToSalesTmp[geographyName][i]);
                    }
                    countries.push(newCountry);
                }

                return countries;
            };

            var updateParentWithChildrenValues = function(parentRow) {
                var children = _.filter($scope.table.rows, function(row) {
                    return row.parentRow != undefined && row.parentRow.cells[0].value == parentRow.cells[0].value;
                });
                
                var total = 0;
                children.forEach(function(child) {
                    total += child.cells[2].value;
                });
                parentRow.cells[2].value = total;
            };

            var initTable = function (listToSales) {
                var columnNames = ["Country", "Segment", "Market", "ASP", "Segment Impacted", "Impact Timing"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                var countries = prepareListToSales(listToSales);

                for (var i = 0; i < countries.length; i++) {
                    var sumMarketPercentage = 0;

                    var countryRow = {
                        listToSales: countries[i],
                        expanded: (validate)? true: false,
                        cells: [
                            {
                                value: countries[i].name,
                                actions: [
                                    {
                                        text: 'Expand',
                                        class: (validate) ? "table-expandable-row icon-expanded" : 'table-expandable-row icon-collapsed',
                                        click: function (cell, row, table, action) {
                                            row.expanded = row.expanded ? false : true;
                                            action.text = row.expanded ? "Collapse" : "Expand";
                                            action.class = row.expanded ? "table-expandable-row icon-expanded" : "table-expandable-row icon-collapsed";
                                        }
                                    }
                                ]
                            },
                            { value: "Add a new segment" },
                            { format: helperService.formatPercentage },
                            { value: "" },
                            { value: "" },
                            { value: "" }
                        ]
                    };

                    var childRows = [];
                    for (var j = 0; j < countries[i].items.length; j++) {
                        sumMarketPercentage += countries[i].items[j].marketPercentage;

                        var currentListToSales = countries[i].items[j];
                        var countryCell = { value: "" };
                        var segmentCell = { value: currentListToSales.segmentName };
                        var marketCell = {
                            value: currentListToSales.marketPercentage,
                            format: helperService.formatPercentage,
                            ready: validate && currentListToSales.tag == helperService.LoadTag.Empty
                        };
                        var aspCell = {
                            value: currentListToSales.asp,
                            format: helperService.formatNumber
                        };
                        var impactCell = {
                            value: currentListToSales.impactPercentage,
                            format: helperService.formatPercentage,
                        };
                        var impactTimingCell = {
                            actions: [
                                {
                                    text: 'Details',
                                    class: '',
                                    click: function(cell, row) {
                                        var versionId = getSelectedVersionId();
                                        var path = '/data/dimensions/listToSalesImpact/' + row.original.geographyId +
                                            '/' + row.original.productId + '/' + versionId;
                                        $location.path(path);
                                    }
                                }
                            ]
                        };

                        var newRow = {
                            original: currentListToSales,
                            listToSales: helperService.clone(currentListToSales),
                            parentRow: countryRow,
                            cells: [countryCell, segmentCell, marketCell, aspCell, impactCell, impactTimingCell]
                        };

                        // style the row
                        if (validate) {
                            if (!currentListToSales.tag == "") {
                                countryCell.styleLoad = {
                                    border: helperService.borderLeft,
                                    background: currentListToSales.tag == helperService.LoadTag.Deleted
                                };
                                segmentCell.styleLoad = {
                                    border: helperService.borderMiddle,
                                    background: currentListToSales.tag == helperService.LoadTag.Deleted
                                };
                                marketCell.styleLoad = {
                                    border: helperService.borderMiddle,
                                    background: currentListToSales.tag == helperService.LoadTag.Deleted
                                };
                                aspCell.styleLoad = {
                                    border: helperService.borderMiddle,
                                    background: currentListToSales.tag == helperService.LoadTag.Deleted
                                };
                                impactCell.styleLoad = {
                                    border: helperService.borderMiddle,
                                    background: currentListToSales.tag == helperService.LoadTag.Deleted
                                };
                                impactTimingCell.styleLoad = {
                                    border: helperService.borderRight,
                                    background: currentListToSales.tag == helperService.LoadTag.Deleted
                                };
                            }
                        }

                        // Enrich cells
                        if (!validate || currentListToSales.tag == helperService.LoadTag.Empty) {
                            initRowCellsRightMenu(newRow, currentListToSales);
                        } else {
                            countryCell.rightMenu = initRightMenuValidate(newRow, countryCell);
                            segmentCell.rightMenu = initRightMenuValidate(newRow, segmentCell);
                            marketCell.rightMenu = initRightMenuValidate(newRow, marketCell);
                            aspCell.rightMenu = initRightMenuValidate(newRow, aspCell);
                            impactCell.rightMenu = initRightMenuValidate(newRow, impactCell);
                            impactTimingCell.rightMenu = initRightMenuValidate(newRow, impactTimingCell);
                        }
                        if (!$scope.firstVersionSelected())
                        {
                            countryCell.blockRightMenu = true;
                            segmentCell.blockRightMenu = true;
                            marketCell.blockRightMenu = true;
                            aspCell.blockRightMenu = true;
                            impactCell.blockRightMenu = true;
                            impactTimingCell.blockRightMenu = true;
                        }
                        childRows.push(newRow);
                    }
                    countryRow.cells[2].value = sumMarketPercentage;
                    rows.push(countryRow);
                    rows = rows.concat(childRows);
                }

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getListToSales();
                        },
                        counterText: $scope.counter
                    }
                };
            };

            var initRowCellsRightMenu = function (row, currentListToSales) {
                row.cells[0].rightMenu = null;
                row.cells[1].rightMenu = null;
                row.cells[2].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: "Market percentage:",
                            cellValue: currentListToSales.marketPercentage * 100,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                allowDecimal: true,
                                areEquals: function (x, y) {
                                    return x == y;
                                },
                                isValid: function (field, cell, row, table) {
                                    var brothers = _.filter($scope.table.rows, function (rowTmp) {
                                        return rowTmp.parentRow != undefined && rowTmp.parentRow.cells[0].value == row.parentRow.cells[0].value
                                        && rowTmp.original.id != row.original.id;
                                    });
                                    var total = 0;
                                    var value = field.value / 100;
                                    if (brothers)
                                        brothers.forEach(function (brother) {
                                            total += brother.value;
                                        });
                                    total += value;
                                    return total >= 0.0 && total <= 1.0;
                                }
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            cell.value = fields[0].value / 100;
                            cell.rightMenu.fields[0].cellValue = fields[0].value;
                            cell.rightMenu.properties.showReverseEdit = true;
                            row.listToSales.marketPercentage = fields[0].value / 100;
                            updateParentWithChildrenValues(row.parentRow);
                            $scope.canSave = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.listToSales.marketPercentage = row.original.marketPercentage;
                            cell.edited = false;
                            cell.value = row.listToSales.marketPercentage;
                            cell.rightMenu.fields[0].cellValue = row.listToSales.marketPercentage * 100;
                            cell.rightMenu.properties.showReverseEdit = false;
                            updateParentWithChildrenValues(row.parentRow);
                            canSaveVersion();
                        }
                    }
                };

                row.cells[3].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: "Asp:",
                            cellValue: currentListToSales.asp,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                allowDecimal: true,
                                areEquals: function (x, y) {
                                    return x == y;
                                },
                                isValid: function (field, cell, row, table) {
                                    return field.value >= -1;
                                }
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            cell.value = parseFloat(fields[0].value);
                            cell.rightMenu.fields[0].cellValue = parseFloat(fields[0].value);
                            cell.rightMenu.properties.showReverseEdit = true;
                            row.listToSales.asp = parseFloat(fields[0].value);
                            $scope.canSave = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.listToSales.asp = row.original.asp;
                            cell.edited = false;
                            cell.value = row.listToSales.asp;
                            cell.rightMenu.fields[0].cellValue = row.listToSales.asp;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };
                row.cells[4].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: "Impact percentage:",
                            cellValue: currentListToSales.impactPercentage * 100,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                allowDecimal: true,
                                areEquals: function (x, y) {
                                    return x == y;
                                },
                                isValid: function (field, cell, row, table) {
                                    return field.value >= 0.0 && field.value <= 100.0;
                                }
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            cell.value = fields[0].value / 100;
                            cell.rightMenu.fields[0].cellValue = fields[0].value;
                            row.listToSales.impactPercentage = fields[0].value / 100;
                            cell.rightMenu.properties.showReverseEdit = true;
                            $scope.canSave = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.listToSales.impactPercentage = row.original.impactPercentage;
                            cell.edited = false;
                            cell.value = row.listToSales.impactPercentage;
                            cell.rightMenu.fields[0].cellValue = row.listToSales.impactPercentage;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };
                row.cells[5].rightMenu = null;
            };

            var initRightMenuValidate = function (row, cell) {
                var rightMenu = {
                    hideTextHelp: true,
                    fields: [
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Accept",
                                class: 'button button-border button-green button-icon icon-save text-align-center',
                                callback: function () {
                                    applyLoadOnCell(cell, row);
                                }
                            }
                        },
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Discard",
                                class: 'button button-border button-red button-icon icon-delete text-align-center',
                                callback: function () {
                                    rejectLoadOnCell(cell, row);
                                }
                            }
                        }
                    ],
                    properties: {
                        showReverseEdit: false,
                        showApply: false,
                    }
                };

                if (row.listToSales.tag == helperService.LoadTag.Edited && row.listToSales.oldValue) {
                    rightMenu.fields.unshift({
                        type: helperService.FieldTypes.Label,
                        properties: {
                            text: "OldValue: [Asp," + helperService.formatNumber(row.listToSales.oldValue.asp) + "]"
                        }
                    });
                }

                return rightMenu;
            };

            var rejectLoadOnCell = function (cell, row) {
                switch (row.listToSales.tag) {
                    case helperService.LoadTag.Loaded:
                        row.hide = true;
                        row.listToSales.active = false;
                        break;
                    case helperService.LoadTag.Deleted:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        row.listToSales.tag = helperService.LoadTag.Empty;
                        row.listToSales.active = true;
                        initRowCellsRightMenu(row, row.listToSales);
                        break;
                    case helperService.LoadTag.Edited:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        initRowCellsRightMenu(row, row.listToSales);
                        row.listToSales.asp = row.listToSales.oldValue.asp;
                        row.cells[3].value = row.listToSales.oldValue.asp;
                        row.cells[3].rightMenu.fields[0].cellValue = row.listToSales.oldValue.asp;
                        break;
                }
                row.cells.forEach(function (cell) {
                    cell.edited = true;
                    cell.ready = true;
                    cell.onClose();
                    canSaveVersion();
                });
            };

            var applyLoadOnCell = function (cell, row) {
                switch (row.listToSales.tag) {
                    case helperService.LoadTag.Loaded:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        row.listToSales.active = true;
                        row.listToSales.tag = null;
                        initRowCellsRightMenu(row, row.listToSales);
                        break;
                    case helperService.LoadTag.Deleted:
                        row.hide = true;
                        row.listToSales.active = false;
                        break;
                    case helperService.LoadTag.Edited:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        row.listToSales.tag = null;
                        row.listToSales.active = true;
                        initRowCellsRightMenu(row, row.listToSales);
                        break;
                }

                row.cells.forEach(function (cell) {
                    cell.edited = true;
                    cell.ready = true;
                    cell.onClose();
                    canSaveVersion();
                });
                
            }

            var prepareRegionAndCountriesForFilter = function (data) {
                var result = [];

                data.forEach(function (region, i) {
                    var newRegion = {
                        id: region.region.id,
                        text: region.region.name,
                        name: region.region.name.replace(/\s+/g, ''),
                        items: []
                    };

                    newRegion.value = (($scope.lastSettingsChosen.defaultRegionId && $scope.lastSettingsChosen.defaultRegionId == newRegion.id)
                        || (!$scope.lastSettingsChosen.defaultRegionId && i == 0)) ? newRegion : null;

                    region.countries.forEach(function (country) {
                        var newCountry = {
                            id: country.id,
                            text: country.name,
                            name: country.name.replace(/\s+/g, ''),
                            value:(newRegion.value != null)? true: false
                        };

                        newRegion.items.push(newCountry);
                    });


                    result.push(newRegion);
                });

                return result;
            };

            var filterGeographyForProduct = function () {

                var selectedProductId = getSelectedProducts()[0];
                var usableGeographiesForProduct = _.find(usableProductsAndGeographyIds, function (p) { return p.productId == selectedProductId; });

                var regionsToRemove = [];
                countryFilter.properties.items.forEach(function (region) {
                    var countriesToRemove = [];
                    region.items.forEach(function (country) {
                        if (!_.any(usableGeographiesForProduct.geographyIds, function (geo) {
                            return country.id == geo;
                        })) {
                            countriesToRemove.push(country);
                        }
                    });
                    countriesToRemove.forEach(function (toRemove) {
                        region.items.splice(region.items.indexOf(toRemove), 1);
                    });
                    if (region.items.length == 0) {
                        regionsToRemove.push(region);
                    }
                });

                regionsToRemove.forEach(function (toRemove) {
                    countryFilter.properties.items.splice(countryFilter.properties.items.indexOf(toRemove), 1);
                });

                if (getSelectedCountries().length == 0) {
                    countryFilter.properties.items[0].value = true;
                    countryFilter.properties.items[0].items.forEach(function (item) {
                        item.value = true;
                    });
                }
            };

            var prepareProductsForFilter = function (data) {
                var products = [];

                if (validate) {

                    data.forEach(function (p, i) {
                        if (_.any(usableProductsAndGeographyIds, function (us) {
                            return us.productId == p.id;
                        })) {
                            products.push({
                                id: p.id,
                                text: p.name,
                                textShort: p.shortname,
                                selected: ($scope.lastSettingsChosen.defaultProductId && $scope.lastSettingsChosen.defaultProductId == p.id)
                                    || (!$scope.lastSettingsChosen.defaultProductId && i == 0) ? true : false
                            });
                        }
                    });

                    if (!_.any(products, function (p) { return p.selected; })) {
                        products[0].selected = true;
                    }

                } else {
                    data.forEach(function (p, i) {
                        products.push({
                            id: p.id,
                            text: p.name,
                            textShort: p.shortname,
                            selected: (($scope.lastSettingsChosen.defaultProductId && $scope.lastSettingsChosen.defaultProductId == p.id)
                                || (!$scope.lastSettingsChosen.defaultProductId && i == 0)) ? true : false
                        });
                    });
                }

                return products;
            };

            var getSelectedCountries = function () {
                var countriesId = [];
                for (var i = 0; i < countryFilter.properties.items.length; i++) { // for each region

                    _.each(countryFilter.properties.items[i].items, function (country) {
                        if (country.value)
                            countriesId.push(country.id);
                    });
                }

                return countriesId;
            };

            var getSelectedProducts = function () {
                var products = [];
                _.each(productFilter.properties.items, function (product) {
                    if (product.selected == true)
                        products.push(product.id);
                });
                return products;
            };

            var getSelectedVersionId = function () {
                var version = _.find(versionFilter.properties.items, function (v) {
                    return v.selected == true;
                });
                return (version != undefined) ? version.versionId : 0;
            };

            var resetPaging = function () {
                $scope.searchRequest.pageNumber = 0;
                $scope.listToSales = [];
            };

            var onSearch = function () {
                $scope.searchRequest.productsId = getSelectedProducts();
                $scope.searchRequest.countriesId = getSelectedCountries();
                $scope.searchRequest.versionId = getSelectedVersionId();
                resetPaging();
                $scope.getListToSales();
            };

            var reloadVersionsAndGetListToSales = function () {
                $scope.searchRequest.productsId = getSelectedProducts();
                $scope.searchRequest.countriesId = getSelectedCountries();
                var userMappingPromise = userService.getUserMapping();
                var versionPromise = versionService.getListToSalesVersions($scope.searchRequest);
                $q.all([versionPromise, userMappingPromise]).then(function (data) {
                    versionFilter.properties.items = versionService.getVersionsForFilter(data[0], data[1]);
                    $scope.searchRequest.versionId = getSelectedVersionId();
                    resetPaging();
                    $scope.getListToSales();
                });
            };

            $scope.getListToSales = function () {
                listToSalesService.getPaged($scope.searchRequest).then(function (result) {

                    if (result) {
                        $scope.listToSales = $scope.listToSales.concat(result.listToSales);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more list to sales - "+$scope.listToSales.length + " out of " + result.totalListToSales;
                        initTable($scope.listToSales);
                    }
                    canSaveVersion();
                    $scope.searchRequest.pageNumber++;

                    calculateDeleteSmartResolution();
                    calculateSmartResolution();

                });
            };

            var init = function () {
                var countryPromise = countryService.getRegionsAndCountries();
                var productPromise = productService.getAllProducts();
                var userPromise = userService.getLastSettingsChosen();

                var promiseArray = [countryPromise, productPromise, userPromise];

                if (validate) {
                    var next = loadService.getNext($routeParams.loadId, helperService.LoadItemNames.NetData);
                    promiseArray.push(next);
                }

                $q.all(promiseArray).then(function (data) {

                    if (validate) {
                        usableProductsAndGeographyIds = data[3];
                    }

                    $scope.lastSettingsChosen = data[2];

                    productFilter.properties.items = prepareProductsForFilter(data[1]);
                    countryFilter.properties.items = prepareRegionAndCountriesForFilter(data[0]);

                    if (validate) {
                        filterGeographyForProduct();
                    }

                    $scope.searchRequest.countriesId = getSelectedCountries();
                    $scope.searchRequest.productsId = getSelectedProducts();
                    var userMappingPromise = userService.getUserMapping();
                    var versionPromise = versionService.getListToSalesVersions($scope.searchRequest);

                    $q.all([versionPromise, userMappingPromise]).then(function (datas) {
                        // define filter
                        $scope.filterOptions = {
                            header: {
                                items: [
                                     {
                                         type: helperService.FieldTypes.Label,
                                         properties:
                                         {
                                             text: "List To Sales For"
                                         }
                                     },
                                    productFilter,
                                    versionFilter
                                ],
                                primaryDisplayOptions: [],
                            },
                            filters: [
                                 countryFilter
                            ],
                            showAdvancedFilters: false,
                            onChanged: function (sourceChanged) {
                                if (sourceChanged == countryFilter || sourceChanged == productFilter) {
                                    if (sourceChanged == countryFilter) {
                                        userService.lastSettingsChosen.defaultRegionId = getSelectedRegionId();
                                        reloadVersionsAndGetListToSales();
                                    } else {
                                        var products = getSelectedProducts();
                                        userService.lastSettingsChosen.defaultProductId = products[0];
                                        if (validate) {
                                            countryPromise.then(function(result) {
                                                countryFilter.properties.items = prepareRegionAndCountriesForFilter(result);
                                                filterGeographyForProduct();
                                                $scope.searchRequest.countriesId = getSelectedCountries();
                                                reloadVersionsAndGetListToSales();
                                            });
                                        }
                                        else {
                                            reloadVersionsAndGetListToSales();
                                        }
                                    }
                                    
                                } else {
                                    onSearch();
                                }
                            }
                        };
                        versionFilter.properties.items = versionService.getVersionsForFilter(datas[0], datas[1]);

                        if (!validate)
                            $scope.filterOptions.header.primaryDisplayOptions.push({
                                type: helperService.FieldTypes.Action,
                                properties:
                                {
                                    text: 'Export to Excel',
                                    class: 'color-white icon-before icon-before-excel',
                                    callback: function() {

                                        var modalInstance = $modal.open({
                                            templateUrl: 'Content/js/pricecare/modal/excelFilterModal.html',
                                            controller: 'ExcelFilterModalController',
                                            backdrop: 'static',
                                            resolve: {
                                                infos: function() {
                                                    return {
                                                        noFilterOnEvent: true,
                                                        noFilterOnCountry: true,
                                                        noFilterOnProduct: true
                                                    }
                                                }
                                            }
                                        });

                                        modalInstance.result.then(function(response) {
                                            $scope.searchRequest.databaseLike = response.databaseLike;
                                            excelService.postFilterExcel($scope.searchRequest).then(function(result) {
                                                window.location.href = 'api/listtosales/excel?token=' + result.token;
                                            });
                                        }, function() {

                                        });
                                    }
                                }
                            });
                        onSearch();
                    });                    
                });
            };

            init();

            var getSelectedRegionId = function() {
                var region = _.find(countryFilter.properties.items, function(item) {
                    return item.value != null;
                });
                return region.id;
            };

            var canSaveVersion = function () {
                if (validate) {
                    var allRowsAreReady = true;
                    for (var i = 1; i < $scope.table.rows.length && allRowsAreReady; i++) {
                        if (!$scope.table.rows[i].parentRow) continue;
                        var cptRowReady = 0;
                        for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                            cptRowReady += $scope.table.rows[i].cells[j].ready ? 1 : 0;
                        }
                        if (cptRowReady == 0) allRowsAreReady = false;
                    }
                    $scope.canSave = allRowsAreReady;
                } else {
                    $scope.canSave = false;
                    for (var i = 0; i < $scope.table.rows.length && !$scope.canSave; i++) {
                        for (var j = 0; j < $scope.table.rows[i].cells.length && !$scope.canSave; j++) {
                            if ($scope.table.rows[i].cells[j].edited) {
                                $scope.canSave = true;
                            }
                        }
                    }
                }
            };

            $scope.saveVersion = function () {
                var data = {
                    listToSales: getEditedRows(),
                    validate: validate,
                    loadId: $routeParams.loadId
                };
                if (data.listToSales.length > 0) {
                    listToSalesService.saveVersion(data).then(function() {
                        if (validate) {
                            validateLoadItemDetail();
                        } else {
                            reloadVersionsAndGetListToSales();
                        }
                    }, function() { // if error occurs
                        canSaveVersion();
                    });
                } else {
                    if (validate) {
                        validateLoadItemDetail();
                    }
                }
            };

            var getEditedRows = function () {
                var editedRows = [];

                if (!$scope.canSave) return editedRows;

                for (var i = 1; i < $scope.table.rows.length; i++) {
                    for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            editedRows.push($scope.table.rows[i].listToSales);
                            break;
                        }
                    }
                }
                return editedRows;
            };

            var validateLoadItemDetail = function () {
                loadService.validateLoadItemDetail($routeParams.loadId, helperService.LoadItemNames.NetData, getSelectedProducts()[0], getSelectedCountries()).then(function (result) {
                    if(result.url)
                        $location.path(result.url);
                    else {
                        init();
                    }
                });
            }


        }]);
});