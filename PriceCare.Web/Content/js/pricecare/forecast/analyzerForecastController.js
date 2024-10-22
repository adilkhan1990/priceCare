define(['pricecare/forecast/module'], function (module) {
    'use strict';
    module.controller('AnalyzerForecastController', [
        '$scope', '$q', 'helperService', 'priceService', 'productService', 'countryService', 'priceTypeService',
        '$http', 'dimensionService', 'userService', '$modal', 'simulationService', 'analyzerService', 'RowFactory',
        'excelService', 'formatHelperService',
        function ($scope, $q, helperService, priceService, productService, countryService, priceTypeService,
            $http, dimensionService, userService, $modal, simulationService, analyzerService, RowFactory,
            excelService, formatHelperService) {
            $scope.showBadFilter = false;
            var countryPromise;
            var currencyFilter = helperService.getCurrencyFilter();
            var priceTypePromise = priceTypeService.getAllPriceTypes(0);
            var eventTypePromise = dimensionService.getEventTypes();
            var eventTypes;
            var data = {};
            var selectedCountries = [];
            var countries = [];
            var selectedEvents = [];
            var referencedtext = 'Referenced';
            var impactedText = 'impacted';

            var simulationFilter = {
                type: helperService.FieldTypes.SelectMultiLevel,
                name: 'simulations',
                properties: {
                    class: 't-custom-select-text',
                    items: []
                }
            };

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
            var buttonDownloadSalesImpactExcel = {
                type: helperService.FieldTypes.Action,
                hide:true,
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
                                        showAllEventsOption: true
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
                                events: getSelectedEvents(),
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
            var dataTypeFilter = {
                type: helperService.FieldTypes.Select,
                name: 'analyzeType',
                properties: {
                    class: 't-custom-select-text',
                    items: [
                        { id: 41, text: 'Prices', selected: true },
                        { id: 42, text: 'Volumes', selected: false },
                        { id: 51, text: 'Net prices', selected: false },
                        { id: 52, text: 'Sales', selected: false },
                        { id: 999, text: 'Sales impact', selected: false }
                    ]
                }
            };
            userService.getLastSettingsChosen().then(function (result) {
                $scope.lastSettingsChosen = result;
                getSimulations();
                getCountriesAndProducts();
            });

            var getSimulations = function () {
                simulationService.getSimulations().then(function (result) {
                    simulationFilter.properties.items = simulationService.prepareSimulationForFilter(result);
                });
            };

            var loadCacheAndReloadData = function () {
                var saveId = getSelectedSaveId();
                if (saveId && saveId != 0) {
                    simulationService.loadSimulation(saveId, getSelectedProductId()).then(function (data) {
                        $scope.simulationId = data.id;
                        getData();
                    });
                } else {
                    $scope.table = null;
                }

            };

            var getSelectedSaveId = function () {
                for (var i = 0; i < simulationFilter.properties.items.length; i++) {
                    var save = _.find(simulationFilter.properties.items[i].items, function (saveTmp) {
                        return saveTmp.selected == true;
                    });
                    if (save != null) break;
                }

                return (save == null) ? 0 : save.id;
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
                    if (product.id == productId)
                    {
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

            var displaySecondaryImpactsFilter = {
                type: helperService.FieldTypes.Checkbox,
                text: 'Show secondary impacts',
                value: false,
                name: 'displaySecondaryImpacts'
            };

            var displayEffectivelyImpactedCountriesFilter = {
                type: helperService.FieldTypes.Checkbox,
                text: 'Only show effectively impacted',
                value: true,
                name: 'displayEffectivelyImpactedCountries'
            };

            var displayReferencingDataFilter = {
                type: helperService.FieldTypes.QuickSwitch,
                name: 'displayReferencingData',
                properties: {
                    label: "Show",
                    class: 't-custom-select-boxed',
                    items: [
                        { text: impactedText, selected: true },
                        { text: referencedtext }
                    ]
                }
            };

            $scope.filterOptions = {
                header: {
                    items: [
                        dataTypeFilter,
                        {
                            type: helperService.FieldTypes.Label,
                            properties:
                            {
                                text: "for"
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
                        buttonDownloadSalesImpactExcel
                    ],
                    displayOptions: [
                        displayReferencingDataFilter,
                        displaySecondaryImpactsFilter,
                        displayEffectivelyImpactedCountriesFilter
                    ],
                },
                showAdvancedFilters: false,
                onChanged: function (sourceChanged) {
                    if (sourceChanged == dataTypeFilter) {
                        currencyFilter.hide = getSelectedFilterItem(dataTypeFilter).id == 42;
                        displayEffectivelyImpactedCountriesFilter.hide = getSelectedFilterItem(dataTypeFilter).id != 999
                    }
                    if (sourceChanged == currencyFilter || sourceChanged == consolidationFilter || sourceChanged == displayReferencingDataFilter || sourceChanged == displaySecondaryImpactsFilter || sourceChanged == displayEffectivelyImpactedCountriesFilter) {
                        if (sourceChanged == displayReferencingDataFilter) {
                            var referenced = getSelectedDisplayType() == referencedtext;
                            displayEffectivelyImpactedCountriesFilter.hide = referenced;
                            displaySecondaryImpactsFilter.hide = referenced;
                        }
                        drawData();
                    } else if (sourceChanged == productFilter) {

                        var productId = getSelectedProductId();
                        if (productId != 0)
                            userService.lastSettingsChosen.defaultProductId = productId;
                        
                        getConsolidations(getSelectedProductId());
                    } else if (sourceChanged == simulationFilter) {
                        if (getSelectedSimulationOptionItem().isLaunch)
                            changeSelectedProductId(15);
                        loadCacheAndReloadData();
                    } else {
                        getData();
                    }
                }
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
            };

            var setSelectedFilterItem = function (filter, filterItemId) {
                _.each(filter.properties.items, function (item) {
                    item.selected = item.id == filterItemId;
                });
            };

            

            var getCountriesAndProducts = function () {
                countryPromise = countryService.getAllCountries();
                var productPromise = productService.getAllProducts();

                $q.all([countryPromise, productPromise]).then(function (data) {
                    countries = data[0];
                    var firstCountry;
                    if (userService.lastSettingsChosen.defaultCountryId == 0) {
                        firstCountry = countries[0];
                    } else {
                        firstCountry = _.find(countries, function (country) {
                            return country.id == userService.lastSettingsChosen.defaultCountryId;
                        });
                    }
                    selectedCountries.push(firstCountry);

                    productFilter.properties.items = getProductsForFilter(data[1], false);

                    getConsolidations(getSelectedProductId());
                });
            };

            var getConsolidations = function (productId) {
                var consolidationPromise = $http.get('api/consolidation/' + productId);

                consolidationPromise.then(function (data) {
                    consolidationFilter.properties.items = getConsolidationsForFilter(data.data);
                    simulationService.getFirstSimulation().then(function (result) {
                        if (result != "null") {
                            changeSelectedSaveId(result.saveId);
                            $scope.simulationId = result.id;
                            getData();
                        } else {
                            loadCacheAndReloadData();
                        }
                    });
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

            var getConsolidationsForFilter = function (data) {
                data.forEach(function (consolidation) {
                    if (consolidation.isDefault) {
                        consolidation.selected = true;
                    }
                    consolidation.text = consolidation.factorScreen + " " + consolidation.name;
                });

                return data;
            };

            var addCountry = function () {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/addCountryModal.html',
                    controller: 'AddCountryModalController',
                    backdrop: 'static'
                });
                modalInstance.result.then(function (country) {
                    selectedCountries.push(_.find(countries, function (c) {
                        return country.id == c.id;
                    }));
                    getData();
                });
            };

            var removeCountry = function (countryId) {
                var selectedCountry = _.find(selectedCountries, function (c) {
                    return countryId == c.id;
                });
                selectedCountries.splice(selectedCountries.indexOf(selectedCountry), 1);

                getData();
            };

            var getData = function () {
                $scope.showBadFilter = false;
                var pricePromise;
                if (getSelectedDataTypeId() == 999) {
                    displayEffectivelyImpactedCountriesFilter.hide = false;
                    pricePromise = analyzerService.GetSalesImpact($scope.simulationId, getSelectedCountriesIds(), getSelectedProductId(), getSelectedDataTypeId(), getSelectedEvents());
                } else {
                    displayEffectivelyImpactedCountriesFilter.hide = true;
                    pricePromise = analyzerService.getAnalyzerData($scope.simulationId, getSelectedCountriesIds(), getSelectedProductId(), getSelectedDataTypeId());
                }
                $q.all([pricePromise, countryPromise, priceTypePromise, eventTypePromise]).then(function (getDataResult) {
                    data = getDataResult;
                    drawData();

                });
            };

            var isSalesImpactSelected = function () {
                return getSelectedDataTypeId() == 999;
            }

            var formatDate = function (date) {
                if (isSalesImpactSelected()) {
                    return helperService.formatDateYear(date);
                }
                return helperService.formatDateMonthYear(date);
            }
            var drawData = function () {
                if (data) {

                    var prices = data[0];
                    var regionsAndCountries = data[1];
                    $scope.priceTypes = data[2];
                    eventTypes = data[3];


                    if (prices.focus.length == 0) {

                        $scope.table = null;
                        $scope.showBadFilter = true;
                        return;
                    }

                    var allDates = _.map(prices.focus, function (item) { return item.dataTime; });
                    var uniqDates = _.uniq(allDates);
                    var dateLabels = [];

                    var now = formatDate(new Date());

                    var indiceNow = -1;
                    var dateCpt = 0;
                    uniqDates.forEach(function (d) {
                        var labelDate = formatDate(d);
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

                    var rows = [];
                    var colspan = dateLabels.length + 1;
                    if (colspan > 13) {
                        colspan = 13;
                    }
                    var rowFactory = new RowFactory(dateLabels, regionsAndCountries, $scope.priceTypes, {
                        getRightMenu: getRightMenu,
                        setCellDisplay: setCellDisplayLimited,
                        getLeftMenu: getLeftMenu,
                        useOnlyReviewedCells: getSelectedDataTypeId() == 41
                    });

                    rows.push(rowFactory.createTitleRow());
                    var countriesOfInterestRow = rowFactory.createHeaderRow("Countries of interest", colspan);
                    countriesOfInterestRow.cells[0].actions = [];
                    countriesOfInterestRow.cells[0].actions.push({
                        //text: 'Add country',
                        class: 'icon icon-add icon-in-row',
                        click: function (cell, row) {
                            addCountry();
                        }
                    });
                    rows.push(countriesOfInterestRow);
                    var focusRows = rowFactory.createPriceRows(prices.focus);
                    _.each(focusRows, function (row) {
                        if (focusRows.length > 1) {
                            row.cells[0].actions = [
                            {
                                text: 'Remove country',
                                class: 'icon icon-remove',
                                click: function (cell, row) {
                                    removeCountry(cell.country);
                                }
                            }
                            ];
                        }
                        rows.push(row);
                    });


                    if (getSelectedDisplayType() == impactedText) {

                        rows.push(rowFactory.createHeaderRow("Impacted countries", colspan));
                        _.each(rowFactory.createPriceRows(prices.impactedOrder1), function (row) {
                            if (displayEffectivelyImpactedCountriesFilter.value)
                            {
                                var emptyCells = _.every(_.filter(row.cells, function (c) { return c.price != null; }), function (c) {
                                    return c.price.value == 0;
                                });
                                if(!emptyCells)
                                    rows.push(row);
                                
                            }
                            else {
                                rows.push(row);
                            }
                            
                        });

                        if (displaySecondaryImpactsFilter.value) {
                            rows.push(rowFactory.createHeaderRow("Secondary impacted countries", colspan));
                            _.each(rowFactory.createPriceRows(prices.impactedOrderN), function (row) {
                                if (displayEffectivelyImpactedCountriesFilter.value) {
                                    var emptyCells = _.every(_.filter(row.cells, function (c) { return c.price != null; }), function (c) {
                                        return c.price.value == 0;
                                    });
                                    if (!emptyCells)
                                        rows.push(row);

                                }
                                else {
                                    rows.push(row);
                                }
                            });
                        }

                    } else {
                        rows.push(rowFactory.createHeaderRow("Referenced countries", colspan));
                        _.each(rowFactory.createPriceRows(prices.referenced), function (row) { rows.push(row); });

                    }

                    var table = {
                        columnFilteringOptions: getColumnFilteringOptions(dateLabels, indiceNow),
                        rows: rows
                    };
                    $scope.table = table;
                }
            }

            var getColumnFilteringOptions = function (columns, indiceNow) {
                if (columns.length > 12) {
                    return {
                        offset: indiceNow,
                        count: 12,
                        fixed: 1,
                    }
                }
                return null;
            }
            var setCellDisplay = function (cell, currency, consolidation) {
                if (cell.title) {
                    return;
                }

                cell.blockLeftMenu = true;
                if (cell.price && cell.price.value) {
                    cell.empty = false;
                    cell.background = '';

                    // Background event type
                    if (cell.price.eventTypeId) {
                        var eventType = getEventType(cell.price.eventTypeId);
                        if (!cell.background && eventType) {
                            cell.background = eventType.colorCode;
                            if (cell.background != '#ffffff') { // TODO : find another way to filter it out
                                cell.blockLeftMenu = false;
                            }
                        }
                    }

                    cell.value = getDisplayValue(cell.price, currency, consolidation);
                    if (getSelectedDataTypeId() === 42 || getSelectedDataTypeId() === 52 || getSelectedDataTypeId() === 999) {
                        cell.format = formatHelperService.formatNumber;
                    } else {
                        cell.format = helperService.formatNumber;
                    }
                } else {
                    cell.empty = false;
                }
                cell.blockRightMenu = true;
            };

            var getEventType = function (eventTypeId) {
                return _.find(eventTypes, function (item) {
                    return eventTypeId == item.id;
                });
            }

            var getDisplayValue = function (price, currency, consolidation) {
                if (getSelectedDataTypeId() === 42) {
                    return price.value / (consolidation.factorScreen * consolidation.factor);

                } else {
                    return priceService.getPriceValue(price, currency,
                        getSelectedDataTypeId() == 999 || getSelectedDataTypeId() == 52 ? null : consolidation);
                }
            }
            var setCellDisplayLimited = function (cell) {
                setCellDisplay(cell, getSelectedCurrency().id, getSelectedConsolidation());
            }

            var getLeftMenu = function (cell) {
                var result = {
                    content: "No info",
                };

                if (cell.price) {
                    var description = cell.price.description ? cell.price.description : "";
                    result.callback = viewSalesImpact;
                    result.cell = cell;
                    result.content = '<div>' + description + '<br/><a href="" ng-click="options.callback(options.cell)">View sales impact</a></div>';
                }

                return result;
            };
            var viewSalesImpact = function (cell) {
                buttonDownloadSalesImpactExcel.hide = false;
                addEventToSelectedEvents(cell.price);
                setSelectedFilterItem(dataTypeFilter, 999);
                getData();
            }
            var getRightMenu = function (currentCell) {
                return null;
            };

            var getSelectedCountriesIds = function () {
                return _.map(selectedCountries, function (country) {
                    return country.id;
                });
            };

            var getSelectedEvents = function () {
                return selectedEvents;
            };

            var addEventToSelectedEvents = function (event) {
                selectedEvents = [event];
            }

            var getSelectedProductId = function () {
                return _.find(productFilter.properties.items, function (item) { return item.selected; }).id;
            }

            var getSelectedDataTypeId = function () {
                return _.find(dataTypeFilter.properties.items, function (item) { return item.selected; }).id;
            }
            var getSelectedDisplayType = function () {
                return _.find(displayReferencingDataFilter.properties.items, function (item) { return item.selected; }).text;
            };

            var getProductsForFilter = function (productsResponse, includeAll) {
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
                        selected: ($scope.lastSettingsChosen.defaultProductId && $scope.lastSettingsChosen.defaultProductId == p.id)
                            || (!$scope.lastSettingsChosen.defaultProductId && i == 0) ? true : false
                    });
                });

                return products;
            };
        }
    ]);
});