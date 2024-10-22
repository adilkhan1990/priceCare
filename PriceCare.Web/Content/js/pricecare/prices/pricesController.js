﻿define(['pricecare/prices/module'], function(module) {
    'use strict';
    module.controller('PricesController', [
        '$scope', '$q', 'helperService', 'priceService', 'productService', 'countryService', 'priceTypeService', '$http', 'dimensionService', 'userService', '$modal', 'validate', 'versionService',
        'loadService', '$routeParams', '$location', 'tableService', 'excelService',
        function ($scope, $q, helperService, priceService, productService, countryService, priceTypeService, $http, dimensionService, userService, $modal, validate, versionService,
            loadService, $routeParams, $location, tableService, excelService) {

            $scope.loadId = $routeParams.loadId;
            $scope.cellsToValidateSmartResolution1 = 0;
            $scope.cellsToValidateSmartResolution2 = 0;
            
            $scope.validate = validate;
            $scope.showBadFilter = false;
            var countryPromise;
            var currencyFilter = helperService.getCurrencyFilter();
            var priceTypePromise = priceTypeService.getAllPriceTypes(0);
            var eventTypePromise = dimensionService.getEventTypes();
            var eventTypes;

            var usableProductsAndGeographyIds;

            userService.getLastSettingsChosen().then(function(result) {
                $scope.lastSettingsChosen = result;
                getCountriesAndProducts();
            });

            $scope.smartResolutionOptions = {
                predescription: 'cells vary by',
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
            
            $scope.smartDeleteResolutionOptions = {
                predescription: 'cells correspond to previous version with no new values',
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
                            var upItemSelected = _.find(countryFilter.properties.items, function (upItem) {
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

            var productFilter = {
                type: helperService.FieldTypes.Select,
                properties:
                {
                    class: 't-custom-select-text',
                }
            };

            var versionFilter = {
                type: helperService.FieldTypes.Select,
                properties: {
                    class: 't-custom-select-text'
                }
            };

            var consolidationFilter = {
                type: helperService.FieldTypes.Select,
                properties:
                {
                    class: 't-custom-select-text',
                }
            };

            var displayOnlyChangingDataFilter = {
                type: helperService.FieldTypes.Checkbox,
                text: 'Display Only Changing Data',
                value: false, name: 'onlyChanging'
            };

            var highlightTresholdFilter = {
                type: helperService.FieldTypes.NumericTextbox,
                text: 'Highlight when more than',
                value: 15,
                name: 'highlightWhenMore',
                properties: {
                    trigger: helperService.FieldTriggers.OnEnter | helperService.FieldTriggers.OnBlur,
                    min: 0,
                    max: 200,
                    unit: '%'
                }
            };

            var compareToFilter = {
                type: helperService.FieldTypes.Select,
                text: 'Compare to',
                name: 'compareTo',
                properties:
                {
                    class: 't-custom-select-text',
                    items: [
                        {
                            text: 'None',
                            selected: true
                        }
                    ]
                }
            };

            $scope.filterOptions = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties:
                            {
                                text: "Prices for"
                            }
                        },
                        productFilter,
                        {
                            type: helperService.FieldTypes.Label,
                            properties:
                            {
                                text: "in"
                            }
                        },
                        consolidationFilter,
                        currencyFilter,
                        versionFilter
                    ],
                    primaryDisplayOptions: [],
                    displayOptions: [],
                },
                filters: [
                    countryFilter
                ],
                showAdvancedFilters: false,
                onChanged: function (sourceChanged) {
                    if (sourceChanged == currencyFilter || sourceChanged == consolidationFilter || sourceChanged == highlightTresholdFilter || sourceChanged == displayOnlyChangingDataFilter) {
                        setCellsDisplay();
                    } else if (sourceChanged == countryFilter || sourceChanged == productFilter) {
                        if (sourceChanged == countryFilter) {
                            userService.lastSettingsChosen.defaultRegionId = getSelectedRegionId();
                            getVersionsAndConsolidations(getSelectedCountriesIds(), getSelectedProductId());
                        } else {
                            if (validate) {
                                countryPromise.then(function (result) {
                                    countryFilter.properties.items = getRegionForFilter(result);

                                    filterGeographyForProduct();

                                    getVersionsAndConsolidations(getSelectedCountriesIds(), getSelectedProductId());
                                });
                            }
                            var productId = getSelectedProductId();
                            if (productId != 0)
                                userService.lastSettingsChosen.defaultProductId = getSelectedProductId();

                            if (!validate) {
                                getVersionsAndConsolidations(getSelectedCountriesIds(), getSelectedProductId());
                            }
                        }

                    } else {
                        getData();
                    }
                }
            };

            var calculateSmartResolution = function () {
                if ($scope.smartResolutionOptions.rulesData.length == 0) {
                    $scope.smartResolutionOptions.affectedCells = 0;
                } else {
                    $scope.cellsToValidateSmartResolution1 = 0;
                    tableService.foreachCells($scope.table, function (cell) {
                        if (cell.price && (cell.price.tag == helperService.LoadTag.Edited || cell.price.tag == helperService.LoadTag.Loaded) && !cell.edited) {
                            var canResolve = true;

                            for (var i = 0; i < $scope.smartResolutionOptions.rulesData.length; i++) {
                                var rule = $scope.smartResolutionOptions.rulesData[i];
                                var baseRule = _.find(helperService.RuleTypes, function (r) { return r.id == rule.type; });

                                if (!baseRule.check(cell.price.percentageVariationFromX, rule.value / 100)) {
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
            var applySmartResolution = function (applyChanges) {
                if ($scope.smartResolutionOptions.rulesData.length == 0) {
                    return;
                } else {
                    tableService.foreachCells($scope.table, function (cell, row) {
                        if (cell.price && (cell.price.tag == helperService.LoadTag.Edited || cell.price.tag == helperService.LoadTag.Loaded) && !cell.edited) {
                            var canResolve = true;

                            for (var i = 0; i < $scope.smartResolutionOptions.rulesData.length; i++) {
                                var rule = $scope.smartResolutionOptions.rulesData[i];
                                var baseRule = _.find(helperService.RuleTypes, function (r) { return r.id == rule.type; });

                                if (!baseRule.check(cell.price.percentageVariationFromX, rule.value / 100)) {
                                    canResolve = false;
                                    break;
                                }
                            }
                            if (canResolve) {
                                applyChanges(cell, row);
                            }
                        }
                    });

                    calculateSmartResolution();
                }
            };
            var calculateDeleteSmartResolution = function () {
                $scope.cellsToValidateSmartResolution2 = 0;
                tableService.foreachCells($scope.table, function (cell) {
                    if (cell.price && cell.price.tag == helperService.LoadTag.Deleted && !cell.edited) {
                        $scope.cellsToValidateSmartResolution2++;
                    }
                });

                $scope.smartDeleteResolutionOptions.affectedCells = $scope.cellsToValidateSmartResolution2;

            };
            var applyDeleteSmartResolution = function (applyChanges) {
                tableService.foreachCells($scope.table, function (cell, row) {
                    if (cell.price && cell.price.tag == helperService.LoadTag.Deleted && !cell.edited) {                        
                        applyChanges(cell, row);
                    }
                });
                calculateDeleteSmartResolution();
            };
                       
            $scope.firstVersionSelected = function ()
            {
                if (versionFilter.properties.items && versionFilter.properties.items.length > 0) {
                    return versionFilter.properties.items[0].selected == true;
                }

                return true;
            }
                        
            var getSelectedCompareVersionOptionItem = function() {
                var item = _.find(compareToFilter.properties.items, function(s) {
                    return s.selected;
                });

                return item;
            };

            var getSelectedRegionId = function () {
                var region = _.find(countryFilter.properties.items, function (item) {
                    return item.value != null;
                });
                return region.id;
            };

            var getSelectedCurrency = function () {
                return getSelectedFilterItem(currencyFilter);
            };
            
            var getSelectedVersion = function () {
                return getSelectedFilterItem(versionFilter);
            };

            var getSelectedConsolidation = function () {
                return getSelectedFilterItem(consolidationFilter);
            };

            var getSelectedFilterItem = function(filter) {
                var selectedItem = _.find(filter.properties.items, function (item) {
                    return item.selected == true;
                });
                return selectedItem;
            }

            var getCountriesAndProducts = function(callback) {
                countryPromise = countryService.getRegionsAndCountries();
                var productPromise = productService.getAllProducts();

                var promiseArray = [countryPromise, productPromise];

                if (validate) {
                    var next = loadService.getNext($routeParams.loadId, helperService.LoadItemNames.Price);
                    promiseArray.push(next);
                }

                $q.all(promiseArray).then(function (data) {

                    if (validate) {
                        usableProductsAndGeographyIds = data[2];
                    }

                    productFilter.properties.items = getProductsForFilter(data[1], false);
                    countryFilter.properties.items = getRegionForFilter(data[0]);
                    if (validate) {
                        filterGeographyForProduct();
                    }
                    getVersionsAndConsolidations(getSelectedCountriesIds(), getSelectedProductId());

                    if (callback != null)
                        callback();
                });
            };

            var getVersionsAndConsolidations = function(countryIds, productId) {
                var versionPromise = $http.post('api/price/versions', { GeographyIds: countryIds, productId: productId });
                var consolidationPromise = $http.get('api/consolidation/' + productId);
                var userMappingPromise = userService.getUserMapping();

                $q.all([versionPromise, consolidationPromise, userMappingPromise]).then(function (data) {
                    versionFilter.properties.items = versionService.getVersionsForFilter(data[0].data, data[2]);
                    compareToFilter.properties.items = compareToFilter.properties.items.concat(versionService.getCompareVersionsForFilter(data[0].data, data[2]));
                    consolidationFilter.properties.items = getConsolidationsForFilter(data[1].data);

                    getData();
                });
            };

            var getConsolidationsForFilter = function (data) {
                data.forEach(function (consolidation) {
                    if (consolidation.isDefault) {
                        consolidation.selected = true;
                    }
                    consolidation.text = consolidation.factorScreen + " " + consolidation.name;
                });

                return data;
            };

            var getRegionForFilter = function (data) {
                var result = [];

                data.forEach(function (region, i) {
                    var newRegion = {
                        id: region.region.id,
                        text: region.region.name,
                        name: region.region.name.replace(/\s+/g, ''),
                        items: [],
                    };

                    newRegion.value = (($scope.lastSettingsChosen.defaultRegionId && $scope.lastSettingsChosen.defaultRegionId == newRegion.id)
                        || (!$scope.lastSettingsChosen.defaultRegionId && i == 0)) ? newRegion : null;

                    region.countries.forEach(function (country) {
                        var newCountry = {
                            id: country.id,
                            text: country.name,
                            name: country.name.replace(/\s+/g, ''),
                            value: (newRegion.value != null) ? true : false
                        };

                        newRegion.items.push(newCountry);
                    });

                    result.push(newRegion);
                });

                return result;
            };

            var filterGeographyForProduct = function() {
                var selectedProductId = getSelectedProductId();
                var usableGeographiesForProduct = _.find(usableProductsAndGeographyIds, function (p) { return p.productId == selectedProductId; });

                var regionsToRemove = [];
                countryFilter.properties.items.forEach(function (region) {
                    var countriesToRemove = [];
                    region.items.forEach(function(country) {
                        if (!_.any(usableGeographiesForProduct.geographyIds, function(geo) {
                            return country.id == geo;
                        })) {
                            countriesToRemove.push(country);
                        }
                    });
                    countriesToRemove.forEach(function(toRemove) {
                        region.items.splice(region.items.indexOf(toRemove), 1);
                    });
                    if (region.items.length == 0) {
                        regionsToRemove.push(region);
                    }
                });

                regionsToRemove.forEach(function (toRemove) {
                    countryFilter.properties.items.splice(countryFilter.properties.items.indexOf(toRemove), 1);
                });

                if (getSelectedCountriesIds().length == 0) {
                    countryFilter.properties.items[0].value = true;
                    countryFilter.properties.items[0].items.forEach(function(item) {
                        item.value = true;
                    });
                }
            };

            var getData = function () {                
                $scope.showBadFilter = false;
                var selectedVersion = getSelectedVersion();
                var compareToVersionId = getSelectedCompareVersionOptionItem().versionId;
                var selectedVersionId = null;
                if (selectedVersion) {
                    selectedVersionId = selectedVersion.versionId;
                }
                var pricePromise = priceService.getPrices(getSelectedCountriesIds(), getSelectedProductId(), selectedVersionId, validate, null, compareToVersionId);

                $q.all([pricePromise, countryPromise, priceTypePromise, eventTypePromise]).then(function (data) {
                    if (data[0].length == 0) {

                        $scope.table = null;
                        $scope.showBadFilter = true;
                        calculateSmartResolution();
                        calculateDeleteSmartResolution();
                        if (validate) {
                            $scope.canSave = true;
                        }
                        return;
                    }

                    eventTypes = data[3];

                    var consolidation = getSelectedConsolidation();
                    $scope.priceTypes = data[2];
                    var regionsAndCountries = data[1];
                    var allDates = _.map(data[0], function (item) { return item.dataTime; });
                    var uniqDates = _.uniq(allDates);
                    var dateLabels = [];
                    var currentCurrency = getSelectedCurrency();

                    var now = helperService.formatDateMonthYear(new Date());
                    var indiceNow = -1;
                    var dateCpt = 0;
                    uniqDates.forEach(function (d) {
                        var labelDate = helperService.formatDateMonthYear(d);
                        dateLabels.push({
                            label: labelDate,
                            value: d
                        });
                        if (labelDate == now) {
                            indiceNow = dateCpt;
                        }
                        dateCpt++;
                    });

                    if (indiceNow == -1)
                        indiceNow = dateLabels.length - 12; // if the actual date is not found, we take the last 12 months


                    var countryGroups = _.groupBy(data[0], 'geographyId');

                    var rows = [];

                    // Create header row
                    var headerRow = {
                        title: true,
                        cells: [{}] // Empty cell for the first column of header
                    };
                    dateLabels.forEach(function (item) {
                        headerRow.cells.push({
                            title: true,
                            value: item.label,
                            baseValue: item.value
                        });
                    });
                    rows.push(headerRow);

                    for (var regionId in regionsAndCountries) {
                        var region = regionsAndCountries[regionId];
                        var regionRows = [];

                        for (var countryId in region.countries) {

                            var country = region.countries[countryId];
                            var countryData = countryGroups[country.id];
                            if (countryData) {
                                var priceTypeGroups = _.groupBy(countryData, 'priceTypeId');
                                for (var priceTypeId in priceTypeGroups) {
                                    var priceTypeData = priceTypeGroups[priceTypeId];

                                    var priceTypeRow = {
                                        cells: [
                                            { value: country.name + " - " + _.find($scope.priceTypes, function (pt) { return pt.id == priceTypeId; }).shortName }
                                        ]
                                    };

                                    for (var i = 0; i < dateLabels.length; i++) {
                                        var price = priceTypeData[i];
                                        var newCell = {
                                            originalPrice: helperService.clone(price),
                                            price: price,
                                        };

                                        newCell.rightMenu = getRightMenu(newCell, priceTypeRow);
                                        newCell.blockleftMenu = true;
                                        setCellDisplay(newCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, priceTypeRow);
                                        if (!$scope.firstVersionSelected())
                                            newCell.blockRightMenu = true;
                                        priceTypeRow.cells.push(newCell);
                                    }
                                    regionRows.push(priceTypeRow);
                                }
                            }
                        }

                        // If there were any country rows added
                        if (regionRows.length >= 1) {
                            rows = rows.concat(regionRows);
                        }
                    }

                    var table = {
                        columnFilteringOptions: {
                            offset: indiceNow,
                            count: 12,
                            fixed: 1,
                        },
                        rows: rows
                    };
                    $scope.table = table;

                    if (!validate) {
                        var exportToExcelFilter = {
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
                                                    noFilterOnEvent: true
                                                };
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function(response) {
                                        var searchRequest = {
                                            geographyId: getSelectedCountriesIds(),
                                            productId: [getSelectedProductId()],
                                            versionId: getSelectedVersion().versionId,
                                            allCountries: response.allCountries,
                                            allProducts: response.allProducts,
                                            databaseLike: response.databaseLike,
                                            dataTypeId: 41,
                                            scenarioTypeId: 1
                                        };
                                        excelService.postFilterExcel(searchRequest).then(function(result) {
                                            window.location.href = 'api/price/excel?token=' + result.token;
                                        });
                                    }, function() {

                                    });
                                }
                            }
                        };
                        $scope.filterOptions.header.primaryDisplayOptions = [exportToExcelFilter];
                        $scope.filterOptions.header.displayOptions = [displayOnlyChangingDataFilter, highlightTresholdFilter, compareToFilter];                        
                    }

                    calculateSmartResolution();
                    calculateDeleteSmartResolution();
                });              
            };

            var setCellDisplay = function (cell, currency, consolidation, highlightThreshold, displayOnlyChangingData, row) {
                if (cell.title) {
                    return;
                }

                if (validate) {
                    if ((!cell.price.tag == "" || !cell.price.tag == undefined) && !cell.edited) {
                        cell.styleLoad = {
                            border: helperService.borderAll,
                            background: cell.price.tag == helperService.LoadTag.Deleted
                        };
                    }
                }

                if (cell.price.value) {
                    cell.empty = false;
                    var isChanging = cell.price.percentageVariation && Math.abs(cell.price.percentageVariation) > highlightThreshold;

                    // Background changing price
                    if (isChanging) {
                        cell.blockLeftMenu = false;
                        cell.leftMenu = getLeftMenu(cell);
                        cell.background = "#FDF8BC";
                    } else {
                        cell.background = '';
                        cell.blockLeftMenu = true;
                        cell.leftMenu = null;
                    }
                    // Background event type
                    if (cell.price.eventTypeId) {
                        var eventType = _.find(eventTypes, function (item) {
                            return cell.price.eventTypeId == item.id;
                        });
                        if (!cell.background && eventType) {
                            cell.background = eventType.colorCode;
                        }
                    }

                    if (displayOnlyChangingData) {
                        cell.blockRightMenu = true;
                        if (isChanging) {
                            cell.value = cell.price.percentageVariation;
                            cell.format = helperService.formatPercentage;
                        } else {
                            cell.blockLeftMenu = true;
                            cell.format = null;
                            cell.value = null;

                            if (_.all(row.cells.slice(1), function(c) { return c.value == null; })) {
                                row.hide = true;
                            } else {
                                row.hide = false;
                            }
                        }
                    } else {
                        row.hide = false;
                        cell.blockLeftMenu = false;
                        cell.blockRightMenu = false;
                        cell.value = priceService.getPriceValue(cell.price, currency, consolidation);
                        cell.format = helperService.formatNumber;
                    }

                    cell.rightMenu.fields[0].cellValue = cell.value;
                } else {
                    if (!displayOnlyChangingData) {
                        cell.empty = true;
                        cell.value = null;
                    } else {
                        cell.blockLeftMenu = true;
                        cell.blockRightMenu = true;
                        cell.empty = false;
                    }
                }
            };

            var setCellsDisplay = function () {
                var consolidation = getSelectedConsolidation();
                var currentCurrency = getSelectedCurrency();
                tableService.foreachCells($scope.table, function (currentCell, row) {
                    if (!currentCell.deleted)
                        setCellDisplay(currentCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);
                });
            };

            var getLeftMenu = function (currentCell) {
                return {
                    content: currentCell.price.description
                };
            };
            var getLeftMenuWithDefaultMessage = function (currentCell) {
                return {
                    content: '<strong>The value has changed, click on save version button to recalculate the left view.</strong>'
                };
            };
            var getRightMenu = function (currentCell, row) {
                var rightMenu;
                if (currentCell.price == undefined)
                    return;
                if (!validate || currentCell.price.tag == '' || currentCell.price.tag == undefined || currentCell.edited) {
                    rightMenu = {
                        fields: [
                            {
                                type: helperService.FieldTypes.NumericTextbox,
                                show: true,
                                properties: {
                                    required: true,
                                    focus: true,
                                    select: true,
                                    allowDecimal: true,
                                    allowNegative: true,
                                    areEquals: function (x, y) {
                                        return Math.abs((x - y) / (x + y) * 2) < helperService.EqualityThreshold;
                                    },
                                    parse: function (x) {
                                        return parseFloat(x);
                                    }
                                }
                            },
                            {
                                type: helperService.FieldTypes.Action,
                                properties:
                                {
                                    text: 'Delete',
                                    class: 'button button-border button-red button-icon icon-delete text-align-center',
                                    callback: function () {
                                        var modalInstance = $modal.open({
                                            templateUrl: 'Content/js/pricecare/modal/confirmationModal.html',
                                            controller: 'ConfirmationModalController',
                                            backdrop: 'static',
                                            resolve: {
                                                infos: function () {
                                                    return {
                                                        title: 'Remove Price',
                                                        content: 'Are you sure you want to delete this price ?'
                                                    }
                                                }
                                            }
                                        });

                                        modalInstance.result.then(function () {
                                            currentCell.edited = true;
                                            currentCell.deleted = true;
                                            currentCell.value = null;
                                            currentCell.background = null;
                                            currentCell.border = null;
                                            currentCell.empty = true;
                                            currentCell.blockLeftMenu = true;
                                            currentCell.price = helperService.clone(currentCell.originalPrice);
                                            currentCell.price.active = false;
                                    
                                            currentCell.rightMenu = getRightMenu(currentCell, row);
                                            // re-init the previous and next cells
                                            var row = -1;
                                            $scope.table.rows.forEach(function (r) {
                                                if (_.any(r.cells, function (c) { return c == currentCell; }))
                                                    row = r;
                                            });

                                            var indexCurrentRow = row.cells.indexOf(currentCell);

                                            // Get next cell to recalculate percentageVariation on nextCell
                                            if (indexCurrentRow + 1 < row.cells.length) {
                                                var nextCell = row.cells[indexCurrentRow + 1];
                                                nextCell.background = null;
                                                nextCell.border = null;
                                            }

                                            canSaveVersion();
                                        }, function () {

                                        });
                                    }
                                }
                            }
                        ],
                        properties: {
                            apply : function(fields, cell, row, table) {
                                var editedValue = fields[0].value;
                                cell.deleted = false;
                                cell.rightMenu = getRightMenu(cell, row);
                                prepareCellAndNeighbours(cell, row, editedValue);
                                $scope.canSave = true;
                                cell.blockLeftMenu = false;
                                cell.leftMenu = getLeftMenuWithDefaultMessage(cell);
                            },
                            reverseEdit: function (fields, cell, row, table) {
                                var currency = getSelectedCurrency();
                                var consolidation = getSelectedConsolidation();
                                cell.price = helperService.clone(cell.originalPrice);
                                cell.price.value = priceService.getPriceValue(cell.price, currency, consolidation);
                                cell.edited = false;
                                cell.deleted = false;
                                cell.rightMenu = getRightMenu(cell, row);
                                prepareCellAndNeighbours(cell, row, cell.price.value);
                                canSaveVersion();
                            }
                        }
                    };

                    rightMenu.properties.showReverseEdit = (currentCell.edited || currentCell.deleted) ? true : false;
                    rightMenu.properties.showApply = (!currentCell.deleted) ? true : false;
                    rightMenu.fields[0].show = (!currentCell.deleted) ? true : false;
                    rightMenu.fields[1].show = (!currentCell.deleted && currentCell.price && currentCell.price.value && currentCell.originalPrice.value) ? true : false;
                } else {
                    var consolidation = getSelectedConsolidation();
                    var currentCurrency = getSelectedCurrency();
                    rightMenu = {
                        hideTextHelp: true,
                        fields: [
                            { type: helperService.FieldTypes.Checkbox, text: "Apply to all identical cases on this row", value: false, name: 'wholerow' },
                            {
                                type: helperService.FieldTypes.Action,
                                properties: {
                                    text: "Accept",
                                    class: 'button button-border button-green button-icon icon-save text-align-center',
                                    callback: function () {
                                        applyLoad(currentCell, row);
                                    }
                                }
                            },
                            {
                                type: helperService.FieldTypes.Action,
                                properties: {
                                    text: "Discard",
                                    class: 'button button-border button-red button-icon icon-delete text-align-center',
                                    callback: function () {
                                        rejectLoad(currentCell, row);
                                    }
                                }
                            }
                        ],
                        properties: {
                            showReverseEdit: false,
                            showApply: false,
                            apply: function (fields, cell, row, table) {
                                //cell.value = parseFloat(fields[0].value);
                            }
                        }
                    };

                    if (currentCell.price.tag == helperService.LoadTag.Edited && currentCell.price.oldValue && currentCell.price.oldValue.value) {
                        rightMenu.fields.unshift({
                            type: helperService.FieldTypes.Label,
                            properties: {
                                text: "OldValue: " + helperService.formatNumber(priceService.getPriceValue({
                                    value: currentCell.price.oldValue.value,
                                    eurSpot: currentCell.price.eurSpot,
                                    eurBudget: currentCell.price.eurBudget,
                                    usdSpot: currentCell.price.usdSpot,
                                    usdBudget: currentCell.price.usdBudget
                                }, currentCurrency.id, consolidation))
                            }
                        });
                    }
                }

                return rightMenu;
            };
            var getApplyWholeRowValue = function(currentCell) {
                if (currentCell.rightMenu.fields[0].type == helperService.FieldTypes.Checkbox) {
                    return currentCell.rightMenu.fields[0].value;
                }
                return currentCell.rightMenu.fields[1].value;
            };
            var applyLoad = function (currentCell, row) {
                currentCell.onClose();
                var applyWhole = getApplyWholeRowValue(currentCell);

                var currentTag = currentCell.price.tag;
                if (applyWhole === true) {
                    row.cells.forEach(function(cell) {
                        if (cell.price && !cell.edited && cell.price.tag && cell.price.tag == currentTag) {
                            applyLoadOnCell(cell, row);
                        }
                    });
                } else {
                    applyLoadOnCell(currentCell, row);
                }
                
            };

            var applyLoadOnCell = function (currentCell, row) {
                var consolidation = getSelectedConsolidation();
                var currentCurrency = getSelectedCurrency();
                switch (currentCell.price.tag) {
                    case helperService.LoadTag.Loaded:
                        currentCell.styleLoad = null;
                        currentCell.edited = true;
                        currentCell.rightMenu = getRightMenu(currentCell, row);
                        setCellDisplay(currentCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);

                        break;
                    case helperService.LoadTag.Deleted:
                        currentCell.styleLoad = null;
                        currentCell.edited = true;

                        currentCell.price.value = null;

                        currentCell.rightMenu = getRightMenu(currentCell, row);
                        setCellDisplay(currentCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);

                        break;
                    case helperService.LoadTag.Edited:
                        currentCell.styleLoad = null;
                        currentCell.edited = true;
                        currentCell.rightMenu = getRightMenu(currentCell, row);
                        setCellDisplay(currentCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);

                        currentCell.styleLoad = null;
                        break;
                }
                currentCell.edited = true;
                currentCell.ready = true;
                canSaveVersion();
            }

            var rejectLoad = function (currentCell, row) {
                currentCell.onClose();

                var applyWhole = getApplyWholeRowValue(currentCell);
                var currentTag = currentCell.price.tag;
                if (applyWhole === true) {
                    row.cells.forEach(function (cell) {
                        if (cell.price && !cell.edited && cell.price.tag && cell.price.tag == currentTag) {
                            rejectLoadOnCell(cell, row);
                        }
                    });
                } else {
                    rejectLoadOnCell(currentCell, row);
                }
            };

            var rejectLoadOnCell = function (currentCell, row) {
                var consolidation = getSelectedConsolidation();
                var currentCurrency = getSelectedCurrency();
                
                switch (currentCell.price.tag) {
                    case helperService.LoadTag.Loaded:
                        currentCell.styleLoad = null;
                        //currentCell.edited = true;
                        currentCell.price.tag = '';

                        currentCell.price.value = null;

                        currentCell.rightMenu = getRightMenu(currentCell, row);
                        setCellDisplay(currentCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);

                        break;
                    case helperService.LoadTag.Deleted:
                        currentCell.styleLoad = null;
                        //currentCell.edited = true;
                        currentCell.price.tag = '';
                        //currentCell.price.value = currentCell.price.oldValue.value;

                        currentCell.rightMenu = getRightMenu(currentCell, row);
                        setCellDisplay(currentCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);


                        break;
                    case helperService.LoadTag.Edited:
                        currentCell.styleLoad = null;
                        //currentCell.edited = true;
                        currentCell.price.tag = '';

                        currentCell.price.value = currentCell.price.oldValue.value;

                        currentCell.rightMenu = getRightMenu(currentCell, row);
                        setCellDisplay(currentCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);

                        currentCell.styleLoad = null;
                        break;
                }
                currentCell.ready = true;
                canSaveVersion();
            };

            var prepareCellAndNeighbours = function(cell, row, value) {
                var currency = getSelectedCurrency();
                var consolidation = getSelectedConsolidation();

                var actualValue = (value != undefined) ? priceService.reversePriceValue(value, cell.price, currency.id, consolidation) : undefined;

                cell.price.value = actualValue;

                var indexCurrentRow = row.cells.indexOf(cell);

                // Get previous cell to recalculate percentageVariation on currentCell
                var previousCell = row.cells[indexCurrentRow - 1];
                if (previousCell.price && !previousCell.deleted) {
                    cell.price.percentageVariation = calculateVariation(cell.price.value, previousCell.price.value);
                }

                // Get next cell to recalculate percentageVariation on nextCell
                if (indexCurrentRow + 1 < row.cells.length) {
                    var nextCell = row.cells[indexCurrentRow + 1];
                    
                    if (!nextCell.deleted) {
                        nextCell.price.percentageVariation = calculateVariation(nextCell.price.value, cell.price.value);
                        setCellDisplay(nextCell, currency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);
                    }
                }

                setCellDisplay(cell, currency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, row);
            };
            var calculateVariation = function(currentValue, previousValue) {
                return previousValue != undefined && previousValue != 0 ? (currentValue - previousValue) / previousValue : 0;
            };
            var getSelectedCountriesIds = function() {
                var ids = [];

                countryFilter.properties.items.forEach(function(region) {
                    region.items.forEach(function(country) {
                        if (country.value) {
                            ids.push(country.id);
                        };
                    });
                });

                return ids;
            };
            var getSelectedProductId = function () {
                var result = _.find(productFilter.properties.items, function (item) { return item.selected; });
                if (result)
                    return result.id;
                else
                    return productFilter.properties.items[0].id;

                //return _.find(productFilter.properties.items, function(item) { return item.selected; }).id;
            }
            var getProductsForFilter = function (productsResponse, includeAll) {
                var products = [];

                if (includeAll) {
                    productsResponse.push({
                        id: 0,
                        name: "All products",
                        shortName: "All products"
                    });
                }

                if (validate) {
                    productsResponse.forEach(function (p, i) {
                        if (_.any(usableProductsAndGeographyIds, function(us) {
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

                    if (!_.any(products, function(p) { return p.selected; })) {
                        products[0].selected = 1;
                    }

                } else {
                    productsResponse.forEach(function(p, i) {
                        products.push({
                            id: p.id,
                            text: p.name,
                            textShort: p.shortname,
                            selected: ($scope.lastSettingsChosen.defaultProductId && $scope.lastSettingsChosen.defaultProductId == p.id)
                                || (!$scope.lastSettingsChosen.defaultProductId && i == 0) ? true : false
                        });
                    });
                }
                return products;
            };

            $scope.saveVersion = function () {
                if ($scope.table && $scope.table.rows && $scope.canSave) {
                    var editedCells = [];

                    tableService.foreachCells($scope.table, function (cell) {
                        if (cell.edited)
                            editedCells.push(cell.price);
                    });
                    if (editedCells.length > 0) {
                        priceService.saveVersion(editedCells).then(function(data) {
                            getVersionsAndConsolidations(getSelectedCountriesIds(), getSelectedProductId());
                            $scope.canSave = false;

                            if (validate) {
                                validateLoadItemDetail();
                            }
                        });
                    } else {
                        if (validate) {
                            validateLoadItemDetail();
                        }
                    }
                } else {
                    if (validate && $scope.table == null && $scope.canSave) { //todo: $scope.canSave is undefined and $scope.table is not null => unable to validate rows... see why
                        validateLoadItemDetail();
                    }
                }
            };

            var validateLoadItemDetail = function () {
                loadService.validateLoadItemDetail($routeParams.loadId, helperService.LoadItemNames.Price, getSelectedProductId(), getSelectedCountriesIds()).then(function (result) {                     
                    if (result.url) {
                        $location.path(result.url);
                    } else {
                        userService.getLastSettingsChosen().then(function (res) {
                            $scope.lastSettingsChosen = res;
                            getCountriesAndProducts(getData);                            
                        });                        
                    }                    
                });
            }

            var canSaveVersion = function () {
                $scope.canSave = false;

                if (validate) {
                    var cpt = 0;
                    tableService.foreachCells($scope.table, function (cell) {
                        if (cell.price.tag != undefined && cell.price.tag != '') {
                            if (!cell.edited) {
                                cpt++;
                            }
                        }
                    });

                    if (cpt == 0) {
                        $scope.canSave = true;
                    }
                } else {
                    for (var i = 0; i < $scope.table.rows.length; i++) {
                        for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                            if ($scope.table.rows[i].cells[j].edited) {
                                $scope.canSave = true;
                                break;
                            }
                        }
                    }
                }
            };

        }
    ]);
});