define(['pricecare/priceguidance/module'], function (module) {
    'use strict';
    module.controller('PriceGuidanceController', ['$scope', '$q', 'helperService', 'priceGuidanceService', 'countryService', 'productService', 'versionService',
        'userService', '$modal', 'excelService',
        function ($scope, $q, helperService, priceGuidanceService, countryService, productService, versionService,
            userService, $modal, excelService) {

            $scope.searchRequest = {
                pageNumber : 0,
                itemsPerPage: helperService.itemsPerPage,
                priceTypes : []
            };

            $scope.dataTable = {
                rows : [] 
            };

            var regionCountryFilter = {
                type: helperService.FieldTypes.DynamicPop,
                name: 'countries',
                properties:
                {
                    class: 't-custom-select-boxed',
                    directive: 'op-selection-tree-popup-limited',
                    items: [],
                    getText: function () {
                        if (regionCountryFilter.properties.items.length > 0) {
                            var upItemSelected = _.find(regionCountryFilter.properties.items, function (upItem) {
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
                name:'products',
                properties: { class: 't-custom-select-text' }
            }            
            var ruleTypesFilter = {
                type: helperService.FieldTypes.Select,
                name: 'ruleSenario',
                properties: { class: 't-custom-select-boxed' } 
            }
            var applicableFromFilter = {
                type: helperService.FieldTypes.Select,
                name: 'applicableFrom',
                properties: { class: 't-custom-select-text' }
            }
            var versionFilter = {
                type: helperService.FieldTypes.Select,
                name:'versions',
                properties: { class: 't-custom-select-text' }
            }

            $scope.firstVersionSelected = function () {
                if (versionFilter.properties.items && versionFilter.properties.items.length > 0) {
                    return versionFilter.properties.items[0].selected == true;
                }

                return false;
            }

            $scope.filterOptions = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties: {
                                text: "Price map for"
                            }
                        },
                        productFilter,
                        applicableFromFilter,
                        versionFilter
                    ],
                    primaryDisplayOptions: [
                        {
                            type: helperService.FieldTypes.Action,
                            properties:
                            {
                                text: 'Export to Excel',
                                class: 'color-white icon-before icon-before-excel',
                                callback: function () {
                                    var modalInstance = $modal.open({
                                        templateUrl: 'Content/js/pricecare/modal/excelFilterModal.html',
                                        controller: 'ExcelFilterModalController',
                                        backdrop: 'static',
                                        resolve: {
                                            infos: function () {
                                                return {
                                                    noFilterOnEvent: true
                                                }
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function (response) {
                                        $scope.searchRequest.allCountries = response.allCountries;
                                        $scope.searchRequest.allProducts = response.allProducts;
                                        $scope.searchRequest.databaseLike = response.databaseLike;
                                        excelService.postFilterExcel($scope.searchRequest).then(function (result) {
                                            window.location.href = 'api/priceguidance/excel?token=' + result.token;
                                        });
                                    }, function () {

                                    });
                                }
                            }
                        }
                    ],
                },
                filters: [
                    regionCountryFilter,
                    ruleTypesFilter
                ],
                onChanged: function (sourceChanged) {
                    if (sourceChanged == regionCountryFilter || sourceChanged == productFilter) {
                        if (sourceChanged == regionCountryFilter) {
                            userService.lastSettingsChosen.defaultRegionId = getSelectedRegionId();
                        } else if (sourceChanged == productFilter) {
                            userService.lastSettingsChosen.defaultProductId = getSelectedProductId();
                        }
                        getVersions();
                    }
                    
                    searchPriceMap();
                }
            }

            var getRegionsForFilter = function (data) {
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
                            value: (newRegion.value != null) ? true : false
                        };

                        newRegion.items.push(newCountry);
                    });

                    result.push(newRegion);
                });

                return result;
            }

            var getFilterItems = function (list) {
                var items = [];
                list.forEach(function (element, i) {
                    items.push({
                        id: element.id,
                        text: element.name,
                        selected: i == 0
                    });
                });
                return items;
            }

            var getSelectedProductId = function () {
                var productId = 0;
                productFilter.properties.items.forEach(function (product) {
                    if (product.selected)
                        productId = product.id;
                });
                return productId;
            };

            var getSelectedRuleTypeId = function () {
                var id = 0;
                ruleTypesFilter.properties.items.forEach(function (item) {
                    if (item.selected)
                        id = item.id;
                });
                return id;
            };

            var getSelectedCountriesId = function () {
                var ids = [];
                for (var i = 0; i < regionCountryFilter.properties.items.length; i++) {
                    var region = regionCountryFilter.properties.items[i];
                    for (var j = 0; j < region.items.length; j++) {
                        if (region.items[j].value) {
                            ids.push(region.items[j].id);
                        }
                    }
                }
                return ids;
            };

            var getSelectedVersionId = function () {
                var id = 0;
                versionFilter.properties.items.forEach(function (item) {
                    if (item.selected)
                        id = item.versionId;
                });
                return id;
            };

            var getSelectedRegionId = function () {
                var region = _.find(regionCountryFilter.properties.items, function (item) {
                    return item.value != null;
                });
                return region.id;
            };

            var getApplicableFrom = function () {
                var result;

                if (applicableFromFilter.properties.items) {
                    applicableFromFilter.properties.items.forEach(function (date) {
                        if (date.selected) {
                            result = date.text;
                        }
                    });
                }
                if (result)
                    result = result + '-01 00:00:00Z'; //BAD but necessary...
                return result;
            }

            var prepareProductsForFilter = function (productsResponse) {
                var products = [];
                productsResponse.push({ id: 0, name: 'All products', shortName: "All products" });
                productsResponse.forEach(function (p) {
                    products.push({
                        id: p.id,
                        text: p.name,
                        textShort: p.name,
                        selected: ($scope.lastSettingsChosen.defaultProductId && $scope.lastSettingsChosen.defaultProductId == p.id)
                            || (!$scope.lastSettingsChosen.defaultProductId && i == 0) ? true : false
                    });
                });

                return products;
            };

            var getSelectedPriceType = function(items) {
                var priceType = _.find(items, function(it) { return it.selected; });
                return priceType != undefined ? priceType.shortText : "XX";
            };

            var getSelectedPriceTypeId = function(items) {
                var priceType = _.find(items, function (it) { return it.selected; });
                return priceType != undefined ? priceType.id : 0;
            };

            var setSelectedPriceType = function (cell) {
                var result = [];
                var priceTypeId = cell.reviewedPrice ? cell.reviewedPrice.reviewedPriceTypeId : cell.referencedPrice.referencedPriceTypeId;
                cell.priceTypeOptions.forEach(function(pt) {
                    result.push({
                        id: pt.id,
                        shortText: pt.shortName,
                        text: pt.name,
                        selected: (pt.id == priceTypeId) ? true : false
                    });
                });
                return result;
            };

            var setSelectedPriceTypeForId = function (priceTypes, priceTypeId) {
                for (var i = 0; i < priceTypes.length; i++) {
                    priceTypes[i].selected = priceTypes[i].id == priceTypeId ? true : false;
                }
                return priceTypes;
            };

            var setTable = function () {
                var columnNames = ["Referencing", "Referenced"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });
                var parentRow = {};
                $scope.dataTable.rows.forEach(function (tableRow, i) {
                    var row = { cells: [] };
                    tableRow.cells.forEach(function (c, j) {
                        var tmpCell = {
                            value: c.text
                        };
                        if (c.isEditable) {

                            if (j == 0) {
                                tmpCell.referencing = true;
                                tmpCell.actions = [
                                    {
                                        text: 'Expand',
                                        class: 'table-expandable-row icon-collapsed',
                                        click: function(cell, row, table, action) {
                                            row.expanded = row.expanded ? false : true;
                                            action.text = row.expanded ? "Collapse" : "Expand";
                                            action.class = row.expanded ? "table-expandable-row icon-expanded" : "table-expandable-row icon-collapsed";
                                        }
                                    }
                                ];
                                parentRow = row;
                            } else {
                                row.parentRow = parentRow;
                            }
                            

                            var isReviewedPrice = c.reviewedPrice ? true : false;
                            var originalPriceGuidance = isReviewedPrice ? c.reviewedPrice : c.referencedPrice;
                            tmpCell.reviewedPrice = c.reviewedPrice;
                            tmpCell.referencedPrice = c.referencedPrice;
                            tmpCell.originalPriceGuidance = helperService.clone(originalPriceGuidance);
                            tmpCell.originalPriceGuidance.isReviewedPrice = isReviewedPrice;
                            var priceTypes = setSelectedPriceType(c);
                            if (!$scope.firstVersionSelected())
                                tmpCell.blockRightMenu = true;
                            tmpCell.rightMenu = {
                                fields: [
                                {
                                    type: helperService.FieldTypes.Select,
                                    text: "Price Type: ",
                                    properties: {
                                        class: 't-custom-select-boxed',
                                        items: priceTypes
                                    }
                                },
                                {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    text: "Adjustement: ",
                                    //cellValue: isReviewedPrice ? helperService.formatPercentage(c.reviewedPrice.reviewedPriceAdjustment)
                                    //    : helperService.formatPercentage(c.referencedPrice.referencedPriceAdjustment),
                                    cellValue: isReviewedPrice ? c.reviewedPrice.reviewedPriceAdjustment : c.referencedPrice.referencedPriceAdjustment,
                                    properties: {
                                            select: true,
                                            allowNegative: true,
                                            allowDecimal: true,
                                            min: -100.0,
                                            max: 100.0,
                                            unit:'%'
                                        }
                                    }
                                ],
                                properties: {
                                    apply: function (fields, cell, row, table) {
                                        $scope.dataTable.rows[i].cells[j].edited = true;
                                        var priceType = getSelectedPriceType(fields[0].properties.items);
                                        var priceTypeId = getSelectedPriceTypeId(fields[0].properties.items);
                                        cell.value = isReviewedPrice ? c.reviewedPrice.geography : c.referencedPrice.referencedGeography; 
                                        fields[0].cellValue = priceType;
                                        if (cell.originalPriceGuidance.isReviewedPrice) {
                                            cell.reviewedPrice.reviewedPriceAdjustment = parseFloat(fields[1].value / 100);
                                            cell.reviewedPrice.reviewedPriceTypeId = priceTypeId;
                                            cell.reviewedPrice.reviewedPriceType = priceType;
                                        } else {
                                            cell.referencedPrice.referencedPriceAdjustment = parseFloat(fields[1].value / 100);
                                            cell.referencedPrice.referencedPriceTypeId = priceTypeId;
                                            cell.referencedPrice.referencedPriceType = priceType;
                                        }

                                        cell.value = isReviewedPrice ? c.reviewedPrice.geography : c.referencedPrice.referencedGeography;
                                        if (priceType != "XX")
                                            cell.value += " - " + priceType;
                                        if (parseFloat(fields[1].value) > 0.0)
                                            cell.value += " (" + fields[1].value + ")";

                                        cell.rightMenu.fields[1].cellValue = fields[1].value;
                                        cell.rightMenu.properties.showReverseEdit = true;
                                        $scope.canSave = true;
                                    },
                                    showReverseEdit: false,
                                    reverseEdit: function (fields, cell, row, table) {
                                        $scope.dataTable.rows[i].cells[j].edited = false;
                                        var priceType = "";
                                        if (cell.originalPriceGuidance.isReviewedPrice) {
                                            cell.reviewedPrice.reviewedPriceAdjustment = cell.originalPriceGuidance.reviewedPriceAdjustment;
                                            cell.reviewedPrice.reviewedPriceTypeId = cell.originalPriceGuidance.reviewedPriceTypeId;
                                            cell.reviewedPrice.reviewedPriceType = cell.originalPriceGuidance.reviewedPriceType;
                                            cell.rightMenu.fields[1].cellValue = helperService.formatPercentage(cell.reviewedPrice.reviewedPriceAdjustment);
                                            cell.rightMenu.fields[0].cellValue = cell.originalPriceGuidance.reviewedPriceTypeId;
                                            cell.rightMenu.fields[0].properties.items =
                                                setSelectedPriceTypeForId(cell.rightMenu.fields[0].properties.items, cell.reviewedPrice.reviewedPriceTypeId);
                                            priceType = cell.originalPriceGuidance.reviewedPriceType;
                                        } else {
                                            cell.referencedPrice.referencedPriceAdjustment = cell.originalPriceGuidance.referencedPriceAdjustment;
                                            cell.referencedPrice.referencedPriceTypeId = cell.originalPriceGuidance.referencedPriceTypeId;
                                            cell.referencedPrice.referencedPriceType = cell.originalPriceGuidance.referencedPriceType;
                                            cell.rightMenu.fields[1].cellValue = helperService.formatPercentage(cell.referencedPrice.referencedPriceAdjustment);
                                            cell.rightMenu.fields[0].cellValue = cell.originalPriceGuidance.reviewedPriceTypeId;
                                            cell.rightMenu.fields[0].properties.items =
                                                setSelectedPriceTypeForId(cell.rightMenu.fields[0].properties.items, cell.referencedPrice.referencedPriceTypeId);
                                            priceType = cell.originalPriceGuidance.referencedPriceType;
                                        }
                                        cell.value = isReviewedPrice ? c.reviewedPrice.geography : c.referencedPrice.referencedGeography;
                                        if (priceType != "XX")
                                            cell.value += " - " + priceType;
                                        if ((cell.originalPriceGuidance.isReviewedPrice && parseFloat(cell.originalPriceGuidance.reviewedPriceAdjustment) > 0.0)
                                            || (!cell.originalPriceGuidance.isReviewedPrice && parseFloat(cell.originalPriceGuidance.referencedPriceAdjustment) > 0.0))
                                            cell.value += " (" + ((cell.originalPriceGuidance.isReviewedPrice) ? cell.originalPriceGuidance.reviewedPriceAdjustment * 100
                                                : cell.originalPriceGuidance.referencedPriceAdjustment * 100) + ")";
                                        
                                        cell.rightMenu.properties.showReverseEdit = false;
                                        cell.edited = false;
                                        canSaveVersion();
                                    }
                                }
                            }
                        }
                        row.cells.push(tmpCell);
                    });
                    rows.push(row);
                });

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getData();
                        },
                        counterText: $scope.counter
                    }
                };
            }

            var getBaseScreenInfo = function () {
                var userPromise = userService.getLastSettingsChosen();
                var regionCountriesPromise = countryService.getRegionsAndCountries();
                var productPromise = productService.getAllProducts();
                var ruleTypePromise = priceGuidanceService.getRuleTypes();

                $q.all([productPromise, regionCountriesPromise, ruleTypePromise, userPromise]).then(function (data) {
                    $scope.lastSettingsChosen = data[3];
                    ruleTypesFilter.properties.items = getFilterItems(data[2]);
                    regionCountryFilter.properties.items = getRegionsForFilter(data[1]);
                    productFilter.properties.items = prepareProductsForFilter(data[0]);
                    getVersions();
                });
            }

            getBaseScreenInfo();

            var getVersions = function () {
                var selectedProductId = getSelectedProductId();
                var selectedCountryId = getSelectedCountriesId();
                var selectedRuleTypeId = getSelectedRuleTypeId();

                var userMappingPromise = userService.getUserMapping();
                var versionPromise = versionService.getPriceMapVersions({
                    productId: selectedProductId,
                    geographyIds: selectedCountryId,
                    ruleTypeId: selectedRuleTypeId
                });

                $q.all([versionPromise, userMappingPromise]).then(function (data) {
                    versionFilter.properties.items = versionService.getVersionsForFilter(data[0], data[1]);

                    getApplicableFroms();
                });
            };

            var getApplicableFroms = function () {
                var geographyIds = getSelectedCountriesId(); 
                var productId = getSelectedProductId();
                var ruleTypeId = getSelectedRuleTypeId();
                var versionId = getSelectedVersionId();

                var request = {
                    geographyIds: geographyIds, 
                    gprmRuleTypeId: ruleTypeId,
                    productId: productId,
                    versionId: versionId
                };

                priceGuidanceService.getApplicableFromList(request).then(function (data) {
                    applicableFromFilter.properties.items = data;
                    applicableFromFilter.properties.items.forEach(function (appFrom, index) {
                        appFrom.originalText = appFrom.text;
                        appFrom.text = appFrom.text.substring(0, 7);
                        appFrom.selected = index == 0;
                    });

                    searchPriceMap();
                });
            };

            var searchPriceMap = function() {
                $scope.searchRequest.pageNumber = 0;
                $scope.dataTable.rows = [];
                $scope.getData();
            };

            $scope.getData = function () {
                var countriesId = getSelectedCountriesId();
                var productId = getSelectedProductId();
                var ruleTypeId = getSelectedRuleTypeId();
                var applicableFrom = getApplicableFrom();
                var versionId = getSelectedVersionId();

                $scope.searchRequest.countriesId = countriesId;
                $scope.searchRequest.productId = productId;
                $scope.searchRequest.ruleTypeId = ruleTypeId;
                $scope.searchRequest.applicableFrom = applicableFrom;
                $scope.searchRequest.pageNumber = $scope.searchRequest.pageNumber;
                $scope.searchRequest.itemsPerPage = $scope.searchRequest.itemsPerPage;
                $scope.searchRequest.versionId = versionId;

                priceGuidanceService.getAllPriceGuidances($scope.searchRequest).then(function (result) {
                    $scope.searchRequest.pageNumber++;
                    if (result) {
                        $scope.dataTable.rows = $scope.dataTable.rows.concat(result.dataTable.rows);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more price map - " + getReferencingCountry() + " out of " + result.totalPriceMap;
                        setTable();
                        canSaveVersion();
                    }
                });
            };

            var getReferencingCountry = function () {
                var cpt = 0;
                for (var i = 0; i < $scope.dataTable.rows.length; i++) {
                    cpt += $scope.dataTable.rows[i].cells[0].isEditable ? 1 : 0;
                }
                return cpt;
            };

            var canSaveVersion = function () {
                $scope.canSave = false;

                for (var i = 1; i < $scope.table.rows.length; i++) {
                    for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            $scope.canSave = true;
                            break;
                        }
                    }
                }
            };

            var getEditedRows = function() {
                var editedRows = [];

                for (var i = 0; i < $scope.dataTable.rows.length; i++) {
                    if ($scope.dataTable.rows[i].cells[0].edited || $scope.dataTable.rows[i].cells[1].edited) 
                        editedRows.push($scope.dataTable.rows[i]);
                }

                return editedRows;
            };

            $scope.saveVersion = function () {
                var rows = getEditedRows();
                var applicableFrom = getApplicableFrom();
                var productId = getSelectedProductId();
                var ruleTypeId = getSelectedRuleTypeId();
                var data = { rows: rows, applicableFrom: applicableFrom, productId: productId, ruleTypeId: ruleTypeId }
                
                if (rows.length > 0) {
                    priceGuidanceService.savePriceMap(data).then(function (result) {
                        if (result)
                            getVersions();
                    });
                }
            };

        }]);
});