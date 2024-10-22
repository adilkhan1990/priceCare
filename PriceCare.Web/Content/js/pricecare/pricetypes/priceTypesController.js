define(['pricecare/pricetypes/module'], function (module) {
    'use strict';
    module.controller('PriceTypesController', ['$scope', 'helperService', 'priceTypeService', 'currencyService', '$q', 'validate', '$modal', '$routeParams', '$location', 'excelService',
        function ($scope, helperService, priceTypeService, currencyService, $q, validate, $modal, $routeParams, $location, excelService) {

            $scope.loadId = $routeParams.loadId;

            $scope.cellsToValidate = 0;

            $scope.showAddForm = false;
            $scope.priceTypes = [];          
            $scope.counter = ""; // contains counter pagination
            $scope.validate = validate;

            var canceler;

            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: 1000,//helperService.itemsPerPage,
                searchText: '',
                status: 1, // active by default
                validate: validate
            };

            $scope.addNew = function() {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/addPriceTypeModal.html',
                    controller: 'AddPriceTypeModalController',
                    backdrop: 'static'
                });
                modalInstance.result.then(function(newPriceType) {
                    priceTypeService.addPriceType(newPriceType).then(function(result) {
                        onSearch();
                    });
                });
            };
            
            var initTable = function () {
                var columnNames = ["Name", "ShortName", "Currency", "Status"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                for (var i = 0; i < $scope.priceTypes.length; i++) {
                    var currentPriceType = $scope.priceTypes[i];

                    var nameCell = { value: currentPriceType.name };
                    var shortNameCell = { value: currentPriceType.shortName, ready: validate && currentPriceType.tag == helperService.LoadTag.Empty }
                    var currencyCell = { value: currentPriceType.currencyName };
                    var statusCell = { value: currentPriceType.status ? "Active" : "Inactive" };

                    var newRow = {
                        originalPriceType: currentPriceType,
                        priceType: helperService.clone(currentPriceType),
                        cells: [nameCell, shortNameCell, currencyCell, statusCell]
                    };

                    // style the row
                    if (validate) {
                        if (!currentPriceType.tag == "") {
                            $scope.cellsToValidate++;
                            nameCell.styleLoad = {
                                border: helperService.borderLeft,
                                background: currentPriceType.tag == helperService.LoadTag.Deleted
                            };
                            shortNameCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentPriceType.tag == helperService.LoadTag.Deleted
                            };
                            currencyCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentPriceType.tag == helperService.LoadTag.Deleted
                            };
                            statusCell.styleLoad = {
                                border: helperService.borderRight,
                                background: currentPriceType.tag == helperService.LoadTag.Deleted
                            };
                        }
                    }

                    // Enrich cells
                    if (!validate || currentPriceType.tag == helperService.LoadTag.Empty) {
                        initRowCellsRightMenu(newRow, currentPriceType);
                    } else {
                        nameCell.rightMenu = initRightMenuValidate(newRow, nameCell);
                        shortNameCell.rightMenu = initRightMenuValidate(newRow, shortNameCell);
                        currencyCell.rightMenu = initRightMenuValidate(newRow, currencyCell);
                        statusCell.rightMenu = initRightMenuValidate(newRow, statusCell);
                    }

                    rows.push(newRow);
                }

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getPriceTypes();
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
                                    switch (row.priceType.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.priceType.status = true;
                                            row.cells[3].value = "Active";
                                            initRowCellsRightMenu(row, row.priceType);
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.hide = true;
                                            row.priceType.status = false;
                                            break;
                                        case helperService.LoadTag.Edited:
                                            break;
                                    }
                                    cell.edited = true;
                                    cell.ready = true;
                                    $scope.cellsToValidate--;                                  
                                    cell.onClose();
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
                                    switch (row.priceType.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.hide = true;
                                            row.priceType.status = false;
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.priceType.tag = helperService.LoadTag.Empty;
                                            row.priceType.status = true;
                                            initRowCellsRightMenu(row, row.priceType);
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

            var initRowCellsRightMenu = function (row, currentPriceType) {
                row.cells[0].rightMenu = null;

                row.cells[1].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            text: "ShortName: ",
                            cellValue: currentPriceType.shortName,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                isValidAsync: function (field, cell, row, table) {
                                    var priceType = {
                                        id: row.priceType.id,
                                        shortName: field.value
                                    };
                                    return priceTypeService.isValid(priceType);
                                }
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.priceType.shortName = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.priceType.shortName;
                            cell.value = row.priceType.shortName;
                            cell.rightMenu.properties.showReverseEdit = true;
                            canSaveVersion();
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.priceType.shortName = row.originalPriceType.shortName;
                            cell.rightMenu.fields[0].cellValue = row.priceType.shortName;
                            cell.value = row.priceType.shortName;
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
                            type: helperService.FieldTypes.Select,
                            cellValue: currentPriceType.status,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: prepareStatusForEdit(currentPriceType.status),
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.priceType.status = fields[0].value; // check !
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.priceType.status);
                            cell.rightMenu.fields[0].cellValue = row.priceType.status;
                            cell.value = row.priceType.status ? "Active" : "Inactive";
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.priceType.status = row.originalPriceType.status;
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.priceType.status);
                            cell.rightMenu.fields[0].cellValue = row.priceType.status;
                            cell.value = row.priceType.status ? "Active" : "Inactive";
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };
            };


            var getSearchText = function () {
                var text = "";
                if ($scope.filterOptions.filters[1].value != null || $scope.filterOptions.filters[1].value != "")
                    text = $scope.filterOptions.filters[1].value;

                return text;
            };
            var getSelectedStatus = function() {
                var status = _.find($scope.filterOptions.filters[0].properties.items, function(s) {
                    return s.selected == true;
                });
                return (status != undefined) ? status.id : 0;
            };
            var resetPaging = function () {
                $scope.priceTypes = [];
                $scope.searchRequest.pageNumber = 0;
            };
            var onSearch = function () {
                $scope.searchRequest.searchText = getSearchText();
                $scope.searchRequest.status = getSelectedStatus();
                resetPaging();
                $scope.getPriceTypes();
            };
            
            $scope.getPriceTypes = function () {
                if (canceler) {
                    canceler.resolve();
                    canceler = null;
                }
                canceler = $q.defer();

                priceTypeService.getPagedPriceTypes($scope.searchRequest, canceler).then(function (result) {
                    $scope.searchRequest.pageNumber++;

                    if (result) {
                        result.priceTypes.forEach(function(item) {
                            item.statusAsString = item.status ? "Active" : "Inactive";
                        });
                        $scope.priceTypes = $scope.priceTypes.concat(result.priceTypes);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more price types - "+$scope.priceTypes.length + " out of " + result.totalPriceTypes;
                        initTable();
                    }
                    canceler = null;
                });
            };

            var init = function () {                 
                    $scope.filterOptions = {
                        header: {
                            items: [
                                {
                                    type: helperService.FieldTypes.Label,
                                    properties:
                                    {
                                        text: "List of prices type"
                                    }
                                }
                            ],
                            primaryDisplayOptions: []
                        },
                        filters: [
                            {
                                type: helperService.FieldTypes.Select,
                                name: 'status',
                                hide: validate,
                                properties: {
                                    class: 't-custom-select-boxed',
                                    items: [
                                        { id: 0, text: 'All status', textShort: 'All status' },
                                        { id: 1, text: 'Active', textShort: 'Active', selected: true },
                                        { id: 2, text: 'Inactive', textShort: 'Inactive' }
                                    ]
                                }
                            },
                             {
                                 type: helperService.FieldTypes.Textbox,
                                 hide: validate,
                                 name: 'searchBox',
                                 class: 'search',
                                 properties: {
                                     trigger: helperService.FieldTriggers.OnEnter | helperService.FieldTriggers.OnChange
                                 }
                             }
                        ],
                        showAdvancedFilters: false,
                        onChanged: function () {
                            onSearch();
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
                                                    noFilterOnProduct: true
                                                }
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function (response) {
                                        $scope.searchRequest.databaseLike = response.databaseLike;
                                        excelService.postFilterExcel($scope.searchRequest).then(function (result) {
                                            window.location.href = 'api/pricetype/excel?token=' + result.token;
                                        });
                                    }, function () {

                                    });
                                }
                            }
                        });
                    }

                    $scope.getPriceTypes();                        
            };
            init();


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
                var data = { priceTypes: getEditedRows(), validate: validate, loadId: $routeParams.loadId };
                if (validate || data.priceTypes.length > 0) {
                    priceTypeService.save(data).then(function () {
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
                            editedRows.push($scope.table.rows[i].priceType);
                            break;
                        }
                    }
                }
                return editedRows;
            };
        }]);
});