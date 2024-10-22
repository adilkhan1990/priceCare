 define(['pricecare/countries/module'], function (module) {
    'use strict';
    module.controller('CountriesController', [
        '$scope', '$rootScope', 'helperService', 'countryService', 'currencyService', '$q', 'userService', 'validate', '$modal', '$routeParams', '$location',
        function ($scope, $rootScope, helperService, countryService, currencyService, $q, userService, validate, $modal, $routeParams, $location) {

            $scope.loadId = $routeParams.loadId;

            $scope.countries = {};
            $scope.showAddRegionForm = false;
            $scope.validate = validate;
            $scope.cellsToValidate = 0;

            var searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage,
                regionId: 0,
                validate: validate
            };

            var statusFilter = {
                type: helperService.FieldTypes.Select,
                name: 'status',
                properties: {
                    class: 't-custom-select-boxed',
                    items: [
                        { text: "All status", id: 2, selected: true },
                        { text: "Active", id: 1 },
                        { text: "Inactive", id: 0}
                    ]
                }
            }

            var countryPromise = countryService.getRegionsAndCountries();
            var currencyPromise = currencyService.getCurrenciesForFilter();
            var userPromise = userService.getLastSettingsChosen();

            var countryFilter = {
                type: helperService.FieldTypes.Select,
                hide: validate,
                name: 'regions',
                properties: {
                    class: 't-custom-select-text',
                }
            };

            var getRegionsCountries = function() {
                countryPromise = countryService.getRegionsAndCountries();

                $q.all([countryPromise]).then(function(data) {
                    $scope.regionsCountries = data[0];
                    $scope.regions = $scope.getRegionForFilter(data[0]);
                    countryFilter.properties.items = $scope.regions;

                    resetPaging();
                    $scope.getCountries();
                });
            };

            $q.all([countryPromise, currencyPromise, userPromise]).then(function (data) {
                $scope.lastSettingsChosen = data[2];
                $scope.regionsCountries = data[0];
                $scope.regions = $scope.getRegionForFilter(data[0]);
                
                data[1].shift(); //remove 'all currencies' option from filter
                $scope.currencies = data[1];

                $scope.filterOptions = {
                    header: {
                        items: [
                                {
                                    type: helperService.FieldTypes.Label,
                                    properties:
                                    {
                                        text: validate ? "List of countries" : "List of countries for"
                                    }
                                },
                                countryFilter
                        ]                        
                    },
                    filters: [
                           statusFilter
                    ],
                    onChanged: function (sourceChanged) {
                        if (sourceChanged == countryFilter) {
                            userService.lastSettingsChosen.defaultRegionId = getSelectedRegionId();
                        }
                        onSearch();
                    }
                };
                countryFilter.properties.items = $scope.regions;
                onSearch();

            });

            var initTable = function () {                
                var columnNames = ["Country", "ISO2", "ISO3", "Export Name", "Default Currency", "Status"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                for (var j = 0; j < $scope.countries.length; j++) {
                    var currentCountry = $scope.countries[j];
                    var nameCell = { value: currentCountry.name }
                    var iso2Cell = { value: currentCountry.iso2, ready: validate && currentCountry.tag == helperService.LoadTag.Empty };
                    var iso3Cell = { value: currentCountry.iso3 };
                    var exportNameCell = { value: currentCountry.exportName};
                    var displayCurrencyCell = { value: currentCountry.displayCurrency };
                    var statusCell = { value: currentCountry.active ? "Active" : "Inactive" };

                    var newRow = {
                        originalCountry: currentCountry,
                        country: helperService.clone(currentCountry),
                        cells: [nameCell, iso2Cell, iso3Cell, exportNameCell, displayCurrencyCell, statusCell]
                    };

                    // style the row
                    if (validate) {
                        if (!currentCountry.tag == "") {
                            $scope.cellsToValidate++;
                            nameCell.styleLoad = {
                                border: helperService.borderLeft,
                                background: currentCountry.tag == helperService.LoadTag.Deleted
                            };
                            iso2Cell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentCountry.tag == helperService.LoadTag.Deleted
                            };
                            iso3Cell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentCountry.tag == helperService.LoadTag.Deleted
                            };
                            exportNameCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentCountry.tag == helperService.LoadTag.Deleted
                            };
                            displayCurrencyCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentCountry.tag == helperService.LoadTag.Deleted
                            };
                            statusCell.styleLoad = {
                                border: helperService.borderRight,
                                background: currentCountry.tag == helperService.LoadTag.Deleted
                            };
                        }
                    }

                    // Enrich cells
                    if (!validate || currentCountry.tag == helperService.LoadTag.Empty) {
                        initRowCellsRightMenu(newRow, currentCountry);
                    } else {
                        nameCell.rightMenu = initRightMenuValidate(newRow, nameCell);
                        iso2Cell.rightMenu = initRightMenuValidate(newRow, iso2Cell);
                        iso3Cell.rightMenu = initRightMenuValidate(newRow, iso3Cell);
                        exportNameCell.rightMenu = initRightMenuValidate(newRow, exportNameCell);
                        displayCurrencyCell.rightMenu = initRightMenuValidate(newRow, displayCurrencyCell);
                        statusCell.rightMenu = initRightMenuValidate(newRow, statusCell);
                    }

                    rows.push(newRow);
                }

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getCountries();
                        },
                        counterText: $scope.counter
                    }
                };
                canSaveVersion();
            };

            var initRightMenuValidate = function (row, cell) {
                return {
                    hideTextHelp: true,
                    fields: [
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Accept",
                                class: 'button button-border button-green button-icon icon-save text-align-center',
                                callback: function () {
                                    switch (row.country.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.country.active = true;
                                            row.cells[5].value = "Active";
                                            initRowCellsRightMenu(row, row.country);
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.hide = true;
                                            row.country.active = false;
                                            break;
                                        case helperService.LoadTag.Edited:
                                            break;
                                    }
                                    cell.edited = true;
                                    cell.ready = true;
                                    cell.onClose();
                                    $scope.cellsToValidate--;
                                    canSaveVersion();
                                }
                            }
                        },
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Discard",
                                class: 'button button-border button-red button-icon icon-delete text-align-center',
                                callback: function () {
                                    switch (row.country.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.hide = true;
                                            row.country.active = false;
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.country.tag = helperService.LoadTag.Empty;
                                            row.country.active = true;
                                            initRowCellsRightMenu(row, row.country);
                                            cell.edited = true;
                                            break;
                                        case helperService.LoadTag.Edited:
                                            break;
                                    }
                                    cell.ready = true;
                                    cell.onClose();
                                    $scope.cellsToValidate--;
                                    canSaveVersion();
                                }
                            }
                        }
                    ],
                    properties: {
                        showReverseEdit: false,
                        showApply: false,
                    }
                };
            };

            var isInRegion = function (regionCountries, countryId) {
                var result = false;
                regionCountries.countries.forEach(function (country) {
                    if (country.id == countryId)
                        result = true;
                });
                return result;
            };

            var updateRegionCountries = function (fields, countryId) {
                var updates = [];
                fields.forEach(function(field) {
                    if (field.value != field.originalValue) {
                        field.cellValue = field.value;
                        updates.push({ regionId: field.regionId, countryId: countryId, isAssign: field.cellValue });
                    }                        
                });
                countryService.updateRegionCountries(updates).then(function(result) {
                    if (result) {
                        getRegionsCountries();                        
                    }
                });
            };

            var canUpdateRegionCountries = function(fields) {
                var result = false;
                fields.forEach(function(field) {
                    if (field.value == true)
                        result = true;
                });
                return result;
            };

            var getAssignCountryToRegionsMenu = function(countryId) {
                var rightMenu = {fields: []}

                $scope.regionsCountries.forEach(function(regionCountries) {
                    var field = {
                        type: helperService.FieldTypes.Checkbox,
                        regionId: regionCountries.region.id,
                        text: regionCountries.region.name,
                        originalValue: isInRegion(regionCountries, countryId),
                        value: isInRegion(regionCountries, countryId),                        
                        cellValue: isInRegion(regionCountries, countryId),
                        name: helperService.makeid()
                    }
                    rightMenu.fields.push(field);
                });

                rightMenu.properties = {
                    apply: function (fields, cell, row, table) {
                        if (canUpdateRegionCountries(fields)) {                            
                            updateRegionCountries(fields, row.country.id);
                        }                            
                    },
                    showReverseEdit: false
                }

                return rightMenu;
            };

            var initRowCellsRightMenu = function (row, currentCountry) {
                row.cells[0].rightMenu = getAssignCountryToRegionsMenu(currentCountry.id);
                row.cells[1].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            text: "ISO 2: ",
                            cellValue: currentCountry.iso2,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                isValid: function (field, cell, row, table) {
                                    return field.value.length < 3 && field.value.length > 0;
                                }
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.country.iso2 = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.country.iso2;
                            cell.value = row.country.iso2;
                            cell.rightMenu.properties.showReverseEdit = true;
                            canSaveVersion();
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.country.iso2 = row.originalCountry.iso2;
                            cell.rightMenu.fields[0].cellValue = row.country.iso2;
                            cell.value = row.country.iso2;
                            cell.edited = false;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };
                row.cells[2].rightMenu = null;
                row.cells[3].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            text: "Export name: ",
                            cellValue: currentCountry.exportName,
                            properties: {
                                required: true,
                                focus: true,
                                select: true
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.country.exportName = fields[0].value; 
                            cell.rightMenu.fields[0].cellValue = row.country.exportName;
                            cell.value = row.country.exportName;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.country.exportName = row.originalCountry.exportName;
                            cell.rightMenu.fields[0].cellValue = row.country.exportName;
                            cell.value = row.country.exportName;
                            cell.edited = false;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };
                row.cells[4].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Select,
                            text: "Currency: ",
                            cellValue: currentCountry.displayCurrency,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: prepareCurrencyForEdit(row.country.currencyId),
                                areEquals: function (x, y) {
                                    return Math.abs((x - y) / (x + y) * 2) < helperService.EqualityThreshold;
                                },
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.country.currencyId = fields[0].value; // check !
                            var currency = getSelectedCurrency(fields[0].properties.items);
                            row.country.displayCurrency = currency.text;
                            cell.value = row.country.displayCurrency;
                            cell.rightMenu.fields[0].cellValue = row.country.currencyId;
                            cell.rightMenu.fields[0].properties.items = prepareCurrencyForEdit(row.country.currencyId);
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.country.currencyId = row.originalCountry.currencyId;
                            row.country.displayCurrency = row.originalCountry.displayCurrency;
                            cell.rightMenu.fields[0].cellValue = row.country.currencyId;
                            cell.rightMenu.fields[0].properties.items = prepareCurrencyForEdit(row.country.currencyId);
                            cell.value = row.country.displayCurrency;
                            cell.edited = false;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };
                row.cells[5].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Select,
                            cellValue: currentCountry.active,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: prepareStatusForEdit(currentCountry.active),
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.country.active = fields[0].value; // check !
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.country.active);
                            cell.rightMenu.fields[0].cellValue = row.country.active;
                            cell.value = row.country.active ? "Active" : "Inactive";
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.country.active = row.originalCountry.active;
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.country.active);
                            cell.rightMenu.fields[0].cellValue = row.country.active;
                            cell.value = row.country.active ? "Active" : "Inactive";
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };
            };

            var prepareStatusForEdit = function (isActive) {
                var status = [];

                status.push({
                    id: true,
                    text: "Active",
                    textShort: "Active",
                    selected: isActive
                });

                status.push({
                    id: false,
                    text: "Inactive",
                    textShort: "Inactive",
                    selected: !isActive
                });

                return status;
            };
            
            var prepareCurrencyForEdit = function (selectedCurrencyId) {
                var currencies = [];
                for (var i = 0; i < $scope.currencies.length; i++) {
                    var curr = helperService.clone($scope.currencies[i]);
                    curr.selected = curr.id == selectedCurrencyId;
                    currencies.push(curr);
                }
                return currencies;
            };

            var getSelectedCurrency = function (currencies) {
                var currency = {};
                currencies.forEach(function (item) {
                    if (item.selected)
                        currency = item;
                });
                return currency;
            };

            var getSelectedRegionId = function () {
                var regionSelected = _.find($scope.filterOptions.header.items[1].properties.items, function (region) {
                    return region.selected == true;
                });
                return regionSelected ? regionSelected.id : 0;
            };

            $scope.getRegionForFilter = function (regionsCountriesResponse) {
                var regions = [];

                regionsCountriesResponse.forEach(function (elem, i) {
                    regions.push({
                        id: elem.region.id,
                        text: elem.region.name,
                        textShort: elem.region.shortname,
                        selected: (($scope.lastSettingsChosen.defaultRegionId && $scope.lastSettingsChosen.defaultRegionId == elem.region.id)
                            || (!$scope.lastSettingsChosen.defaultRegionId && i == 0)) ? true : false
                    });
                });

                if (!hasRegionSelected(regions))
                    regions[0].selected = true;

                return regions;
            };

            var hasRegionSelected = function (regions) {
                var n = 0;
                regions.forEach(function(region) {
                    if (region.selected)
                        n++;
                });
                return n > 0;
            };
                        
            var onSearch = function () {
                searchRequest.status = getSelectedStatus();
                searchRequest.regionId = getSelectedRegionId();
                resetPaging();
                $scope.getCountries();
            };

            var getSelectedStatus = function () {                
                var statusSelected = _.find(statusFilter.properties.items, function (statusSelectedTmp) {
                    return statusSelectedTmp != undefined && statusSelectedTmp.selected == true;
                });

                return statusSelected == null ? 0 : statusSelected.id;
            };

            $scope.getCountries = function () {                
                countryService.getCountriesPaged(searchRequest).then(function (result) {
                    searchRequest.pageNumber++;

                    if (result.countries) {
                        $scope.countries = $scope.countries.concat(result.countries);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more countries - " + $scope.countries.length + " out of " + result.totalCountries;
                        initTable();
                    }

                });
            };

            var resetPaging = function () {
                $scope.countries = [];
                searchRequest.pageNumber = 0;
            };

            var canSaveVersion = function () {
                if (validate) {
                    var allRowsAreReady = true;
                    for (var i = 1; i < $scope.table.rows.length && allRowsAreReady; i++) {
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

            $scope.save = function () {
                var data = { countries: getEditedRows(), validate: validate, loadId: $routeParams.loadId };
                if (validate || data.countries.length > 0) {
                    countryService.save(data).then(function () {
                        if (validate) {
                            $location.path('/data/load/' + $routeParams.loadId);
                        } else {
                            onSearch();
                        }
                    });
                }
            };

            var getEditedRows = function () {
                var editedRows = [];

                if (!$scope.canSave) return editedRows;
                
                for (var i = 1; i < $scope.table.rows.length; i++) {
                    for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            editedRows.push($scope.table.rows[i].country);
                            break;
                        }
                    }
                }
                return editedRows;
            };
        }
    ]);
});