define(['pricecare/forecast/module'], function (module) {
    'use strict';
    module.controller('EventsForecastController', [
        '$scope', '$rootScope', '$controller', '$q', 'helperService', 'productService', 'countryService', 'dimensionService', 'eventService', '$modal', 'userService',
        'simulationService', 'excelService',
    function ($scope, $rootScope, $controller, $q, helperService, productService, countryService, dimensionService, eventService, $modal, userService,
        simulationService, excelService) {

        $scope.searchRequest = {
            geographyId: [],
            productId: [],
            eventTypeId: [],
            dataTypeId: 43
        };

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
                                productId: getSelectedProductsId()[0],
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
                        editedCells.push(row.cells[i].event);
            });

            if (editedCells.length > 0) {
                var saveId = getSelectedSaveId();
                simulationService.updateSimulationCache({ updatedData: editedCells, simulationId: $scope.simulationId }).then(function (result) {
                    getFirstSimulation();
                    $scope.canSave = true;
                });
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

        var simulationFilter = {
            type: helperService.FieldTypes.SelectMultiLevel,
            name: 'simulations',
            properties: {
                class: 't-custom-select-text',
                items: []
            }
        };

        var productFilter = {
            type: helperService.FieldTypes.Select,
            name: 'products',
            properties: {
                class: 't-custom-select-text',
                items: []
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

        var eventTypeFilter = {
            type: helperService.FieldTypes.Select,
            name: 'eventTypes',
            properties: {
                class: 't-custom-select-boxed',
                items: []
            }
        };



        $scope.filterOptions = {
            header: {
                items: [
                    {
                        type: helperService.FieldTypes.Label,
                        properties: {
                            text: "Events for"
                        }
                    },
                    productFilter
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
                                                    };
                                                }
                                            }
                                        });

                                        modalInstance.result.then(function (response) {
                                            $scope.searchRequest.saveId = getSelectedSaveId();
                                            $scope.searchRequest.simulationId = $scope.simulationId;
                                            $scope.searchRequest.allCountries = response.allCountries;
                                            $scope.searchRequest.allProducts = response.allProducts;
                                            $scope.searchRequest.databaseLike = response.databaseLike;
                                            $scope.searchRequest.dataTypeId = 43;
                                            excelService.postFilterExcel($scope.searchRequest).then(function (data) {
                                                window.location.href = 'api/event/excelForecast?token=' + data.token;
                                            });
                                        }, function () {

                                        });
                                    }
                                }
                            }
                ],
            },
            filters: [
                countryFilter,
                eventTypeFilter
            ],
            showAdvancedFilters: false,
            onChanged: function (sourceChanged) {
                if (sourceChanged == simulationFilter) {
                    if (getSelectedSimulationOptionItem().isLaunch)
                        changeSelectedProductId(15);
                    loadCacheAndReloadData();
                } else {
                    getData();
                }
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

        var prepareRegionsForFilter = function (data) {
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
        };

        var prepareProductsForFilter = function (productsResponse) {
            var products = [];
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

        var prepareEventTypesForFilter = function (eventTypesReponse) {
            var eventTypesResult = [];
            eventTypesResult.push({
                id: 0,
                text: "All event types",
                textShort: "All event types",
                selected: true
            });

            eventTypesReponse.forEach(function (e, i) {
                eventTypesResult.push({
                    id: e.id,
                    text: e.name,
                    textShort: e.shortname
                });
            });

            return eventTypesResult;
        };

        var prepareEventTypesForFilterWithoutAllEventsByDefault = function (eventTypesReponse, defaultEvent) {
            var eventTypesResult = [];

            eventTypesReponse.forEach(function (e, i) {
                eventTypesResult.push({
                    id: e.id,
                    text: e.name,
                    textShort: e.shortname,
                    selected: (defaultEvent == e.id) ? true : false
                });
            });

            return eventTypesResult;
        };

        var getSelectedCountriesId = function () {
            var countriesId = [];
            for (var i = 0; i < $scope.filterOptions.filters[0].properties.items.length; i++) { // for each region

                _.each($scope.filterOptions.filters[0].properties.items[i].items, function (country) {
                    if (country.value)
                        countriesId.push(country.id);
                });
            }

            return countriesId;
        };

        var getSelectedEventTypes = function () {
            var eventTypesId = [];
            _.each($scope.filterOptions.filters[1].properties.items, function (eventType) {
                if (eventType.selected)
                    eventTypesId.push(eventType.id);
            });
            if (eventTypesId[0] == 0) // send all ids
                return _.pluck($scope.filterOptions.filters[1].properties.items, "id");
            return eventTypesId;
        };

        var getSelectedProductsId = function () {
            var products = [];
            _.each($scope.filterOptions.header.items[1].properties.items, function (product) {
                if (product.selected == true)
                    products.push(product);
            });
            return _.pluck(products, "id");
        };

        var getCountriesAndProducts = function () {
            var eventTypePromise = dimensionService.getEventTypes();
            var countryPromise = countryService.getRegionsAndCountries();
            var productPromise = productService.getAllProducts();
            var simulationPromise = simulationService.getSimulations();

            $q.all([countryPromise, productPromise, eventTypePromise, simulationPromise]).then(function (data) {
                countryFilter.properties.items = prepareRegionsForFilter(data[0]);
                productFilter.properties.items = prepareProductsForFilter(data[1]);
                $scope.eventTypes = data[2];
                eventTypeFilter.properties.items = prepareEventTypesForFilter(data[2]);
                simulationFilter.properties.items = simulationService.prepareSimulationForFilter(data[3]);

                if (!_.any($scope.filterOptions.header.items, function (item) {
                    return item == simulationFilter;
                })) {
                    $scope.filterOptions.header.items.push(simulationFilter);
                }


                getFirstSimulation();
            });
        };

        var getFirstSimulation = function () {
            simulationService.getFirstSimulation().then(function (result) {
                if (result.simulationId != -1) {
                    currentSimulation = result;
                    changeSelectedSaveId(result.saveId);
                    $scope.simulationId = result.id;
                    getData();
                } else {
                    var saveId = getSelectedSaveId();
                    if (saveId != null) {
                        getData();
                    }
                }
            });
        }

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
            var productId = getSelectedProductsId()[0];
            if (saveId && saveId != 0) {
                simulationService.loadSimulation(saveId, productId).then(function (data) {
                    currentSimulation = data;
                    $scope.simulationId = data.id;
                    getData();
                });
            } else {
                $scope.table = null;
            }
        };

        var getRegionAndCountriesSelected = function () {
            var result = [];

            _.forEach(countryFilter.properties.items, function (region) {
                if (region.value != undefined) {
                    var regionSelected = {
                        name: region.name,
                        id: region.id,
                        countries: []
                    };
                    regionSelected.countries = _.where(region.items, function (country) {
                        return country.value == true;
                    });
                    result.push(regionSelected);
                }
            });

            return result;
        };

        var getData = function () {
            $scope.searchRequest.geographyId = getSelectedCountriesId();
            $scope.searchRequest.productId = getSelectedProductsId();
            $scope.searchRequest.eventTypeId = getSelectedEventTypes();
            $scope.searchRequest.simulationId = $scope.simulationId;
            $scope.searchRequest.saveId = getSelectedSaveId();
            var eventPromise = eventService.getEventsForecast($scope.searchRequest);

            $q.all([eventPromise]).then(function (data) {
                if (data[0].length == 0) {
                    $scope.table = null;
                    return;
                }
                var regionAndCountries = getRegionAndCountriesSelected();
                // FORMAT DATE
                var allDates = _.map(data[0], function (item) { return item.dataTime; });

                var uniqDates = _.uniq(allDates);
                var dateLabels = [];
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

                // FILL Table 
                for (var regionId in regionAndCountries) {
                    var region = regionAndCountries[regionId];
                    var regionRows = [];

                    for (var countryId in region.countries) {

                        var country = region.countries[countryId];
                        var countryData = countryGroups[country.id];
                        if (countryData) {
                            var eventTypeRow = {
                                cells: [
                                    { value: country.name }
                                ]
                            };

                            for (var i = 0; i < dateLabels.length; i++) {
                                var event = countryData[i];

                                var newCell = {
                                    originalEvent: helperService.clone(event),
                                    event: event
                                };

                                newCell.rightMenu = getRightMenu(newCell);

                                setCellDisplay(newCell);

                                newCell.leftMenu = getLeftMenu(newCell);

                                eventTypeRow.cells.push(newCell);
                            }

                            regionRows.push(eventTypeRow);
                        }
                    }

                    // If there were any country rows added
                    if (regionRows.length >= 1) {
                        rows = rows.concat(regionRows);
                    }
                }

                // Table initialistion
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

        var setCellDisplay = function (cell) {
            if (cell.title) {
                return;
            }

            cell.empty = (cell.event && cell.event.value != null && !cell.deleted) ? false : true;

            if (!cell.empty) {
                // Border
                if (cell.event.eventTypeId) {
                    var eventType = _.find($scope.eventTypes, function (item) {
                        return cell.event.eventTypeId == item.id;
                    });
                    if (eventType) {
                        cell.background = eventType.colorCode;
                        cell.border = eventType.colorCode;
                    }
                }

                cell.value = cell.event.value;
            } else {
                cell.background = null;
                cell.border = null;
                cell.value = null;
            }
            cell.toggleEventType = cell.event.eventTypeId != dimensionService.eventTypes.launchWithTargetPrice;
            cell.format = cell.toggleEventType ? helperService.formatPercentage : helperService.formatNumber;
            var eventTypeDefaultSelected = (!cell.empty) ? cell.event.eventTypeId : _.first($scope.eventTypes).id;
            cell.rightMenu.fields[0].properties.items = prepareEventTypesForFilterWithoutAllEventsByDefault($scope.eventTypes, eventTypeDefaultSelected);
            cell.rightMenu.fields[1].cellValue = (!cell.empty) ? cell.value * (cell.toggleEventType ? 100.0 : 1) : 0.0; // add value to right menu
            cell.rightMenu.fields[2].cellValue = (!cell.empty) ? cell.event.description : "";
        };

        var getLeftMenu = function (currentCell) {
            var result = {
                content: "No info"
            };

            if (currentCell.event) {
                var eventType = _.find(eventTypeFilter.properties.items, function (event) { return event.id == currentCell.event.eventTypeId; });
                var description = currentCell.event.description ? currentCell.event.description : "";
                result.content = '<strong>Event type : ' + eventType.text + '</strong><br/><p>' + description + '</p>';
            }

            return result;
        };

        var getRightMenu = function (currentCell) {
            var rightMenu = {
                fields: [
                    {
                        type: helperService.FieldTypes.Select,
                        text: "Event type",
                        properties: {
                            class: 't-custom-select-boxed'
                        }
                    },
                    {
                        type: helperService.FieldTypes.NumericTextbox,
                        text: 'Value',
                        properties: {
                            required: true,
                            allowDecimal: true,
                            allowNegative: true,
                            focus: true,
                            select: true,
                            areEquals: function (x, y) {
                                return Math.abs((x - y) / (x + y) * 2) < helperService.EqualityThreshold;
                            },
                            parse: function (x) {
                                return parseFloat(x);
                            }
                        }
                    },
                    {
                        type: helperService.FieldTypes.Textbox,
                        text: 'Description',
                        properties: {
                            required: false,
                            areEquals: function (x, y) {
                                return x == y;
                            },
                        }
                    },
                    {
                        type: helperService.FieldTypes.Action,
                        properties:
                        {
                            text: 'Delete',
                            class: 'button button-border button-red button-icon icon-save text-align-center',
                            callback: function () {
                                var modalInstance = $modal.open({
                                    templateUrl: 'Content/js/pricecare/modal/confirmationModal.html',
                                    controller: 'ConfirmationModalController',
                                    backdrop: 'static',
                                    resolve: {
                                        infos: function () {
                                            return {
                                                title: 'Remove Event',
                                                content: 'Are you sure you want to delete this event ?'
                                            }
                                        }
                                    }
                                });

                                modalInstance.result.then(function () {
                                    currentCell.event = helperService.clone(currentCell.originalEvent);
                                    currentCell.event.active = false;
                                    currentCell.edited = true;
                                    currentCell.deleted = true;
                                    currentCell.blockLeftMenu = true;
                                    currentCell.rightMenu = getRightMenu(currentCell);
                                    setCellDisplay(currentCell);
                                }, function () {

                                });
                            }
                        }
                    }
                ],
                properties: {
                    apply: function (fields, cell, row, table) {
                        var eventSelected = _.find(fields[0].properties.items, function (item) {
                            return item.selected == true;
                        });
                        cell.event.eventTypeId = eventSelected.id;
                        cell.toggleEventType = cell.event.eventTypeId != dimensionService.eventTypes.launchWithTargetPrice;
                        cell.format = cell.toggleEventType ? helperService.formatPercentage : helperService.formatNumber;

                        cell.event.value = fields[1].value / (cell.toggleEventType ? 100 : 1);
                        cell.event.description = fields[2].value;
                        cell.event.active = true;
                        currentCell.deleted = false;
                        cell.rightMenu = getRightMenu(cell);
                        setCellDisplay(cell);
                        cell.leftMenu = getLeftMenu(cell);
                    },
                    reverseEdit: function (fields, cell, row, table) {
                        cell.event = helperService.clone(cell.originalEvent);
                        cell.edited = false;
                        cell.deleted = false;
                        cell.rightMenu = getRightMenu(cell);
                        setCellDisplay(cell);
                        cell.leftMenu = getLeftMenu(cell);
                    }
                }
            };
            for (var i = 0; i < rightMenu.fields.length; i++) {
                rightMenu.fields[i].show = (!currentCell.deleted) ? true : false;
            }
            rightMenu.fields[3].show = (!currentCell.deleted && currentCell.event && currentCell.event.value && currentCell.originalEvent.value) ? true : false;
            rightMenu.properties.showReverseEdit = (currentCell.edited || currentCell.deleted) ? true : false;
            rightMenu.properties.showApply = (!currentCell.deleted) ? true : false;

            return rightMenu;
        };

        // INITIALISATION
        var init = function () {
            var userInfoPromise = userService.getUserInfo();
            var userPromise = userService.getLastSettingsChosen();
            $q.all([userPromise, userInfoPromise]).then(function (datas) {
                $scope.lastSettingsChosen = datas[0];
                $scope.userService = userService;
                getCountriesAndProducts();
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

    }]);
});