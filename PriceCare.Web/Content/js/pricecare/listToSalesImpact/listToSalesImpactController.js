define(['pricecare/listToSalesImpact/module'], function (module) {
    'use strict';
    module.controller('ListToSalesImpactController', ['$scope', 'helperService', 'productService', 'countryService', 'versionService', '$q', 'listToSalesImpactService', '$routeParams',
            'userService',
        function ($scope, helperService, productService, countryService, versionService, $q, listToSalesImpactService, $routeParams,
            userService) {

            $scope.listToSalesImpact = [];

            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPageLimited,
                countriesId: [],
                productsId: []
            };

            var countryFilter = {
                type: helperService.FieldTypes.DynamicPop,
                name: 'countries',
                properties:
                {
                    class: 't-custom-select-boxed',
                    directive: 'op-selection-tree-popup-limited',
                    items:[],
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

            var versionFilter = {
                type: helperService.FieldTypes.Select,
                name: "versions",
                properties: {
                    class: 't-custom-select-text',
                    items: []
                }
            };

            $scope.firstVersionSelected = function () {
                if (versionFilter.properties.items && versionFilter.properties.items.length > 0) {
                    return versionFilter.properties.items[0].selected == true;
                }

                return false;
            }

            var productFilter = {
                type: helperService.FieldTypes.Select,
                name: 'products',
                properties: {
                    class: 't-custom-select-text',
                }
            };

            var prepareListToSales = function (listToSales) {
                var countries = [];

                var listToSalesTmp = _.groupBy(listToSales, 'geographyName');
                for (var geographyName in listToSalesTmp) {
                    var newCountry = {
                        geographyId: listToSalesTmp[geographyName][0].geographyId,
                        geographyName: geographyName,
                        segmentName: listToSalesTmp[geographyName][0].segmentName,
                        segmentId: listToSalesTmp[geographyName][0].segmentId,
                        productId: listToSalesTmp[geographyName][0].productId,
                        items: []
                    };
                    for (var i = 0; i < listToSalesTmp[geographyName].length; i++) {
                        newCountry.items.push(listToSalesTmp[geographyName][i]);
                    }
                    countries.push(newCountry);
                }

                return countries;
            };

            var initTable = function (listToSales) {
                var rows = [];

                var countries = prepareListToSales(listToSales);

                rows.push({
                    title: true,
                    cells: [
                        { value: 'Country, Segment', title: true },
                        { value: 'Month 1', title: true },
                        { value: 'Month 2', title: true },
                        { value: 'Month 3', title: true },
                        { value: 'Month 4', title: true },
                        { value: 'Month 5', title: true },
                        { value: 'Month 6', title: true },
                        { value: 'Month 7', title: true },
                        { value: 'Month 8', title: true },
                        { value: 'Month 9', title: true },
                        { value: 'Month 10', title: true },
                        { value: 'Month 11', title: true },
                        { value: 'Month 12', title: true }
                    ]
                });

                for (var i = 0; i < countries.length; i++) {

                    var newCellNoCumulated = [];
                    var newCellCumulated = [];
                    var cptImpactPercentageCumulated = 0;
                    for (var k = 0; k < rows[0].cells.length; k++) {
                        newCellNoCumulated.push({});
                        newCellCumulated.push({});
                    }
                    if (countries[i].items.length > 0) {
                        newCellNoCumulated[0].value = countries[i].geographyName + ", " + countries[i].segmentName;
                        newCellCumulated[0].value = "Cumulative Impact";
                        newCellCumulated[0].displayRight = true;
                    }
                       
                    for (var j = 1; j < rows[0].cells.length; j++) {
                        var item = _.find(countries[i].items, function(it) {
                            return it.impactDelay == j;
                        });
                        if (item != null) { // edit
                            newCellNoCumulated[j].value = item.impactPercentage;
                            newCellNoCumulated[j].original = item;
                            newCellNoCumulated[j].new = helperService.clone(item);
                        } else { // create
                            newCellNoCumulated[j].value = 0.0;
                            newCellNoCumulated[j].original = {
                                geographyId: countries[i].geographyId,
                                segmentId: countries[i].segmentId,
                                productId: countries[i].productId,
                                impactDelay: j,
                                impactPercentage : 0.0
                            };
                            newCellNoCumulated[j].new = helperService.clone(newCellNoCumulated[j].original);
                        }

                        newCellNoCumulated[j].format = helperService.formatPercentage;
                        newCellNoCumulated[j].rightMenu = {
                            fields: [
                                {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    text: "Impact percentage:",
                                    nextRow: rows.length + 1, // to identify non cumulative line
                                    cellValue: newCellNoCumulated[j].value * 100,
                                    properties: {
                                        required: true,
                                        focus: true,
                                        select: true,
                                        allowDecimal: true,
                                        areEquals: function(x, y) {
                                            return x == y;
                                        },
                                        isValid: function(field, cell, row, table) {
                                            var total = 0;
                                            for (var t = 1; t < row.cells.length; t++) {
                                                if (row.cells[t] != cell &&
                                                        row.cells[t].value)
                                                    total += row.cells[t].value;
                                            }
                                            var value = (field.value / 100);
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
                                    cell.new.impactPercentage = fields[0].value / 100;
                                    cell.rightMenu.properties.showReverseEdit = true;
                                    updateCumulativeViewOfImpactPercentage(row.cells, fields[0].nextRow);
                                    $scope.canSave = true;
                                },
                                showReverseEdit: false,
                                reverseEdit : function(fields, cell, row, table) {
                                    cell.new = helperService.clone(cell.original);
                                    cell.edited = false;
                                    cell.value = cell.new.impactPercentage;
                                    cell.rightMenu.fields[0].cellValue = cell.new.impactPercentage * 100;
                                    cell.rightMenu.properties.showReverseEdit = false;
                                    updateCumulativeViewOfImpactPercentage(row.cells, cell.rightMenu.fields[0].nextRow);
                                    canSaveVersion();
                                }
                            }
                        };

                        if(!$scope.firstVersionSelected())
                            newCellNoCumulated[j].blockRightMenu = true;

                        cptImpactPercentageCumulated += newCellNoCumulated[j].value;
                        newCellNoCumulated[j].value = newCellNoCumulated[j].value;
                        newCellCumulated[j].value = cptImpactPercentageCumulated;
                        newCellCumulated[j].fontStyle = 'italic';
                        newCellCumulated[j].format = helperService.formatPercentage;
                    }

                    rows.push({
                        cells: newCellNoCumulated
                    });

                    rows.push({
                        cells: newCellCumulated
                    });

                }

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getListToSalesImpact();
                        },
                        counterText: $scope.counter
                    }
                };
            };

            var updateCumulativeViewOfImpactPercentage = function(cells, indiceInRows) {
                var row = $scope.table.rows[indiceInRows].cells; // fetch the row
                var cptImpactPercentageCumulated = 0;
                for (var i = 1; i < $scope.table.rows[0].cells.length; i++) {
                    cptImpactPercentageCumulated += cells[i].value;
                    row[i].value = cptImpactPercentageCumulated;
                }

                $scope.table.rows[indiceInRows].cells = row;
            };

            var prepareRegionAndCountriesForFilter = function (data) {
                var result = [];

                data.forEach(function (region, i) {
                    var newRegion = {
                        id: region.region.id,
                        text: region.region.name,
                        name: region.region.name.replace(/\s+/g, ''),
                        items: []
                    };

                    region.countries.forEach(function (country) {
                        var newCountry = {
                            id: country.id,
                            text: country.name,
                            name: country.name.replace(/\s+/g, '')
                        };

                        if (($routeParams.geographyId && $routeParams.geographyId == newCountry.id)
                            || (!$routeParams.geographyId && $scope.lastSettingsChosen.defaultRegionId && $scope.lastSettingsChosen.defaultRegionId == newRegion.id)
                            || (!$routeParams.geographyId && !$scope.lastSettingsChosen.defaultRegionId && i == 0)) {
                            newCountry.value = true;
                            newRegion.value = newRegion;
                        } 

                        newRegion.items.push(newCountry);
                    });

                    result.push(newRegion);
                });

                return result;
            };

            var prepareProductsForFilter = function (data) {
                var products = [];

                data.forEach(function (p, i) {
                    products.push({
                        id: p.id,
                        text: p.name,
                        textShort: p.shortname,
                        selected: (($scope.lastSettingsChosen.defaultProductId && $scope.lastSettingsChosen.defaultProductId == p.id)
                            || (!$scope.lastSettingsChosen.defaultProductId && i == 0)) ? true : false
                    });

                });

                return products;
            };

            var prepareVersionsForFilter = function (data) {

                data.forEach(function (version) {
                    version.text = version.information + " / " + helperService.formatDate(version.versionTime) + ' / ' + version.userName;
                    if ($routeParams.versionId && $routeParams.versionId == version.id) {
                        version.selected = true;
                    }
                });

                if (! _.any(data, function(version) { return version.selected == true; }) ) {
                    data[0].selected = true;
                }

                return data;
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
                        products.push(product);
                });
                return _.pluck(products, "id");
            };

            var getSelectedVersionId = function () {
                var version = _.find(versionFilter.properties.items, function (v) {
                    return v.selected == true;
                });
                return (version != undefined) ? version.versionId : 0;
            };

            var resetPaging = function () {
                $scope.searchRequest.pageNumber = 0;
                $scope.listToSalesImpact = [];
            };

            var onSearch = function () {
                $scope.searchRequest.productsId = getSelectedProducts();
                $scope.searchRequest.countriesId = getSelectedCountries();
                $scope.searchRequest.versionId = getSelectedVersionId();
                resetPaging();
                $scope.getListToSalesImpact();
            };

            var reloadVersionsAndGetListToSalesImpact = function () {
                $scope.searchRequest.productsId = getSelectedProducts();
                $scope.searchRequest.countriesId = getSelectedCountries();
                var userMappingPromise = userService.getUserMapping();
                var versionPromise = versionService.getListToSalesVersions($scope.searchRequest);

                $q.all([versionPromise, userMappingPromise]).then(function (datas) {
                    versionFilter.properties.items = versionService.getVersionsForFilter(datas[0], datas[1]);
                    $scope.searchRequest.versionId = getSelectedVersionId();
                    resetPaging();
                    $scope.getListToSalesImpact();
                });
            };

            $scope.getListToSalesImpact = function () {
                listToSalesImpactService.getPaged($scope.searchRequest).then(function (result) {

                    if (result) {
                        $scope.listToSalesImpact = $scope.listToSalesImpact.concat(result.listToSalesImpacts);
                        $scope.canLoadMore = !result.isLastPage;

                        // get countries selected
                        var countriesDisplayed = _.groupBy($scope.listToSalesImpact, "geographyId");
                        $scope.counter = "Load more list to sales impact by countries - "+helperService.getObjectSize(countriesDisplayed) + " out of " + result.totalListToSales;
                        initTable($scope.listToSalesImpact);
                    }

                    $scope.searchRequest.pageNumber++;
                    canSaveVersion();
                });
            };

            var init = function () {
                var countryPromise = countryService.getRegionsAndCountries();
                var productPromise = productService.getAllProducts();
                var userPromise = userService.getLastSettingsChosen();

                $q.all([countryPromise, productPromise, userPromise]).then(function (data) {
                    $scope.lastSettingsChosen = data[2];
                    countryFilter.properties.items = prepareRegionAndCountriesForFilter(data[0]);
                    productFilter.properties.items = prepareProductsForFilter(data[1]);
                    $scope.searchRequest.countriesId = getSelectedCountries();
                    $scope.searchRequest.productsId = getSelectedProducts();

                    var userMappingPromise = userService.getUserMapping();
                    var versionPromise = versionService.getListToSalesVersions($scope.searchRequest);

                    $q.all([versionPromise, userMappingPromise]).then(function(datas) {
                        // define filter
                        $scope.filterOptions = {
                            header: {
                                items: [
                                     {
                                         type: helperService.FieldTypes.Label,
                                         properties:
                                         {
                                             text: "List To Sales Impact For"
                                         }
                                     },
                                    productFilter,
                                    versionFilter
                                ],
                            },
                            filters: [
                                 countryFilter
                            ],
                            showAdvancedFilters: false,
                            onChanged: function (sourceChanged) {
                                if (sourceChanged == countryFilter || sourceChanged == productFilter) {
                                    if (sourceChanged == countryFilter) {
                                        userService.lastSettingsChosen.defaultRegionId = getSelectedRegionId();
                                    } else {
                                        var products = getSelectedProducts();
                                        userService.lastSettingsChosen.defaultProductId = products[0];
                                    }
                                    reloadVersionsAndGetListToSalesImpact();
                                } else {
                                    onSearch();
                                }
                            }
                        };
                        versionFilter.properties.items = versionService.getVersionsForFilter(datas[0], datas[1]);
                        onSearch();
                    });

                });
            }; 

            init();

            var getSelectedRegionId = function () {
                var region = _.find(countryFilter.properties.items, function (item) {
                    return item.value != null;
                });
                return region.id;
            };

            var getEditedRows = function() {
                var rows = [];

                for (var i = 1; i < $scope.table.rows.length; i += 2) { // avoid first row and cumulative row
                    for (var j = 1; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited)
                            rows.push($scope.table.rows[i].cells[j].new);
                    }
                }

                return rows;
            };

            var canSaveVersion = function() {
                $scope.canSave = false;

                for (var i = 1; i < $scope.table.rows.length; i += 2) { // avoid first row and cumulative row
                    for (var j = 1; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            $scope.canSave = true;
                            break;
                        }
                    }
                }
            };

            $scope.saveVersion = function() {
                var data = getEditedRows();
                if(data.length > 0)
                    listToSalesImpactService.saveVersion(data).then(function() {
                        reloadVersionsAndGetListToSalesImpact();
                    }, function() { // if error occurs
                        canSaveVersion();
                    });
            };


        }]);
});