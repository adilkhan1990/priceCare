define(['pricecare/forecast/module'], function (module) {
    'use strict';
    module.controller('PricesForecastController', ['$scope', '$q', 'helperService', 'priceService', 'productService', 'countryService', 'priceTypeService', '$http', 'dimensionService', 'userService',
        '$modal', 'simulationService', 'tableService', 'excelService',
        function ($scope, $q, helperService, priceService, productService, countryService, priceTypeService, $http, dimensionService, userService
            , $modal, simulationService, tableService, excelService) {

            $scope.showBadFilter = false;
            var countryPromise;
            var simulationPromise;
            var currencyFilter = helperService.getCurrencyFilter();
            var priceTypePromise = priceTypeService.getAllPriceTypes(0);
            var eventTypePromise = dimensionService.getEventTypes();
            var eventTypes;
            var isLaunch = false;
            var isSimulationInCache = false;
            $scope.searchRequest = {};
            $scope.canSave = false;
            var simulatioInfo;
            var currentSimulation = null;

            $scope.saveEnabled = function () {
                if (currentSimulation != null) {
                    if (currentSimulation.isCurrentUser == true)
                        return true;

                    return false;
                }
                return false;
            }

            $scope.updateEnabled = function () {
                if (currentSimulation != null) {
                    return currentSimulation.isCurrentUser;
                }
                else {
                    return false;
                }
            }

            $scope.createSimulation = function () {
                if (!$scope.inProgressCreateSimulation) {
                    $scope.inProgressCreateSimulation = true;

                    var modalInstance = $modal.open({
                        templateUrl: 'Content/js/pricecare/modal/createSimulationModal.html',
                        controller: 'CreateSimulationModalController',
                        backdrop: 'static',
                        resolve: {
                            infos: function () {
                                return {
                                    productId: getSelectedProductId(),
                                };
                            }
                        }
                    });

                    modalInstance.result.then(function (data) {
                        simulationService.create(data).then(function (data) {
                            currentSimulation = data;
                            $scope.simulationId = data.id;
                            getCountriesAndProducts();
                        });
                    }, function () {
                    });
                }
            };

            $scope.saveVersion = function () {
                if ($scope.saveEnabled) {
                    var modalInstance = $modal.open({
                        templateUrl: 'Content/js/pricecare/modal/saveSimulationModal.html',
                        controller: 'SaveSimulationModalController',
                        backdrop: 'static',
                        resolve: {
                            infos: function () {
                                return {
                                    simulationOption: getSelectedSimulationOption() != null ? getSelectedSimulationOption().id : 'Budget',
                                    simulationName: getSelectedSimulationOptionItem() != null ? getSelectedSimulationOptionItem().name : '',
                                    isCurrentUser: currentSimulation != null ? currentSimulation.isCurrentUser : false,
                                };
                            }
                        }
                    });
                    modalInstance.result.then(function (save) {
                        var data = { save: save, simulationId: $scope.simulationId }
                        simulationService.saveSimulation(data).then(function (result) {
                            currentSimulation = result;
                            $scope.canSave = false;
                            getCountriesAndProducts();


                        });
                    });
                }
            };

            $scope.updateVersion = function () {
                var editedCells = [];

                $scope.table.rows.forEach(function (row) {
                    for (var i = 0; i < row.cells.length; i++)
                        if (row.cells[i].edited)
                            editedCells.push(row.cells[i].price);
                });

                if (editedCells.length > 0) {
                    var saveId = getSelectedSaveId();
                    simulationService.updateSimulationCache({ updatedData: editedCells, simulationId: $scope.simulationId }).then(function (result) {
                        getCountriesAndProducts();
                        $scope.canSave = true;
                    });
                }
            };

            var simulationFilter = {
                type: helperService.FieldTypes.SelectMultiLevel,
                name: 'simulations',
                properties: {
                    class: 't-custom-select-text',
                    items: []
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
            }

            var productFilter = {
                type: helperService.FieldTypes.Select,
                properties:
                {
                    class: 't-custom-select-text',
                }
            };

            var changeSelectedProductId = function (productId) {
                productFilter.properties.items.forEach(function (product) {
                    if (product.id == productId) {
                        product.selected = true;
                        userService.defaultProductId = product.id;
                    }
                    else
                        product.selected = false;
                });
            };

            var consolidationFilter = {
                type: helperService.FieldTypes.Select,
                properties:
                {
                    class: 't-custom-select-text',
                }
            };

            var buttonDownloadSalesImpactExcel = {
                type: helperService.FieldTypes.Action,                
                properties:
                {
                    text: 'Sales Impact',
                    class: 'color-white icon-before icon-before-excel',
                    callback: function () {
                        var modalInstance = $modal.open({
                            templateUrl: 'Content/js/pricecare/modal/excelFilterModal.html',
                            controller: 'ExcelFilterModalController',
                            backdrop: 'static',
                            resolve: {
                                infos: function () {
                                    return {
                                        noFilterOnCountry: true,
                                        showAllEventsOption: false
                                    };
                                }
                            }
                        });

                        modalInstance.result.then(function (response) {
                            var products = [];
                            products.push(getSelectedProductId());
                            var searchRequest = {
                                saveId: getSelectedSaveId(),
                                simulationId: $scope.simulationId,
                                products: products,                                
                                allProducts: response.allProducts,
                                allEvents: response.allEvents,
                                databaseLike: response.databaseLike
                            };
                            excelService.postFilterExcel(searchRequest).then(function (result) {
                                window.location.href = 'api/analyzer/excelSalesImpact?token=' + result.token;
                            });
                        }, function () {

                        });
                    }

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
                    max: 200
                }
            };

            var compareToFilter = {
                type: helperService.FieldTypes.SelectMultiLevel,
                text: 'Compare to',
                name: 'compareTo',
                properties:
                {
                    class: 't-custom-select-text',
                    items: [{
                        text: 'None',
                        selected: true,
                        items: [{ id: null, text: 'None', selected: true }]
                    }]
                }
            }

            var init = function () {
                var lastSettingsChosenPromise = userService.getLastSettingsChosen();
                var userInfoPromise = userService.getUserInfo();
                $q.all([lastSettingsChosenPromise, userInfoPromise]).then(function (datas) {
                    $scope.lastSettingsChosen = datas[0];
                    $scope.userInfo = datas[1]; // for role role definition
                    $scope.userService = userService;
                    getCountriesAndProducts();
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
                                simulationFilter
                            ],
                            primaryDisplayOptions: [
                                buttonDownloadSalesImpactExcel,
                                {
                                    type: helperService.FieldTypes.Action,
                                    properties:
                                    {
                                        text: 'All Prices',
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
                                                        };
                                                    }
                                                }
                                            });

                                            modalInstance.result.then(function (response) {
                                                var searchRequest = {
                                                    geographyId: getSelectedCountriesIds(),
                                                    productId: [getSelectedProductId()],
                                                    saveId: getSelectedSaveId(),
                                                    simulationId: $scope.simulationId,
                                                    allCountries: response.allCountries,
                                                    allProducts: response.allProducts,
                                                    databaseLike: response.databaseLike,
                                                    dataTypeId: 41
                                                };
                                                excelService.postFilterExcel(searchRequest).then(function (result) {
                                                    window.location.href = 'api/price/excelForecast?token=' + result.token;
                                                });
                                            }, function () {

                                            });
                                        }

                                    }
                                },
                                {
                                    type: helperService.FieldTypes.Action,
                                    properties:
                                    {
                                        text: 'Reviewed Price',
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
                                                        };
                                                    }
                                                }
                                            });

                                            modalInstance.result.then(function (response) {
                                                var searchRequest = {
                                                    geographyIds: getSelectedCountriesIds(),
                                                    productId: getSelectedProductId(),
                                                    saveId: getSelectedSaveId(),
                                                    simulationId: $scope.simulationId,
                                                    allCountries: response.allCountries,
                                                    allProducts: response.allProducts,
                                                    databaseLike: response.databaseLike
                                                };
                                                excelService.postFilterExcel(searchRequest).then(function (result) {
                                                    window.location.href = 'api/price/excelReviewedForecast?token=' + result.token;
                                                });
                                            }, function () {

                                            });
                                        }
                                    }
                                },
                                {
                                    type: helperService.FieldTypes.Action,
                                    properties:
                                    {
                                        text: 'Price Changes',
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
                                                        };
                                                    }
                                                }
                                            });

                                            modalInstance.result.then(function (response) {
                                                var searchRequest = {
                                                    geographyIds: getSelectedCountriesIds(),
                                                    productId: getSelectedProductId(),
                                                    saveId: getSelectedSaveId(),
                                                    simulationId: $scope.simulationId,
                                                    allCountries: response.allCountries,
                                                    allProducts: response.allProducts,
                                                    databaseLike: response.databaseLike
                                                };
                                                excelService.postFilterExcel(searchRequest).then(function (result) {
                                                    window.location.href = 'api/price/excelReviewedChanges?token=' + result.token;
                                                });
                                            }, function () {

                                            });
                                        }
                                    }
                                },
                                 {
                                     type: helperService.FieldTypes.Action,
                                     properties:
                                     {
                                         text: 'ASP',
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
                                                         };
                                                     }
                                                 }
                                             });

                                             modalInstance.result.then(function (response) {
                                                 var products = [];
                                                 products.push(getSelectedProductId());

                                                 var searchRequest = {
                                                     saveId: getSelectedSaveId(),
                                                     simulationId: $scope.simulationId,
                                                     countries: getSelectedCountriesIds(),
                                                     products: products,
                                                     allCountries: response.allCountries,
                                                     allProducts: response.allProducts,
                                                     allEvents: response.allEvents,
                                                     databaseLike: response.databaseLike
                                                 };
                                                 excelService.postFilterExcel(searchRequest).then(function (data) {
                                                     window.location.href = 'api/price/excelAspForecast?token=' + data.token;
                                                 });
                                             }, function () {

                                             });
                                         }
                                     }
                                 }
                            ],
                            displayOptions: [
                                displayOnlyChangingDataFilter,
                                highlightTresholdFilter,
                                compareToFilter
                            ],
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
                                } else {
                                    var productId = getSelectedProductId();
                                    if (productId != 0)
                                        userService.lastSettingsChosen.defaultProductId = getSelectedProductId();
                                }
                                getConsolidations(getSelectedCountriesIds(), getSelectedProductId());
                            } else if (sourceChanged == simulationFilter) {
                                if (getSelectedSimulationOptionItem().isLaunch)
                                {
                                    changeSelectedProductId(15);
                                }
                                isSimulationInCache = false;
                                
                                resetCompareToFilter();
                                loadCacheAndReloadData();
                            }
                            else {
                                getData();
                            }
                        }
                    };
                });
            };

            init();

            var getSelectedSimulationOption = function () {
                var simulationOption = _.find(simulationFilter.properties.items, function (s) {
                    return s.selected == true;
                });

                return simulationOption;
            }

            var getSelectedSimulationOptionItem = function () {
                var simulation = getSelectedSimulationOption();
                if (simulation == null) {
                    if (currentSimulation != null)
                        return currentSimulation;
                    return null;
                }
                var item = _.find(simulation.items, function (i) {
                    return i.selected == true;
                });

                return item;
            }

            var getSelectedCompareSimulationOption = function () {
                var simulationOption = _.find(compareToFilter.properties.items, function (s) {
                    return s.selected == true;
                });

                return simulationOption;
            }

            var getSelectedCompareSimulationOptionItem = function () {
                var simulation = getSelectedCompareSimulationOption();
                if (simulation == null) {
                    if (currentSimulation != null)
                        return currentSimulation;
                    return null;
                }
                var item = _.find(simulation.items, function (i) {
                    return i.selected == true;
                });

                return item;
            }

            var resetCompareToFilter = function () {
                compareToFilter.properties.items.forEach(function (lvl1, i) {
                    lvl1.selected = i == 0;
                    lvl1.items.forEach(function (lvl2, y) {
                        lvl2.selected = y == 0;
                    });
                });
            }

            var getSelectedSaveId = function () {
                for (var i = 0; i < simulationFilter.properties.items.length; i++) {
                    var save = _.find(simulationFilter.properties.items[i].items, function (saveTmp) {
                        return saveTmp.selected == true;
                    });
                    if (save != null) break;
                }

                return (save == null) ? 0 : save.id;
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

            var getSelectedConsolidation = function () {
                return getSelectedFilterItem(consolidationFilter);
            };

            var getSelectedFilterItem = function (filter) {
                var selectedItem = _.find(filter.properties.items, function (item) {
                    return item.selected == true;
                });
                return selectedItem;
            }

            var getCountriesAndProducts = function () {
                countryPromise = countryService.getRegionsAndCountries();
                var productPromise = productService.getAllProducts();
                simulationPromise = simulationService.getSimulations();
                var firstSimulationPromise = simulationService.getFirstSimulation();
                $q.all([countryPromise, productPromise, simulationPromise, firstSimulationPromise]).then(function (data) {
                    if (data[3] != null) {
                        isSimulationInCache = true;
                        isLaunch = data[3].isLaunch;
                        simulatioInfo = data[3];
                    }
                    countryFilter.properties.items = prepareRegionForFilter(data[0]);
                    productFilter.properties.items = prepareProductsForFilter(data[1], false);
                    simulationFilter.properties.items = simulationService.prepareSimulationForFilter(data[2]);
                    compareToFilter.properties.items =
                        compareToFilter.properties.items.concat(simulationService.prepareSimulationCompareForFilter(data[2]));
                    getConsolidations(getSelectedCountriesIds(), getSelectedProductId());


                });
            };

            var getConsolidations = function (countryIds, productId) {
                var consolidationPromise = $http.get('api/consolidation/' + productId);
                consolidationPromise.then(function (data) {
                    consolidationFilter.properties.items = prepareConsolidationsForFilter(data.data);
                        if (isSimulationInCache) {
                            currentSimulation = simulatioInfo;
                            changeSelectedSaveId(currentSimulation.saveId);
                            $scope.simulationId = currentSimulation.id;
                            $scope.searchRequest.saveId = currentSimulation.saveId;
                            getData();
                        } else {
                            loadCacheAndReloadData();
                        }
                });

            };

            var changeSelectedSaveId = function (saveId) {
                var parentSimulation = simulationFilter.properties.items;
                parentSimulation.forEach(function (parent) {
                    var childFound = false;
                    for (var i = 0; i < parent.items.length; i++) {
                        parent.items[i].selected = parent.items[i].id == saveId;
                        if (!childFound) childFound = parent.items[i].id == saveId;
                    }
                    parent.selected = childFound;
                });
            };

            var loadCacheAndReloadData = function () {
                var saveId = getSelectedSaveId();
                
                if (saveId && saveId != 0) {
                    simulationService.loadSimulation(saveId, getSelectedProductId()).then(function (data) {
                        $scope.simulationId = data.id;
                        currentSimulation = data;
                        getData();
                    });
                } else {
                    $scope.table = null;
                }
            };

            var prepareConsolidationsForFilter = function (data) {
                data.forEach(function (consolidation) {
                    if (consolidation.isDefault) {
                        consolidation.selected = true;
                    }
                    consolidation.text = consolidation.factorScreen + " " + consolidation.name;
                });

                return data;
            };

            var prepareRegionForFilter = function (data) {
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

            

            

            var getData = function () {
                $scope.showBadFilter = false;

                $scope.searchRequest.productId = getSelectedProductId();
                $scope.searchRequest.geographyIds = getSelectedCountriesIds();
                $scope.searchRequest.simulationId = $scope.simulationId;
                $scope.searchRequest.saveId = getSelectedSimulationOptionItem().id;
                $scope.searchRequest.compareTo = getSelectedCompareSimulationOptionItem().id;
                var pricePromise = priceService.getPricesForecast($scope.searchRequest);

                $q.all([pricePromise, countryPromise, priceTypePromise, eventTypePromise]).then(function (data) {
                    if (data[0].length == 0) {

                        $scope.table = null;
                        $scope.showBadFilter = true;
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
                        var parentRegionRows = [];
                        for (var countryId in region.countries) {
                            var country = region.countries[countryId];
                            var countryData = countryGroups[country.id];
                            var parentTypeRow = {};
                            if (countryData) {
                                var priceTypeGroups = _.groupBy(countryData, 'priceTypeId');
                                for (var priceTypeId in priceTypeGroups) {
                                    var priceTypeData = priceTypeGroups[priceTypeId];
                                    var parentTypeReviewed = _.find(priceTypeData, function (p) { return p.reviewed });

                                    if (parentTypeReviewed != null && country.id == parentTypeReviewed.geographyId)
                                    {
                                        parentTypeRow.cells
                                        = [
                                            {
                                                value:
                                                  country.name + " - " + _.find($scope.priceTypes, function (pt) { return pt.id == priceTypeId; }).shortName,
                                                actions: [
                                                {
                                                    text: 'Expand',
                                                    class: 'table-expandable-row icon-collapsed',
                                                    click: function (cell, row, table, action) {
                                                        row.expanded = row.expanded ? false : true;
                                                        action.text = row.expanded ? "Collapse" : "Expand";
                                                        action.class = row.expanded ? "table-expandable-row icon-expanded" : "table-expandable-row icon-collapsed";
                                                    }
                                                }
                                                ]
                                            }
                                        ];
                                    }
                                    
                                    
                                    var priceTypeRow = {
                                        parentRow: parentTypeRow,
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
                                        newCell.rightMenu = getRightMenu(newCell);
                                        
                                        //newCell.blockleftMenu = true;
                                        setCellDisplay(newCell, currentCurrency.id, consolidation, highlightTresholdFilter.value / 100, displayOnlyChangingDataFilter.value, priceTypeRow);
                                        if(price.description != null)
                                            newCell.leftMenu = getLeftMenu(newCell);

                                        if (parentTypeReviewed != null && country.id == parentTypeReviewed.geographyId)
                                            parentTypeRow.cells.push(newCell);
                                        else
                                            priceTypeRow.cells.push(newCell);
                                    }
                                    if (parentTypeReviewed != null && country.id == parentTypeReviewed.geographyId)
                                        parentRegionRows.push(parentTypeRow);
                                    else
                                        regionRows.push(priceTypeRow);
                                }
                                
                            }
                            rows = rows.concat(parentRegionRows);
                            if (regionRows.length >= 1) {
                                rows = rows.concat(regionRows);
                            }

                            parentRegionRows = [];
                            regionRows = [];

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
                });
            };

            var setCellDisplay = function (cell, currency, consolidation, highlightThreshold, displayOnlyChangingData, row) {
                if (cell.title) {
                    return;
                }

                if (cell.price && cell.price.value) {
                    cell.empty = false;
                    var isChanging = cell.price.percentageVariation && Math.abs(cell.price.percentageVariation) > highlightThreshold;

                    // Background for changing price
                    if (isChanging) {
                        //cell.blockLeftMenu = false;
                        cell.leftMenu = getLeftMenu(cell);
                        cell.background = "#FDF8BC";
                    } else {
                        cell.background = '';
                        //cell.blockLeftMenu = true;
                        //cell.leftMenu = null;
                    }
                    // Background for event type
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
                            //cell.blockLeftMenu = true;
                            cell.format = null;
                            cell.value = null;

                            if (_.all(row.cells.slice(1), function (c) { return c.value == null; })) {
                                row.hide = true;
                            } else {
                                row.hide = false;
                            }
                        }
                    } else {
                        row.hide = false;
                        //cell.blockLeftMenu = false;
                        cell.blockRightMenu = false;
                        if (getSelectedCompareSimulationOptionItem().id != null)
                            cell.blockRightMenu = true;
                        cell.value = priceService.getPriceValue(cell.price, currency, consolidation);
                        cell.format = helperService.formatNumber;


                    }

                    cell.rightMenu.fields[0].cellValue = cell.value;


                    var dataTime = new Date(cell.price.dataTime);
                    var selectedSimulation = getSelectedSimulationOptionItem();
                    var selectedSimulationStartTime = new Date(selectedSimulation.startTime);
                    if (dataTime > selectedSimulationStartTime) {
                        cell.fontStyle = 'italic';
                        cell.blockRightMenu = true;
                    }
                } else {
                    if (!displayOnlyChangingData) {
                        cell.empty = true;
                        cell.value = null;
                    } else {
                        //cell.blockLeftMenu = true;
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
                    content: '<strong>The value has changed, reload the page to recalculate the left view.</strong>'
                };
            };

            var getRightMenu = function (currentCell) {
                var rightMenu = {
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
                                    //currentCell.blockLeftMenu = true;
                                    currentCell.price = helperService.clone(currentCell.originalPrice);
                                    currentCell.price.active = false;

                                    currentCell.rightMenu = getRightMenu(currentCell);
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

                                }, function () {

                                });
                            }
                        }
                    }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            var editedValue = fields[0].value;
                            cell.deleted = false;
                            cell.rightMenu = getRightMenu(cell);
                            prepareCellAndNeighbours(cell, row, editedValue);
                            //cell.blockLeftMenu = false;
                            cell.leftMenu = getLeftMenuWithDefaultMessage(cell);
                        },
                        reverseEdit: function (fields, cell, row, table) {
                            var currency = getSelectedCurrency();
                            var consolidation = getSelectedConsolidation();
                            cell.price = helperService.clone(cell.originalPrice);
                            cell.price.value = priceService.getPriceValue(cell.price, currency, consolidation);
                            cell.edited = false;
                            cell.deleted = false;
                            cell.rightMenu = getRightMenu(cell);
                            prepareCellAndNeighbours(cell, row, cell.price.value);
                        }
                    }
                };

                rightMenu.properties.showReverseEdit = (currentCell.edited || currentCell.deleted) ? true : false;
                rightMenu.properties.showApply = (!currentCell.deleted) ? true : false;
                rightMenu.fields[0].show = (!currentCell.deleted) ? true : false;
                rightMenu.fields[1].show = (!currentCell.deleted && currentCell.price && currentCell.price.value && currentCell.originalPrice.value) ? true : false;

                return rightMenu;
            };

            var prepareCellAndNeighbours = function (cell, row, value) {
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

            var calculateVariation = function (currentValue, previousValue) {
                return previousValue != undefined && previousValue != 0 ? (currentValue - previousValue) / previousValue : 0;
            };

            var getSelectedCountriesIds = function () {
                var ids = [];

                countryFilter.properties.items.forEach(function (region) {
                    region.items.forEach(function (country) {
                        if (country.value) {
                            ids.push(country.id);
                        };
                    });
                });

                return ids;
            };

            var getSelectedProductId = function () {
                return _.find(productFilter.properties.items, function (item) { return item.selected; }).id;
            }

            var getSelectedProductsId = function () {
                var products = [];
                for (var i = 0; i < productFilter.properties.items.length; i++) {
                    if (productFilter.properties.items[i].selected)
                        products.push(productFilter.properties.items[i]);
                }
                return products;
            }

            var prepareProductsForFilter = function (productsResponse, includeAll) {
                var products = [];

                if (includeAll) {
                    productsResponse.push({
                        id: 0,
                        name: "All products",
                        shortName: "All products"
                    });
                }
                productsResponse.forEach(function (p, i) {
                    products.push({
                        id: p.id,
                        text: p.name,
                        textShort: p.shortname,
                        selected: (isLaunch && p.id == 15) || (!isLaunch && (
                            ($scope.lastSettingsChosen.defaultProductId && $scope.lastSettingsChosen.defaultProductId == p.id)
                            || (!$scope.lastSettingsChosen.defaultProductId && i == 0))) ? true : false
                    });
                });
                if (isLaunch) {
                    userService.lastSettingsChosen.defaultProductId = 15;
                }
                return products;
            };

        }]);
});