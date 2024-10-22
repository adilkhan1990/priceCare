define(['pricecare/products/module'], function (module) {
    'use strict';
    module.controller('ProductsController', [
        '$scope', '$rootScope', 'helperService', 'productService', 'dimensionService', 'validate', '$routeParams', '$location',
        function ($scope, $rootScope, helperService, productService, dimensionService, validate, $routeParams, $location) {

            $scope.loadId = $routeParams.loadId;

            $scope.products = [];
            $scope.validate = validate;
            $scope.cellsToValidate = 0;

            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage,
                validate: validate
            };

            $scope.getProducts = function () {
                productService.getPagedProducts($scope.searchRequest).then(function (result) {

                    $scope.searchRequest.pageNumber++;

                    if (result) {
                        $scope.products = $scope.products.concat(result.products);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more products - "+$scope.products.length + " out of " + result.totalProducts;
                        initTable();
                    }

                });
            };

            var prepareUnitForEdit = function(unitSelected) {
                var result = [];

                $scope.units.forEach(function(unit) {
                    result.push({
                        id: unit.id ,
                        text: unit.name,
                        selected: unit.id == unitSelected
                    });

                });

                return result;
            };

            var changeSelectedItemForSelectField = function(selectField, unitSelected) {
                selectField.forEach(function(item) {
                    item.selected = (item.id == unitSelected) ? true : false;
                });
                return selectField;
            };

            var init = function() {
                dimensionService.getUnitTypes().then(function (data) {
                    $scope.getProducts();
                    $scope.units = data;

                    $scope.filterOptions = {
                        header: {
                            items: [
                                 {
                                     type: helperService.FieldTypes.Label,
                                     properties:
                                     {
                                         text: "List of products"
                                     }
                                 }
                            ],
                            primaryDisplayOptions: [],
                            displayOptions: [],
                            showDisplayOptions: false
                        },
                        advancedFilters: [],
                        showAdvancedFilters: false,
                        onChanged: function () {
                        }
                    };
                });
            };

            init();

            var initTable = function () {
                var columnNames = ["Product name", "Short name", "Export name", "Display name", "Display consolidation units", "Status"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                for (var i = 0; i < $scope.products.length; i++) {


                    var currentProduct = $scope.products[i];

                    // Define basic cells
                    var productNameCell = { value: currentProduct.name };

                    var shortNameCell = { value: currentProduct.shortName, ready: validate && currentProduct.tag == helperService.LoadTag.Empty };

                    var exportNameCell = { value: currentProduct.exportName };

                    var displayNameCell = { value: currentProduct.displayName };

                    var displayConsolidationUnitCell = { value: currentProduct.displayConsolidationUnit };

                    var statusCell = { value: currentProduct.active ? "Active" : "Inactive" };
                    
                    var newRow = {
                        originalProduct: $scope.products[i],
                        product: helperService.clone($scope.products[i]),
                        cells: [productNameCell, shortNameCell, exportNameCell, displayNameCell, displayConsolidationUnitCell, statusCell]
                    };

                    // style the row
                    if (validate) {
                        if (!currentProduct.tag == "") {
                            $scope.cellsToValidate++;
                            productNameCell.styleLoad = {
                                border: helperService.borderLeft,
                                background: currentProduct.tag == helperService.LoadTag.Deleted
                            };
                            shortNameCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentProduct.tag == helperService.LoadTag.Deleted
                            };
                            exportNameCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentProduct.tag == helperService.LoadTag.Deleted
                            };
                            displayNameCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentProduct.tag == helperService.LoadTag.Deleted
                            };
                            displayConsolidationUnitCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentProduct.tag == helperService.LoadTag.Deleted
                            };
                            statusCell.styleLoad = {
                                border: helperService.borderRight,
                                background: currentProduct.tag == helperService.LoadTag.Deleted
                            };
                        }
                    }

                    // Enrich cells
                    if (!validate || currentProduct.tag == helperService.LoadTag.Empty) {
                        initRowCellsRightMenu(newRow, currentProduct);
                    } else {
                        productNameCell.rightMenu = initRightMenuValidate(newRow, productNameCell, currentProduct);
                        shortNameCell.rightMenu = initRightMenuValidate(newRow, shortNameCell, currentProduct);
                        exportNameCell.rightMenu = initRightMenuValidate(newRow, exportNameCell, currentProduct);
                        displayNameCell.rightMenu = initRightMenuValidate(newRow, displayNameCell, currentProduct);
                        displayConsolidationUnitCell.rightMenu = initRightMenuValidate(newRow, displayConsolidationUnitCell, currentProduct);
                        statusCell.rightMenu = initRightMenuValidate(newRow, statusCell, currentProduct);
                    }

                    rows.push( newRow );
                };

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getProducts();
                        },
                        counterText: $scope.counter
                    }
                };
                canSaveVersion();
            };

            var initRightMenuValidate = function (row, cell, product) {
                return {
                    hideTextHelp: true,
                    fields: [
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Accept",
                                class: 'button button-border button-green button-icon icon-save text-align-center',
                                callback: function () {
                                    switch (product.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.product.active = true;
                                            row.cells[5].value = "Active";
                                            initRowCellsRightMenu(row, product);
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.hide = true;
                                            row.product.active = false;
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
                                    switch (product.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.hide = true;
                                            row.product.active = false;
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            product.tag = helperService.LoadTag.Empty;
                                            row.product.active = true;
                                            initRowCellsRightMenu(row, product);
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

            var initRowCellsRightMenu = function (row, currentProduct) {
                row.cells[0].rightMenu = null;
                row.cells[1].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            cellValue: currentProduct.shortName,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell, row, table) {
                            row.product.shortName = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.product.shortName;
                            cell.value = row.product.shortName;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function(fields, cell, row, table) {
                            row.product.shortName = row.originalProduct.shortName;
                            cell.value = row.product.shortName;
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.fields[0].cellValue = row.product.shortName;
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };
                row.cells[2].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            cellValue: currentProduct.exportName,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell, row, table) {
                            row.product.exportName = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.product.exportName;
                            cell.value = row.product.exportName;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function(fields, cell, row, table) {
                            row.product.exportName = row.originalProduct.exportName;
                            cell.value = row.product.exportName;
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.fields[0].cellValue = row.product.exportName;
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };
                row.cells[3].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            cellValue: currentProduct.displayName,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell, row, table) {
                            row.product.displayName = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.product.displayName;
                            cell.value = row.product.displayName;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function(fields, cell, row, table) {
                            row.product.displayName = row.originalProduct.displayName;
                            cell.value = row.product.displayName;
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.fields[0].cellValue = row.product.displayName;
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };
                row.cells[4].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            cellValue: currentProduct.factorScreen,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                allowDecimal: true,
                                areEquals: function(x, y) {
                                    return Math.abs((x - y) / (x + y) * 2) < helperService.EqualityThreshold;
                                },
                            }
                        },
                        {
                            type: helperService.FieldTypes.Select,
                            cellValue: currentProduct.baseConsolidationUnit,
                            properties: {
                                required: true,
                                class: 't-custom-select-boxed',
                                items: prepareUnitForEdit(currentProduct.baseConsolidationUnitId),
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell, row, table) {
                            row.product.factorScreen = fields[0].value;
                            row.product.baseConsolidationUnitId = fields[1].value;
                            var unit = _.find($scope.units, function(data) { return data.id == row.product.baseConsolidationUnitId; });
                            row.product.baseConsolidationUnit = unit.name;
                            row.product.displayConsolidationUnit = row.product.factorScreen + row.product.baseConsolidationUnit;

                            cell.rightMenu.fields[0].cellValue = row.product.factorScreen;
                            cell.rightMenu.fields[1].properties.items = changeSelectedItemForSelectField(cell.rightMenu.fields[1].properties.items, row.product.baseConsolidationUnitId);
                            cell.rightMenu.fields[1].cellValue = row.product.baseConsolidationUnitId;
                            cell.value = row.product.displayConsolidationUnit;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function(fields, cell, row, table) {
                            row.product.factorScreen = row.originalProduct.factorScreen;
                            row.product.baseConsolidationUnitId = row.originalProduct.baseConsolidationUnitId;
                            row.product.baseConsolidationUnit = row.originalProduct.baseConsolidationUnit;
                            row.product.displayConsolidationUnit = row.originalProduct.displayConsolidationUnit;

                            cell.rightMenu.fields[0].cellValue = row.product.factorScreen;
                            cell.rightMenu.fields[1].properties.items = changeSelectedItemForSelectField(cell.rightMenu.fields[1].properties.items, row.product.baseConsolidationUnitId);
                            cell.rightMenu.fields[1].cellValue = row.product.baseConsolidationUnitId;
                            cell.value = row.product.displayConsolidationUnit;
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };
                row.cells[5].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Select,
                            cellValue: currentProduct.active,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: prepareStatusForEdit(currentProduct.active),
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell, row, table) {
                            row.product.active = fields[0].value; // check !
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.product.active);
                            cell.rightMenu.fields[0].cellValue = row.product.active;
                            cell.value = row.product.active ? "Active" : "Inactive";
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function(fields, cell, row, table) {
                            row.product.active = row.originalProduct.active;
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.product.active);
                            cell.rightMenu.fields[0].cellValue = row.product.active;
                            cell.value = row.product.active ? "Active" : "Inactive";
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };

            };

            var prepareStatusForEdit = function(isActive) {
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

            var getEditedRows = function () {
                var editedRows = [];

                //if (!$scope.canSave) return editedRows;
                
                for (var i = 1; i < $scope.table.rows.length; i++) {
                    for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            editedRows.push($scope.table.rows[i].product);
                            break;
                        }
                    }
                }
                return editedRows;
            };

            var resetPaging = function() {
                $scope.searchRequest.pageNumber = 0;
                $scope.products = [];
            };

            $scope.saveVersion = function() {
                var datas = { products: getEditedRows(), validate: validate, loadId: $routeParams.loadId };
                if (validate || datas.products.length > 0) {
                    productService.saveProduct(datas).then(function () {
                        if (validate) {
                            $location.path('/data/load/' + $routeParams.loadId);
                        } else {
                            resetPaging();
                            $scope.getProducts();
                        }
                    });
                }
            };

        }
    ]);
});