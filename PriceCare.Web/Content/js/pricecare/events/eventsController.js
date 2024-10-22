define(['pricecare/events/module'], function (module) {
    'use strict';
    module.controller('EventController', [
        '$scope', '$rootScope', '$controller', '$q', 'helperService', 'productService', 'countryService', 'dimensionService', 'versionService', 'eventService', '$modal', 'userService', 'validate', '$routeParams', 'loadService', '$location',
        'simulationService', 'tableService', 'excelService',
    function ($scope, $rootScope, $controller, $q, helperService, productService, countryService, dimensionService, versionService, eventService, $modal, userService, validate, $routeParams, loadService, $location,
        simulationService, tableService, excelService) {
        
        $scope.loadId = $routeParams.loadId;
        $scope.cellsToValidate = 0;

        var usableProductsAndGeographyIds;
        var countryPromise;
        $scope.validate = validate;

        $scope.saveVersion = function () {
            if ($scope.table && $scope.table.rows && $scope.canSave) {
                var editedCells = [];

                $scope.table.rows.forEach(function(row) {
                    for (var i = 0; i < row.cells.length; i++) {
                        if (row.cells[i].edited)
                            editedCells.push(row.cells[i].event);
                    }
                });
                if (editedCells.length > 0) {

                    eventService.saveVersion(editedCells).then(function(data) {
                        $scope.canSave = false;
                        getVersionsAndEvents(getSelectedCountriesId(), getSelectedProductsId());

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
                if (validate && $scope.table == null && $scope.canSave) {
                    validateLoadItemDetail();
                }
            }
        };

        var validateLoadItemDetail = function() {
            loadService.validateLoadItemDetail($routeParams.loadId, helperService.LoadItemNames.Event, getSelectedProductsId()[0], getSelectedCountriesId()).then(function(result) {
                if (result.url)
                    $location.path(result.url);
                else
                    init();
            });
        };

        $scope.searchRequest = {
            geographyId: [],
            productId: [],
            eventTypeId: [],
            versionId: 0,
            validate: validate
        };

        $scope.smartDeleteResolutionOptions = {
            predescription: 'cells differ between old and new',
            actionText: 'Keep Old',
            actionRejectText: 'Take New',
            displayReject: true,
            hideToggle: true,
            affectedCells: 0,
            callback: function () {
                calculateDeleteSmartResolution();
            },
            apply: function () {
                applyKeepSmartResolution();
            },
            reject: function () {
                applyRejectSmartResolution();
            }
        };

        var calculateDeleteSmartResolution = function () {
            $scope.cellsToValidate = 0;
            tableService.foreachCells($scope.table, function (cell) {
                if (cell.event && cell.event.tag && !cell.edited) {
                    $scope.cellsToValidate++;
                }
            });

            $scope.smartDeleteResolutionOptions.affectedCells = $scope.cellsToValidate;

        };

        var applyKeepSmartResolution = function () {
            tableService.foreachCells($scope.table, function (cell, row) {
                if (cell.event && cell.event.tag && !cell.edited) {
                    rejectLoadOnCell(cell, row);
                }
            });

            calculateDeleteSmartResolution();
        };

        var applyRejectSmartResolution = function () {
            tableService.foreachCells($scope.table, function (cell, row) {
                if (cell.event && cell.event.tag && !cell.edited) {
                    applyLoadOnCell(cell, row);
                }
            });

            calculateDeleteSmartResolution();
        };


        var countryFilter = {
            type: helperService.FieldTypes.DynamicPop,
            name: 'countries',
            properties:
            {
                class: 't-custom-select-boxed',
                directive: 'op-selection-tree-popup-limited',
                items: [],
                getText: function() {
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
            name: 'products',
            properties: {
                class: 't-custom-select-text',
                items:[]
            }
        };

        var versionFilter = {
            type: helperService.FieldTypes.Select,
            properties: {
                class: 't-custom-select-text',
                items:[]

            }
        };

        $scope.firstVersionSelected = function () {
            if (versionFilter.properties.items && versionFilter.properties.items.length > 0) {
                return versionFilter.properties.items[0].selected == true;
            }

            return true;
        }

        var eventTypeFilter = {
            type: helperService.FieldTypes.Select,
            name: 'eventTypes',
            properties: {
                class: 't-custom-select-boxed',
                items:[]
            }
        };

        var scenarioFilter = {
            type: helperService.FieldTypes.Select,
            name: 'scenarios',
            properties: {
                class: 't-custom-select-boxed',
                items: []
            }
        };

        var scenarioAddButton = {
            type: helperService.FieldTypes.Action,
            properties: {
                text: '.', // The design is correct only if there is a text to display, '.' has no meaning
                class: 'button button-icon button-icon-only icon-add button-blue float-right tooltip',
                tooltip: 'Add Scenario',
                callback: function() {
                    var modalInstance = $modal.open({
                        templateUrl: 'Content/js/pricecare/modal/addScenarioModal.html',
                        controller: 'AddScenarioModalController',
                        backdrop: 'static'
                    });
                    modalInstance.result.then(function (saveInfo) {
                        simulationService.createAssumptionsScenario(saveInfo).then(function (result) {
                            init();
                        });
                    });
                }
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
                    productFilter,
                    versionFilter
                ],
                primaryDisplayOptions: [],
            },
            filters: [
                countryFilter,
                eventTypeFilter,
                scenarioFilter,
                scenarioAddButton
            ],
            showAdvancedFilters: false,
            onChanged: function (sourceChanged) {
                if (sourceChanged == countryFilter || sourceChanged == productFilter || sourceChanged == eventTypeFilter) {
                    if (sourceChanged == countryFilter) {
                        if (validate) {
                            getVersionsAndEvents(getSelectedCountriesId(), getSelectedProductsId());
                        }
                        userService.lastSettingsChosen.defaultRegionId = getSelectedRegionId();
                    } 
                    else if (sourceChanged == productFilter) {
                        if (validate) {
                            countryPromise.then(function(result) {
                                countryFilter.properties.items = prepareRegionsForFilter(result);
                                filterGeographyForProduct();

                                getVersionsAndEvents(getSelectedCountriesId(), getSelectedProductsId());
                            });
                        }
                        userService.lastSettingsChosen.defaultProductId = getSelectedProductsId()[0];
                    }
                    if (!validate) {
                        getVersionsAndEvents(getSelectedCountriesId(), getSelectedProductsId());
                    }
                } else {
                    getData();
                }
            }
        };

        var getSelectedRegionId = function () {
            var region = _.find(countryFilter.properties.items, function (item) {
                return item.value != null;
            });
            return region.id;
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

        var filterGeographyForProduct = function () {
            var selectedProductId = getSelectedProductsId()[0];
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
            
            if (getSelectedCountriesId().length == 0) {
                countryFilter.properties.items[0].value = true;
                countryFilter.properties.items[0].items.forEach(function (item) {
                    item.value = true;
                });
            }
        };

        var prepareProductsForFilter = function (productsResponse) {
            var products = [];

            if (validate) {
                productsResponse.forEach(function (p, i) {
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
            _.each(eventTypeFilter.properties.items, function (eventType) {
                if (eventType.selected)
                    eventTypesId.push(eventType.id);
            });
            if (eventTypesId[0] == 0) // send all ids
                return _.pluck(eventTypeFilter.properties.items, "id");
            return eventTypesId;
        };

        var getSelectedProductsId = function () {
            var products = [];
            _.each(productFilter.properties.items, function (product) {
                if (product.selected == true)
                    products.push(product);
            });
            return _.pluck(products, "id");
        };

        var getSelectedVersion = function () {
            var version = _.find(versionFilter.properties.items, function (v) {
                return v.selected == true;
            });
            return version;
        };

        var getCountriesAndProducts = function () {
            var eventTypePromise = dimensionService.getEventTypes();
            countryPromise = countryService.getRegionsAndCountries();
            var productPromise = productService.getAllProducts();

            var promiseArray = [countryPromise, productPromise, eventTypePromise];

            if (validate) {
                var next = loadService.getNext($routeParams.loadId, helperService.LoadItemNames.Event);
                promiseArray.push(next);
            }

            $q.all(promiseArray).then(function (data) {

                if (validate) {
                    usableProductsAndGeographyIds = data[3];
                }

                productFilter.properties.items = prepareProductsForFilter(data[1]);
                countryFilter.properties.items = prepareRegionsForFilter(data[0]);

                if (validate) {
                    filterGeographyForProduct();
                }

                $scope.eventTypes = data[2];
                eventTypeFilter.properties.items = prepareEventTypesForFilter(data[2]);

                getVersionsAndEvents(getSelectedCountriesId(), getSelectedProductsId());
            });
        };

        var getVersionsAndEvents = function (countryIds, productId) {
            $scope.searchRequest.geographyId = countryIds;
            $scope.searchRequest.productId = productId;
            $scope.searchRequest.eventTypeId = getSelectedEventTypes();
            var versionPromise = versionService.getEventTypeVersions($scope.searchRequest);
            var userMappingPromise = userService.getUserMapping();

            $q.all([versionPromise,userMappingPromise]).then(function (data) {
                versionFilter.properties.items = versionService.getVersionsForFilter(data[0], data[1]);
                var version = getSelectedVersion();
                if(version)
                    $scope.searchRequest.versionId = version.versionId;
                getData();
            });
        };

        var getRegionAndCountriesSelected = function() {
            var result = [];

            _.forEach(countryFilter.properties.items, function(region) {
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
            $scope.searchRequest.versionId = null;
            $scope.searchRequest.scenarioTypeId = getSelectedScenario();
            var version = getSelectedVersion();
            if (version)
                $scope.searchRequest.versionId = version.versionId;
            var eventPromise = eventService.getEvents($scope.searchRequest);

            $q.all([eventPromise]).then(function (data) {
                if (data[0].length == 0) {
                    $scope.table = null;
                    calculateDeleteSmartResolution();
                    if (validate) {
                        $scope.canSave = true;
                    }
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
                    indiceNow = 0;//dateLabels.length - 12; // if the actual date is not found, we take the last 12 months

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

                                newCell.rightMenu = getRightMenu(newCell, eventTypeRow);

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

                if (!validate) {
                    var exportToExcelFilter = {
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
                                    $scope.searchRequest.dataTypeId = 43;
                                    $scope.searchRequest.scenarioTypeId = getSelectedScenario();
                                    $scope.searchRequest.saveId = getSelectedScenario();
                                    excelService.postFilterExcel($scope.searchRequest).then(function (data) {
                                        window.location.href = 'api/event/excel?token=' + data.token;
                                    });
                                }, function () {

                                });
                            }
                        }
                    };
                    $scope.filterOptions.header.primaryDisplayOptions = [exportToExcelFilter];                                      
                }

                calculateDeleteSmartResolution();
            });

        };

        var setCellDisplay = function (cell) {
            if (cell.title) {
                return;
            }

            if (validate) {
                if ((!cell.event.tag == "" || !cell.event.tag == undefined) && !cell.edited) {
                    cell.styleLoad = {
                        border: helperService.borderAll,
                        background: cell.event.tag == helperService.LoadTag.Deleted
                    };
                }

            }

            cell.empty = (cell.event && cell.event.value != null && !cell.deleted) ? false : true;

            if (!cell.empty) {
                // Border
                if (cell.event.eventTypeId) {
                    var eventType = _.find($scope.eventTypes, function(item) {
                        return cell.event.eventTypeId == item.id;
                    });
                    if (eventType) {
                        cell.background = eventType.colorCode;
                        //cell.border = eventType.colorCode;
                    }
                }

                cell.value = cell.event.value;
            } else {
                cell.background = null;
                //cell.border = null;
                cell.value = null;
            }
            
            var eventTypeDefaultSelected = (!cell.empty) ? cell.event.eventTypeId : _.first($scope.eventTypes).id;
            cell.format = eventTypeDefaultSelected != dimensionService.eventTypes.launchWithTargetPrice ? helperService.formatPercentage : helperService.formatNumber;
            if (!validate || cell.event.tag == '' || cell.event.tag == undefined || cell.edited) {
                cell.toggleEventType = cell.event.eventTypeId != dimensionService.eventTypes.launchWithTargetPrice;
                cell.format = cell.toggleEventType ? helperService.formatPercentage : helperService.formatNumber;
                cell.rightMenu.fields[0].properties.items = prepareEventTypesForFilterWithoutAllEventsByDefault($scope.eventTypes, eventTypeDefaultSelected);
                cell.rightMenu.fields[1].cellValue = (!cell.empty) ? cell.value * (cell.toggleEventType ? 100.0 : 1) : 0.0; // add value to right menu
                cell.rightMenu.fields[2].cellValue = (!cell.empty) ? cell.event.description : "";
            }
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

        var getRightMenu = function (currentCell, row) {
            var rightMenu;
            if (currentCell.event == undefined)
                return;

            if (!validate || currentCell.event.tag == '' || currentCell.event.tag == undefined || currentCell.edited) {
                rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Select,
                            text: "Event type",
                            properties: {
                                class: 't-custom-select-boxed',
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
                                areEquals: function(x, y) {
                                    return Math.abs((x - y) / (x + y) * 2) < helperService.EqualityThreshold;
                                },
                                parse: function(x) {
                                    return parseFloat(x);
                                }
                            }
                        },
                        {
                            type: helperService.FieldTypes.Textbox,
                            text: "Description",
                            properties: {
                                required: false,
                                areEquals: function(x, y) {
                                    return x == y;
                                },
                            }
                        },
                        {
                            type: helperService.FieldTypes.Action,
                            properties:
                            {
                                text: 'Delete',
                                class: 'button button-border button-red button-icon icon-delete text-align-center',
                                callback: function() {
                                    var modalInstance = $modal.open({
                                        templateUrl: 'Content/js/pricecare/modal/confirmationModal.html',
                                        controller: 'ConfirmationModalController',
                                        backdrop: 'static',
                                        resolve: {
                                            infos: function() {
                                                return {
                                                    title: 'Remove Event',
                                                    content: 'Are you sure you want to delete this event ?'
                                                }
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function() {
                                        currentCell.event = helperService.clone(currentCell.originalEvent);
                                        currentCell.event.active = false;
                                        currentCell.edited = true;
                                        currentCell.deleted = true;
                                        currentCell.blockLeftMenu = true;
                                        currentCell.rightMenu = getRightMenu(currentCell, row);
                                        setCellDisplay(currentCell);
                                        if (!$scope.firstVersionSelected())
                                            currentCell.blockRightMenu = true;
                                        canSaveVersion();
                                    }, function() {

                                    });
                                }
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell, row, table) {
                            var eventSelected = _.find(fields[0].properties.items, function(item) {
                                return item.selected == true;
                            });
                            cell.event.eventTypeId = eventSelected.id;

                            cell.toggleEventType = cell.event.eventTypeId != dimensionService.eventTypes.launchWithTargetPrice;
                            cell.format = cell.toggleEventType ? helperService.formatPercentage : helperService.formatNumber;

                            cell.event.value = fields[1].value / (cell.toggleEventType ? 100.0 : 1);
                            cell.event.description = fields[2].value;
                            cell.event.active = true;
                            currentCell.deleted = false;
                            cell.rightMenu = getRightMenu(cell, row);
                            setCellDisplay(cell);
                            cell.leftMenu = getLeftMenu(cell);
                            $scope.canSave = true;
                        },
                        reverseEdit: function(fields, cell, row, table) {
                            cell.event = helperService.clone(cell.originalEvent);
                            cell.edited = false;
                            cell.deleted = false;
                            cell.rightMenu = getRightMenu(cell, row);
                            setCellDisplay(cell);
                            cell.leftMenu = getLeftMenu(cell);
                            canSaveVersion();
                        }
                    }
                };
                for (var i = 0; i < rightMenu.fields.length; i++) {
                    rightMenu.fields[i].show = (!currentCell.deleted) ? true : false;
                }
                rightMenu.fields[3].show = (!currentCell.deleted && currentCell.event && currentCell.event.value && currentCell.originalEvent.value) ? true : false;
                rightMenu.properties.showReverseEdit = (currentCell.edited || currentCell.deleted) ? true : false;
                rightMenu.properties.showApply = (!currentCell.deleted) ? true : false;
            } else {
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
                                    calculateDeleteSmartResolution();
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
                                    calculateDeleteSmartResolution();
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

                if (currentCell.event.tag == helperService.LoadTag.Edited && currentCell.event.oldValue && currentCell.event.oldValue.value) {
                    rightMenu.fields.unshift({
                        type: helperService.FieldTypes.Label,
                        properties: {
                            text: "OldValue: " + helperService.formatNumber(currentCell.event.oldValue.value)
                        }
                    });
                }
            }
            return rightMenu;
        };

        var canSaveVersion = function () {
            $scope.canSave = false;

            if (validate) {
                var cpt = 0;
                tableService.foreachCells($scope.table, function (cell) {
                    if (cell.event.tag != undefined && cell.event.tag != '') {
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

        // INITIALISATION
        var init = function () {
            var promises = [userService.getLastSettingsChosen(), simulationService.getAssumptionsScenarios()];
            
           $q.all(promises).then(function (datas) {
               $scope.lastSettingsChosen = datas[0];
               scenarioFilter.properties.items = prepareScenarios(datas[1]);
               getCountriesAndProducts();

            });
        };

        init();

        var prepareScenarios = function (datas) {
            var result = [];

            datas.forEach(function (data, i) {
                result.push({
                    id: data.saveId,
                    text: data.name,
                    selected: i == 0
                });
            });

            return result;
        };

        var getSelectedScenario = function() {
            var scenario = _.find(scenarioFilter.properties.items, function(item) {
                return item.selected;
            });
            return scenario != null ? scenario.id : -1;
        };

        var getApplyWholeRowValue = function (currentCell) {
            if (currentCell.rightMenu.fields[0].type == helperService.FieldTypes.Checkbox) {
                return currentCell.rightMenu.fields[0].value;
            }
            return currentCell.rightMenu.fields[1].value;
        };
        var applyLoad = function (currentCell, row) {
            currentCell.onClose();
            var applyWhole = getApplyWholeRowValue(currentCell);

            var currentTag = currentCell.event.tag;
            if (applyWhole === true) {
                row.cells.forEach(function (cell) {
                    if (cell.event && !cell.edited && cell.event.tag && cell.event.tag == currentTag) {
                        applyLoadOnCell(cell, row);
                    }
                });
            } else {
                applyLoadOnCell(currentCell, row);
            }

        };

        var applyLoadOnCell = function (currentCell, row) {
            switch (currentCell.event.tag) {
                case helperService.LoadTag.Loaded:
                    currentCell.styleLoad = null;
                    currentCell.edited = true;
                    currentCell.rightMenu = getRightMenu(currentCell, row);
                    setCellDisplay(currentCell, row);

                    break;
                case helperService.LoadTag.Deleted:
                    currentCell.styleLoad = null;
                    currentCell.edited = true;

                    currentCell.event.value = null;
                    currentCell.event.active = false;

                    currentCell.rightMenu = getRightMenu(currentCell, row);
                    setCellDisplay(currentCell, row);


                    break;
                case helperService.LoadTag.Edited:
                    currentCell.styleLoad = null;
                    currentCell.edited = true;
                    currentCell.rightMenu = getRightMenu(currentCell, row);
                    setCellDisplay(currentCell, row);

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
            var currentTag = currentCell.event.tag;
            if (applyWhole === true) {
                row.cells.forEach(function (cell) {
                    if (cell.event && !cell.edited && cell.event.tag && cell.event.tag == currentTag) {
                        rejectLoadOnCell(cell, row);
                    }
                });
            } else {
                rejectLoadOnCell(currentCell, row);
            }
        };

        var rejectLoadOnCell = function (currentCell, row) {

            switch (currentCell.event.tag) {
                case helperService.LoadTag.Loaded:
                    currentCell.styleLoad = null;
                    //currentCell.edited = true;
                    currentCell.event.tag = '';

                    currentCell.event.value = null;

                    currentCell.rightMenu = getRightMenu(currentCell, row);
                    setCellDisplay(currentCell, row);

                    break;
                case helperService.LoadTag.Deleted:
                    currentCell.styleLoad = null;
                    //currentCell.edited = true;
                    currentCell.event.tag = '';
                    //currentCell.event.value = currentCell.event.oldValue.value;

                    currentCell.rightMenu = getRightMenu(currentCell, row);
                    setCellDisplay(currentCell, row);


                    break;
                case helperService.LoadTag.Edited:
                    currentCell.styleLoad = null;
                    //currentCell.edited = true;
                    currentCell.event.tag = '';

                    currentCell.event.value = currentCell.event.oldValue.value;

                    currentCell.rightMenu = getRightMenu(currentCell, row);
                    setCellDisplay(currentCell, row);

                    currentCell.styleLoad = null;
                    break;
            }
            currentCell.ready = true;
            canSaveVersion();
        };
    }
    ]);
});
