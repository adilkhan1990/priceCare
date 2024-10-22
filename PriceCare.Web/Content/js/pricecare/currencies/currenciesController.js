define(['pricecare/currencies/module'], function(module) {
    'use strict';
    module.controller('CurrenciesController', [
        '$scope', '$rootScope', 'helperService', 'currencyService', 'versionService', 'validate', 'userService', '$q', '$routeParams', '$location', '$modal', 'excelService',
        'tableService',
        function ($scope, $rootScope, helperService, currencyService, versionService, validate, userService, $q, $routeParams, $location, $modal, excelService,
            tableService) {

            $scope.loadId = $routeParams.loadId;

            $scope.currencies = [];
            $scope.validate = validate;
            $scope.cellsToValidate = 0;

            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage,
                validate: validate
            };

            var validateState = {
                Budget: false,
                Spot: false
            };

            var resultValidation = [];

            var rateTypeFilter = {
                type: helperService.FieldTypes.Select,
                name: "ratetype",
                properties:
                {
                    class: 't-custom-select-text',
                    items: [
                        { id: 0, text: 'Budget', selected: true },
                        { id: 1, text: 'Spot', selected: false }
                    ],
                }
            };

            var rateTypeLabel = {
                type: helperService.FieldTypes.Label,
                properties: {
                    text: "Budget"
                }
            };

            var versionFilter = {
                type: helperService.FieldTypes.Select,
                name: "version",
                properties: {
                    class: 't-custom-select-text',
                }
            };

            var isValidated = function (row) {
                var validated = false;
                for (var i = 0; i < row.cells.length && !validated; i++) {
                    validated = row.cells[i].ready;
                }
                return validated;
            };

            var updateStatusLoad = function() {
                var cpt = 0;
                for (var i = 0; i < $scope.table.rows.length; i++) {
                    cpt += isValidated($scope.table.rows[i]) ? 1 : 0;
                }
                $scope.statusLoad = cpt +" on "+($scope.table.rows.length - 1);
            };

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
                callback: function () {
                    calculateSmartResolution();
                },
                apply: function () {
                    applySmartResolution(applyLoadOnCell);
                },
                reject: function () {
                    applySmartResolution(rejectLoadOnCell);
                }
            };
           
            var calculateSmartResolution = function () {

                if ($scope.smartResolutionOptions.rulesData.length == 0) {
                    $scope.smartResolutionOptions.affectedCells = 0;
                } else {
                    var cellCount = 0;
                    tableService.foreachRows($scope.table, function (row) {

                        if (row.currency && row.currency.tag == helperService.LoadTag.Edited && !isValidated(row)) {
                            var canResolve = true;

                            for (var i = 0; i < $scope.smartResolutionOptions.rulesData.length; i++) {
                                var rule = $scope.smartResolutionOptions.rulesData[i];
                                var baseRule = _.find(helperService.RuleTypes, function (r) { return r.id == rule.type; });

                                var usdRateToEvaluate = Math.abs(((row.currency.usdRate - row.currency.oldValue.usdRate) / row.currency.usdRate));
                                var eurRateToEvaluate = Math.abs(((row.currency.eurRate - row.currency.oldValue.eurRate) / row.currency.eurRate));

                                if (!baseRule.check(usdRateToEvaluate, rule.value / 100) || !baseRule.check(eurRateToEvaluate, rule.value / 100)) {
                                    canResolve = false;
                                    break;
                                }
                            }

                            if (canResolve) {
                                cellCount++;
                            }
                        }
                    });

                    $scope.smartResolutionOptions.affectedCells = cellCount;
                }
            };

            var applySmartResolution = function (applyChanges) {

                if ($scope.smartResolutionOptions.rulesData.length == 0) {
                    return;
                } else {
                    tableService.foreachRows($scope.table, function (row) {
                        if (row.currency && row.currency.tag == helperService.LoadTag.Edited && !isValidated(row)) {
                            var canResolve = true;

                            for (var i = 0; i < $scope.smartResolutionOptions.rulesData.length; i++) {
                                var rule = $scope.smartResolutionOptions.rulesData[i];
                                var baseRule = _.find(helperService.RuleTypes, function (r) { return r.id == rule.type; });

                                var usdRateToEvaluate = Math.abs(((row.currency.usdRate - row.currency.oldValue.usdRate) / row.currency.usdRate));
                                var eurRateToEvaluate = Math.abs(((row.currency.eurRate - row.currency.oldValue.eurRate) / row.currency.eurRate));

                                if (!baseRule.check(usdRateToEvaluate, rule.value / 100) || !baseRule.check(eurRateToEvaluate, rule.value / 100)) {
                                    canResolve = false;
                                    break;
                                }
                            }
                            if (canResolve) {
                                applyChanges(null, row);
                            }
                        }
                    });

                    calculateSmartResolution();
                }
            };

            $scope.smartDeleteResolutionOptions = {
                predescription: 'rows correspond to previous version with no new values',
                actionText: 'Keep', // discard
                actionRejectText: 'Reject', // accept
                displayReject: true,
                hideToggle: true,
                affectedCells: 0,
                callback: function () {
                    calculateDeleteSmartResolution();
                },
                apply: function () {
                    applyDeleteSmartResolution(rejectLoadOnCell);
                },
                reject: function() {
                    applyDeleteSmartResolution(applyLoadOnCell);
                }
            };

            var calculateDeleteSmartResolution = function () {
                var cellCount = 0;
                tableService.foreachRows($scope.table, function (row) {
                    if (row.currency && row.currency.tag == helperService.LoadTag.Deleted) {
                        cellCount++;
                    }
                });

                $scope.smartDeleteResolutionOptions.affectedCells = cellCount;
            };

            var applyDeleteSmartResolution = function (applyChanges) {

                tableService.foreachRows($scope.table, function (row) {
                    if (row.currency && row.currency.tag == helperService.LoadTag.Deleted) {
                        applyChanges(null, row);

                    }
                });

                calculateDeleteSmartResolution();
            };

            $scope.firstVersionSelected = function () {
                if (versionFilter.properties.items && versionFilter.properties.items.length > 0) {
                    return versionFilter.properties.items[0].selected == true;
                }

                return false;
            }

            var resetPaging = function() {
                $scope.searchRequest.pageNumber = 0;
                $scope.currencies = [];
            };

            var updateCurrencies = function() {
                for (var i = 1; i < $scope.table.rows.length; i++) {
                    if ($scope.table.rows[i].cells[0].edited) //name edited
                        $scope.currencies[i - 1].name = $scope.table.rows[i].cells[0].value;

                    if ($scope.table.rows[i].cells[2].edited) //usd rate
                        $scope.currencies[i - 1].usdRate = $scope.table.rows[i].cells[2].value;

                    if ($scope.table.rows[i].cells[3].edited) //eur rate
                        $scope.currencies[i - 1].eurRate = $scope.table.rows[i].cells[3].value;
                }
            };

            var setTable = function() {
                var columnNames = ["Currency", "ISO", "$ Rate", "€ Rate"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function(name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                for (var i = 0; i < $scope.currencies.length; i++) {
                    var currentCurrency = $scope.currencies[i];

                    // Define basic cells
                    var nameCell = { value: currentCurrency.name, ready: validate && currentCurrency.tag == helperService.LoadTag.Empty }; 

                    var isoCell = { value: currentCurrency.iso };

                    var usdRateCell = {
                        value: currentCurrency.usdRate,
                        format: helperService.formatNumber
                    };

                    var eurRateCell = {
                        value: currentCurrency.eurRate,
                        format: helperService.formatNumber
                    };

                    var newRow = {
                        currency: currentCurrency,
                        cells:[nameCell, isoCell, usdRateCell, eurRateCell]
                    };

                    // style the row
                    if (validate) {
                        if (!currentCurrency.tag == "") {
                            $scope.cellsToValidate++;
                            nameCell.styleLoad = {
                                border: helperService.borderLeft,
                                background: currentCurrency.tag == helperService.LoadTag.Deleted
                            };
                            isoCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentCurrency.tag == helperService.LoadTag.Deleted
                            };
                            usdRateCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentCurrency.tag == helperService.LoadTag.Deleted
                            };
                            eurRateCell.styleLoad = {
                                border: helperService.borderRight,
                                background: currentCurrency.tag == helperService.LoadTag.Deleted
                            };
                        }
                    }

                    // Enrich cells
                    if (!validate || currentCurrency.tag == helperService.LoadTag.Empty) {
                        initRowCellsRightMenu(newRow, currentCurrency);
                    } else {
                        nameCell.rightMenu = initRightMenuValidate(newRow,nameCell, $scope.currencies[i]);
                        isoCell.rightMenu = initRightMenuValidate(newRow, isoCell, $scope.currencies[i]);
                        usdRateCell.rightMenu = initRightMenuValidate(newRow, usdRateCell, $scope.currencies[i]);
                        eurRateCell.rightMenu = initRightMenuValidate(newRow, eurRateCell, $scope.currencies[i]);
                    }

                    rows.push( newRow );
                }

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getCurrencies(false);
                        },
                        counterText: $scope.counter
                    }
                };
                updateStatusLoad();
                canSaveVersion();
            };

            var initRowCellsRightMenu = function(row, currentCurrency) {
                row.cells[0].rightMenu = {
                    fields: [
                    {
                        type: helperService.FieldTypes.Textbox,
                        text: "Name:",
                        cellValue: currentCurrency.name,
                        properties: {
                            required: true,
                            focus: true
                        }
                    }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            cell.value = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = cell.value;
                            cell.rightMenu.properties.showReverseEdit = true;
                            $scope.canSave = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            cell.edited = false;
                            cell.value = row.currency.name;
                            cell.rightMenu.fields[0].cellValue = row.currency.name;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };

                row.cells[1].rightMenu = null;

                row.cells[2].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: "Value:",
                            cellValue: currentCurrency.usdRate,
                            properties: {
                                allowDecimal: true,
                                required: true,
                                focus: true
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            cell.value = parseFloat(fields[0].value);
                            cell.rightMenu.fields[0].cellValue = cell.value;
                            cell.rightMenu.properties.showReverseEdit = true;
                            $scope.canSave = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            cell.edited = false;
                            cell.value = row.currency.usdRate;
                            cell.rightMenu.fields[0].cellValue = row.currency.usdRate;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };

                row.cells[3].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: "Value:",
                            cellValue: currentCurrency.eurRate,
                            properties: {
                                allowDecimal: true,
                                required: true,
                                focus: true
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            cell.value = parseFloat(fields[0].value);
                            cell.rightMenu.fields[0].cellValue = cell.value;
                            cell.rightMenu.properties.showReverseEdit = true;
                            $scope.canSave = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            cell.edited = false;
                            cell.value = row.currency.eurRate;
                            cell.rightMenu.fields[0].cellValue = row.currency.eurRate;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };
            };

            var initRightMenuValidate = function(row, cell, currency) {
                var rightMenu = {
                    hideTextHelp: true,
                    fields: [
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Accept",
                                class: 'button button-border button-green button-icon icon-save text-align-center',
                                callback: function() {
                                    applyLoadOnCell(cell, row);
                                    $scope.cellsToValidate--;
                                }
                            }
                        },
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Discard",
                                class: 'button button-border button-red button-icon icon-delete text-align-center',
                                callback: function() {
                                    rejectLoadOnCell(cell, row);
                                    $scope.cellsToValidate--;
                                }
                            }
                        }
                    ],
                    properties: {
                        showReverseEdit: false,
                        showApply: false,
                    }
                };

                if (row.currency.tag == helperService.LoadTag.Edited && row.currency.oldValue) {
                    rightMenu.fields.unshift({
                        type: helperService.FieldTypes.Label,
                        properties: {
                            text: "OldValue: [UsdRate," + helperService.formatNumber(row.currency.oldValue.usdRate)
                                + "] [EurRate," + helperService.formatNumber(row.currency.oldValue.eurRate) + "]"
                        }
                    });
                }

                return rightMenu;
            };

            var rejectLoadOnCell = function (cell, row) {
                switch (row.currency.tag) {
                    case helperService.LoadTag.Loaded:
                        row.hide = true;
                        row.currency.active = false;
                        break;
                    case helperService.LoadTag.Deleted:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        row.currency.tag = helperService.LoadTag.Empty;
                        row.currency.active = true;
                        initRowCellsRightMenu(row, row.currency);
                        break;
                    case helperService.LoadTag.Edited:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        initRowCellsRightMenu(row, row.currency);
                        row.currency.eurRate = row.currency.oldValue.eurRate;
                        row.currency.usdRate = row.currency.oldValue.usdRate;
                        row.cells[2].value = row.currency.oldValue.eurRate;
                        row.cells[3].value = row.currency.oldValue.usdRate;
                        row.cells[2].rightMenu.fields[0].cellValue = row.currency.oldValue.eurRate;
                        row.cells[3].rightMenu.fields[0].cellValue = row.currency.oldValue.usdRate;
                        break;
                }
                row.cells.forEach(function(cell) {
                    cell.edited = true;
                    cell.ready = true;
                    cell.onClose();
                    canSaveVersion();
                });
                updateStatusLoad();
            };

            var applyLoadOnCell = function (cell, row) {
                switch (row.currency.tag) {
                    case helperService.LoadTag.Loaded:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        row.currency.active = true;
                        initRowCellsRightMenu(row, row.currency);
                        break;
                    case helperService.LoadTag.Deleted:
                        row.hide = true;
                        row.currency.active = false;
                        row.currency.tag = helperService.LoadTag.Empty; // for load
                        break;
                    case helperService.LoadTag.Edited:
                        row.cells.forEach(function (cell) {
                            cell.styleLoad = null; // remove color
                        });
                        row.currency.active = true;
                        initRowCellsRightMenu(row, row.currency);
                        break;
                }
                row.cells.forEach(function(cell) {
                    cell.edited = true;
                    cell.ready = true;
                    cell.onClose();
                    canSaveVersion();
                });                
                updateStatusLoad();
            };

            var getRateTypeId = function() {
                var rateTypeId = 0;
                if ($scope.filterOptions) {
                    rateTypeFilter.properties.items.forEach(function(rateType) {
                        if (rateType.selected)
                            rateTypeId = rateType.id;
                    });
                }
                return rateTypeId;
            };

            var setRateSelected = function(rateIdToSelect) {
                rateTypeFilter.properties.items.forEach(function(rateType) {
                    rateType.selected = rateType.id == rateIdToSelect;
                });
            };

            var getSelectedVersionId = function() {
                var version = _.find(versionFilter.properties.items, function (item) { return item.selected });
                return version != undefined ? version.versionId : 0;
            };

            var getPagedCurrencies = function() {
                $scope.searchRequest.versionId = getSelectedVersionId();

                currencyService.getPaged($scope.searchRequest).then(function (result) {
                    $scope.searchRequest.pageNumber++;
                    
                    if (result) {
                        $scope.currencies = $scope.currencies.concat(result.currencies);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more currencies - " + $scope.currencies.length + " out of " + result.totalCurrencies;
                        setTable();
                        calculateDeleteSmartResolution();
                        calculateSmartResolution();
                    }

                });
            };

            $scope.getCurrencies = function (versionReload) {
                if ($scope.searchRequest.rateTypeId != $scope.originalRateTypeId || versionReload) { // reload versions

                    $scope.originalRateTypeId = $scope.searchRequest.rateTypeId;
                    var versionPromise = versionService.getCurrencyVersions($scope.searchRequest.rateTypeId);
                    var userMappingPromise = userService.getUserMapping();

                    $q.all([versionPromise, userMappingPromise]).then(function (datas) {
                        versionFilter.properties.items = versionService.getVersionsForFilter(datas[0], datas[1]);

                        getPagedCurrencies();
                    });

                } else {
                    getPagedCurrencies();
                }
            };

            var onSearch = function(versionReload) {
                resetPaging();
                $scope.searchRequest.rateTypeId = getRateTypeId();
                $scope.getCurrencies(versionReload);
            };

            var init = function() {
                $scope.originalRateTypeId = 0;
                $scope.searchRequest.rateTypeId = getRateTypeId();

                var versionPromise = versionService.getCurrencyVersions($scope.searchRequest.rateTypeId);
                var userMappingPromise = userService.getUserMapping();

                $q.all([versionPromise, userMappingPromise]).then(function(datas) {
                    var versions = versionService.getVersionsForFilter(datas[0], datas[1]);
                    versionFilter.properties.items = versions;
                    $scope.filterOptions = {
                        header: {
                            items: [
                                (validate) ? rateTypeLabel : rateTypeFilter,
                                versionFilter
                            ],
                            primaryDisplayOptions: [],
                            showAdvancedFilters: false,
                        },
                        onChanged: function() {
                            onSearch(false);
                        }
                    };

                    if (!validate) {
                        $scope.filterOptions.header.primaryDisplayOptions.push({
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
                                                    noFilterOnEvent: true,
                                                    noFilterOnCountry: true,
                                                    noFilterOnProduct: true,
                                                    isBudget: rateTypeFilter.properties.items[0].selected
                                                }
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function (response) {
                                        $scope.searchRequest.databaseLike = response.databaseLike;
                                        excelService.postFilterExcel($scope.searchRequest).then(function (result) {
                                            window.location.href = 'api/currency/excel?token=' + result.token +'&isBudget='+response.isBudget;
                                        });
                                    }, function () {

                                    });
                                }
                            }                                
                        });
                    }

                    onSearch(false);
                });

            };

            init();


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
                var rateSelected = getRateTypeId();
                var editedRows = getEditedRows();
                var data = { rateType: rateSelected, validate: validate, loadId: $routeParams.loadId };

                if (editedRows.length > 0) {
                    if (validate) {
                        resultValidation = resultValidation.concat(editedRows);
                        if (rateSelected == 0) {
                            validateState.Budget = true;
                            if (!validateState.Spot) {
                                setRateSelected(1); // Select spot
                                $scope.searchRequest.rateTypeId = 1;
                                rateTypeLabel.properties.text = "Spot";
                                onSearch(true);
                            }
                        } else {
                            validateState.Spot = true;
                            if (!validateState.Budget) {
                                setRateSelected(0); // Select budget
                                $scope.searchRequest.rateTypeId = 0;
                                rateTypeLabel.properties.text = "Budget";
                                onSearch(true);
                            }
                        }

                        if (validateState.Budget && validateState.Spot) {
                            data.currencies = resultValidation;
                            currencyService.saveVersion(data).then(function () {
                                $location.path('/data/load/' + $routeParams.loadId);
                            });
                        }
                    } else {
                        data.currencies = editedRows;
                        currencyService.saveVersion(data).then(function () {
                            onSearch(true);
                        });
                    }

                }
            };

            var getEditedRows = function () {
                var editedRows = [];

                if (!$scope.canSave) return editedRows;

                updateCurrencies(); // init changed values before

                for (var i = 1; i < $scope.table.rows.length; i++) {
                    for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            editedRows.push($scope.table.rows[i].currency);
                            break;
                        }
                    }
                }
                return editedRows;
            };
        }
    ]);
});