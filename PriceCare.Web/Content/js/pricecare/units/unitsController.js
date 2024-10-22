define(['pricecare/units/module'], function (module) {
    'use strict';
    module.controller('UnitsController', ['$scope', '$rootScope', '$q', 'helperService', 'userService', 'productService', 'dimensionService', '$modal',
        function ($scope, $rootScope, $q, helperService, userService, productService, dimensionService, $modal) {
          
            var productFilter = {
                type: helperService.FieldTypes.Select,
                name: 'products',
                properties: {
                    class: 't-custom-select-text'
                }
            }

            $scope.addUnit = function () {
                var productId = getSelectedProductId();
                var modalR = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/addProductUnitModal.html',
                    controller: 'AddProductUnitModalController',
                    backdrop: 'static',
                    resolve: {
                        items: function() {
                            return {
                                productId: productId,
                                productName: getProductName(productId),
                                unitOptions: getUnitsForFilter(),                                
                            }
                        }
                    }
                });

                modalR.result.then(function(productUnit) {
                    productService.addProductUnit(productUnit).then(function(result) {
                        if (result)
                            getProductUnits();
                    });
                });
            };

            $scope.delete = function (productUnit) {
                productService.deleteProductUnit(productUnit).then(function (result) {
                    if (result)
                        getProductUnits();
                });
            };

            var getActions = function (productUnit) {
                var cellActions = { actions: [] };
                if (!productUnit.isDefault) {
                    cellActions.actions.push({
                        text: 'delete',
                        class: 'icon icon-delete',
                        click: function (cell, row) {
                            $scope.delete(row.productUnit);
                        }
                    });
                }                                        
                return cellActions;
            };
            
            var getUnitsForFilter = function(selectedUnitId) {
                var units = [];
                $scope.units.forEach(function(item, i) {
                    units.push({
                        id: item.id,
                        text: item.name,
                        selected: selectedUnitId ? item.id == selectedUnitId : i == 0
                    });
                });
                return units;
            };

            var getProductName = function (productId) {
                var name;
                productFilter.properties.items.forEach(function(item) {
                    if (item.id == productId)
                        name = item.text;
                });
                return name;
            };

            var getUnitName = function(unitId) {
                var name;
                $scope.units.forEach(function(item) {
                    if (item.id == unitId)
                        name = item.name;
                });
                return name;
            };

            var setTable = function() {
                var columnNames = ["Product", "Unit", "FactorScreen", "Active", "IsDefault", "Action"];
                var rows = [{ title: true, cells: [] }];

                columnNames.forEach(function(name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                $scope.productUnits.forEach(function(productUnit) {
                    var row = {
                        productUnit: productUnit,
                        cells: [                            
                            { value: getProductName(productUnit.productId)},
                            { value: getUnitName(productUnit.unitId)},
                            { value: productUnit.factorScreen },
                            { value: productUnit.active },
                            { value: productUnit.isDefault },
                            getActions(productUnit)
                        ]
                    }

                    setUnitRightMenu(row);
                    setFactorScreenRightMenu(row);
                    setActiveRightMenu(row);
                    setIsDefaultRightMenu(row);
                    
                    rows.push(row);
                });

                $scope.table = {
                    rows: rows,
                };
            };

            var setUnitRightMenu = function(row) {
                row.cells[1].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Select,
                            text: "Unit",
                            cellValue: row.productUnit.unitId,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: getUnitsForFilter(row.productUnit.unitId),
                                areEquals: function(x, y) {
                                    return Math.abs((x - y) / (x + y) * 2) < helperService.EqualityThreshold;
                                },                                
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell) {
                            row.productUnit.unitId = getSelectedUnitId(fields[0]);
                            productService.updateProductUnit(row.productUnit).then(function(result) {
                                if (result)
                                    getProductUnits();
                            });
                        },
                        showReverseEdit: false
                    }
                }
            };

            var setFactorScreenRightMenu = function(row) {
                row.cells[2].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: 'Factor screen:',
                            cellValue: row.productUnit.factorScreen,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                allowDecimal: false,
                                showReverseEdit: false
                            }
                        }
                    ],
                    properties: {
                        apply: function(fields, cell) {
                            row.productUnit.factorScreen = fields[0].value;
                            productService.updateProductUnit(row.productUnit).then(function(result) {
                                if (result)
                                    getProductUnits();
                            });
                        },
                        showReverseEdit: false
                    }
                };
            };

            var setActiveRightMenu = function(row) {
                row.cells[3].rightMenu = {
                    fields: [
                    {
                        type: helperService.FieldTypes.Select,
                        text: "Active: ",
                        cellValue: row.productUnit.active,
                        properties: {
                            class: 't-custom-select-boxed',
                            items: [
                                { id: 1, text: 'Active', selected: row.productUnit.active },
                                { id: 0, text: 'Inactive', selected: !row.productUnit.active }
                            ]
                        }
                    }],
                    properties: {
                        apply: function(fields, cell) {
                            row.productUnit.active = fields[0].properties.items[0].selected;
                            productService.updateProductUnit(row.productUnit).then(function(result) {
                                if (result) {
                                    getProductUnits();
                                }
                            });
                        },
                        showReverseEdit: false
                    }
                }
            };

            var setIsDefaultRightMenu = function (row) {
                if (row.productUnit.isDefault) {
                    row.cells[4].rightMenu = null;
                } else {
                    row.cells[4].rightMenu = {
                        fields: [
                        {
                            type: helperService.FieldTypes.Checkbox,
                            text: "Active",
                            originalValue: row.productUnit.isDefault,
                            value: row.productUnit.isDefault,
                            cellValue: row.productUnit.isDefault,
                            name: helperService.makeid(),
                        }],
                        properties: {
                            apply: function (fields, cell) {
                                if (fields[0].originalValue != fields[0].value) {                                    
                                    row.productUnit.isDefault = fields[0].value;
                                    productService.updateProductUnit(row.productUnit).then(function (result) {
                                        if (result) {
                                            getProductUnits();
                                        }
                                    });
                                }
                                
                            },
                            showReverseEdit: false
                        }
                    }
                }                              
            };

            var getSelectedUnitId = function(field) {
                var id;

                field.properties.items.forEach(function(item) {
                    if (item.selected)
                        id = item.id;
                });

                return id;
            };
           
            var getSelectedProductId = function () {
                var id;

                productFilter.properties.items.forEach(function(product) {
                    if (product.selected)
                        id = product.id;
                });

                return id;
            };

            var getProductUnits = function () {
                var productId = getSelectedProductId();

                productService.getProductUnits(productId).then(function (units) {
                    $scope.productUnits = units;
                    setTable();
                });
            };

            var prepareProductsForFilter = function (productsResponse) {
                var products = [];
                                
                productsResponse.forEach(function (p, i) {
                    products.push({
                        id: p.id,
                        text: p.name,
                        textShort: p.shortname,
                        selected: ($scope.lastSettingsChosen.defaultProductId == p.id)
                            || (!$scope.lastSettingsChosen.defaultProductId && i == 0) ? true : false
                    });
                });
                
                return products;
            };

            var init = function() {
                var userPromise = userService.getLastSettingsChosen();
                var productPromise = productService.getAllProducts();
                var unitPromise = dimensionService.getUnitTypes();

                $q.all([userPromise, productPromise, unitPromise]).then(function(data) {

                    $scope.userSettings = data[0];
                    $scope.products = data[1];
                    $scope.units = data[2];

                    $scope.filterOptions = {
                        header: {
                            items: [
                                productFilter
                            ]
                        },
                        onChanged: function() { //products
                            getProductUnits();
                        }
                    }

                    $scope.lastSettingsChosen = data[0];
                    productFilter.properties.items = prepareProductsForFilter(data[1]);
                    getProductUnits();
                });
            };
            init();

        }
    ]);
});