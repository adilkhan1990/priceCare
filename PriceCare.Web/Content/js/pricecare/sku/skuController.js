define(['pricecare/sku/module'], function (module) {
    'use strict';
    module.controller('SkuController', ['$scope', '$rootScope', 'helperService', 'skuService', 'countryService', 'productService', 'dimensionService', '$q', 'userService', 'validate',
        '$modal', '$routeParams', '$location', 'loadService', 'excelService',
        function ($scope, $rootScope, helperService, skuService, countryService, productService, dimensionService, $q, userService, validate,
            $modal, $routeParams, $location, loadService, excelService) {

            var units = [];

            $scope.loadId = $routeParams.loadId;

            var usableProductsAndGeographyIds;
            $scope.skus = [];
            $scope.validate = validate;
            $scope.cellsToValidate = 0;
            
            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage,
                countryId: 0,
                productId: 0,
                validate: validate
            };

            var countryFilter = {
                type: helperService.FieldTypes.SelectMultiLevel,
                name: 'countries',
                properties: {
                    class: 't-custom-select-text',
                }
            };
            var productFilter = {
                type: helperService.FieldTypes.Select,
                name: 'products',
                properties: {
                    class: 't-custom-select-text',
                }
            };
            var prepareFormulationForCellFilter = function (formulationSelected) {
                var result = [];

                $scope.formulations.forEach(function (form) {
                    result.push({
                        id: form.id,
                        text: form.name,
                        selected: formulationSelected == form.id
                    });
                });

                return result;
            };

            $scope.addSku = function () {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/addSkuModal.html',
                    controller: 'AddSkuModalController',
                    backdrop: 'static'
                });

                modalInstance.result.then(function (newSku) {
                    var countryId = getSelectedCountryId();
                    var productId = getSelectedProductId();
                    var data = {
                        geographyId: countryId,
                        productId: productId,
                        name: newSku.name,
                        productNumber: newSku.productNumber,
                        dosage: newSku.dosage,
                        packSize: newSku.packSize,
                        formulationId: newSku.formulationId,
                        factorUnit: newSku.factorUnit,
                        status: true
                    };

                    skuService.addSku(data).then(function (result) {
                        if (result)
                            window.location.reload();
                    });
                });
            };

            var countryPromise = countryService.getRegionsAndCountries();
            var productPromise = productService.getAllProducts();
            var dimensionPromise = dimensionService.getFormulationTypes();

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

            var initTable = function () {
                var columnNames = ["Name", "Product number", "Dosage", "Pack size", "Device", "Status"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                for (var i = 0; i < $scope.skus.length; i++) {
                    var currentSku = $scope.skus[i];
                    var nameCell = { value: currentSku.name, ready: validate && currentSku.tag == helperService.LoadTag.Empty };
                    var productNumberCell = { value: currentSku.productNumber }
                    var dosageCell = { value: currentSku.dosage + ' (' + currentSku.unit+')' };                    
                    var packSizeCell = { value: currentSku.packSize };
                    var deviceCell = { value: currentSku.formulation };
                    var statusCell = { value: currentSku.status ? "Active" : "Inactive" };

                    var newRow = {
                        originalSku: currentSku,
                        sku: helperService.clone(currentSku),
                        cells: [nameCell, productNumberCell, dosageCell, packSizeCell, deviceCell, statusCell]
                    };

                    // style the row
                    if (validate) {
                        if (!currentSku.tag == "") {
                            $scope.cellsToValidate++;
                            nameCell.styleLoad = {
                                border: helperService.borderLeft,
                                background: currentSku.tag == helperService.LoadTag.Deleted
                            };
                            productNumberCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentSku.tag == helperService.LoadTag.Deleted
                            };
                            dosageCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentSku.tag == helperService.LoadTag.Deleted
                            };
                            packSizeCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentSku.tag == helperService.LoadTag.Deleted
                            };
                            deviceCell.styleLoad = {
                                border: helperService.borderMiddle,
                                background: currentSku.tag == helperService.LoadTag.Deleted
                            };
                            statusCell.styleLoad = {
                                border: helperService.borderRight,
                                background: currentSku.tag == helperService.LoadTag.Deleted
                            };
                        }
                    }

                    // Enrich cells
                    if (!validate || currentSku.tag == helperService.LoadTag.Empty) {
                        initRowCellsRightMenu(newRow, currentSku);
                    } else {
                        nameCell.rightMenu = initRightMenuValidate(newRow, nameCell);
                        productNumberCell.rightMenu = initRightMenuValidate(newRow, productNumberCell);
                        dosageCell.rightMenu = initRightMenuValidate(newRow, dosageCell);
                        packSizeCell.rightMenu = initRightMenuValidate(newRow, packSizeCell);
                        deviceCell.rightMenu = initRightMenuValidate(newRow, deviceCell);
                        statusCell.rightMenu = initRightMenuValidate(newRow, statusCell);
                    }

                    rows.push(newRow);
                };

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getSkus();
                        },
                        counterText: $scope.counter
                    }
                };

                canSaveVersion();
            };

            var initRightMenuValidate = function (row, cell) {
                var rightMenu = {
                    hideTextHelp: true,
                    fields: [
                        {
                            type: helperService.FieldTypes.Action,
                            properties: {
                                text: "Accept",
                                class: 'button button-border button-green button-icon icon-save text-align-center',
                                callback: function () {
                                    switch (row.sku.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.sku.status = true;                                            
                                            row.cells[5].value = "Active";

                                            if (!row.sku.formulation) {
                                                row.sku.formulation = "Not Specified";
                                                row.sku.formulationId = 17;
                                                row.cells[4].value = row.sku.formulation;
                                            }

                                            initRowCellsRightMenu(row, row.sku);
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.hide = true;
                                            row.sku.status = false;
                                            break;
                                        case helperService.LoadTag.Edited:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.sku.status = true;
                                            row.cells[5].value = "Active";
                                            initRowCellsRightMenu(row, row.sku);
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
                                    switch (row.sku.tag) {
                                        case helperService.LoadTag.Loaded:
                                            row.hide = true;
                                            row.sku.status = false;
                                            break;
                                        case helperService.LoadTag.Deleted:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            row.sku.tag = helperService.LoadTag.Empty;
                                            row.sku.status = true;
                                            initRowCellsRightMenu(row, row.sku);
                                            cell.edited = true;
                                            break;
                                        case helperService.LoadTag.Edited:
                                            row.cells.forEach(function (cell) {
                                                cell.styleLoad = null; // remove color
                                            });
                                            initRowCellsRightMenu(row, row.sku);
                                            row.sku.factorUnit = row.sku.oldValue.factorUnit;
                                            row.sku.packSize = row.sku.oldValue.packSize;
                                            row.cells[2].value = row.sku.oldValue.packSize;
                                            row.cells[2].rightMenu.fields[0].cellValue = row.sku.oldValue.packSize;
                                            cell.edited = true;
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

                if (row.sku.tag == helperService.LoadTag.Edited && row.sku.oldValue) {
                    rightMenu.fields.unshift({
                        type: helperService.FieldTypes.Label,
                        properties: {
                            text: "OldValue: [FactorUnit," + row.sku.oldValue.factorUnit + "] [PackSize," + row.sku.oldValue.packSize + "]"
                        }
                    });
                }

                return rightMenu;
            };

            var initRowCellsRightMenu = function (row, currentSku) {
                row.cells[0].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            text: "Name: ",
                            cellValue: currentSku.name,
                            properties: {
                                required: true,
                                focus: true,
                                select: true
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.sku.name = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.sku.name;
                            cell.value = row.sku.name;
                            cell.rightMenu.properties.showReverseEdit = true;
                            canSaveVersion();
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.sku.name = row.originalSku.name;
                            cell.rightMenu.fields[0].cellValue = row.sku.name;
                            cell.value = row.sku.name;
                            cell.edited = false;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };

                row.cells[2].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: "Dosage: ",
                            cellValue: currentSku.dosage,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                allowDecimal: true
                            }
                        },
                        {
                            type: helperService.FieldTypes.Select,
                            text: "Unit: ",                            
                            properties: {
                                class: 't-custom-select-boxed',
                                items: prepareFactorUnitsForFilter(currentSku.factorUnit),
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            var unit = getSelectedUnit(cell.rightMenu.fields[1].properties.items);

                            row.sku.dosage = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.sku.dosage;                            
                            
                            row.sku.factorUnit = unit.value;
                            row.sku.unit = unit.text;

                            cell.value = row.sku.dosage + ' ('+row.sku.unit+')';

                            cell.rightMenu.properties.showReverseEdit = true;
                            canSaveVersion();
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.sku.dosage = row.originalSku.dosage;
                            row.sku.factorUnit = row.originalSku.factorUnit;
                            cell.rightMenu.fields[1].properties.items = prepareFactorUnitsForFilter(row.sku.factorUnit);
                            cell.rightMenu.fields[0].cellValue = row.sku.dosage;
                            cell.value = row.sku.dosage;
                            cell.edited = false;
                            cell.rightMenu.properties.showReverseEdit = false;
                            canSaveVersion();
                        }
                    }
                };

                row.cells[3].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.NumericTextbox,
                            text: "Pack Size: ",
                            cellValue: currentSku.packSize,
                            properties: {
                                required: true,
                                focus: true,
                                select: true,
                                allowDecimal: true
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.sku.packSize = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.sku.packSize;
                            cell.value = row.sku.packSize;
                            cell.rightMenu.properties.showReverseEdit = true;
                            canSaveVersion();
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.sku.packSize = row.originalSku.packSize;
                            cell.rightMenu.fields[0].cellValue = row.sku.packSize;
                            cell.value = row.sku.packSize;
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
                            cellValue: currentSku.formulationId,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: prepareFormulationForCellFilter(currentSku.formulationId),
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            var formulation = _.find(cell.rightMenu.fields[0].properties.items, function (item) { return item.selected; });
                            row.sku.formulationId = fields[0].value;
                            cell.rightMenu.fields[0].properties.items = prepareFormulationForCellFilter(row.sku.formulationId);
                            cell.rightMenu.fields[0].cellValue = row.sku.formulationId;
                            cell.value = formulation.text;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.sku.formulationId = row.originalSku.formulationId;
                            cell.rightMenu.fields[0].properties.items = prepareFormulationForCellFilter(row.sku.formulationId);
                            cell.rightMenu.fields[0].cellValue = row.sku.formulationId;
                            var formulation = _.find(cell.rightMenu.fields[0].properties.items, function (item) { return item.selected; });
                            cell.value = formulation.text;
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
                            cellValue: currentSku.status,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: prepareStatusForEdit(currentSku.status),
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.sku.status = fields[0].value; // check !
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.sku.status);
                            cell.rightMenu.fields[0].cellValue = row.sku.status;
                            cell.value = row.sku.status ? "Active" : "Inactive";
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = true;
                        },
                        showReverseEdit: false,
                        reverseEdit: function (fields, cell, row, table) {
                            row.sku.status = row.originalSku.status;
                            cell.rightMenu.fields[0].properties.items = prepareStatusForEdit(row.sku.status);
                            cell.rightMenu.fields[0].cellValue = row.sku.status;
                            cell.value = row.sku.status ? "Active" : "Inactive";
                            cell.edited = false;
                            canSaveVersion();
                            cell.rightMenu.properties.showReverseEdit = false;
                        }
                    }
                };
            };

            var getSelectedUnit = function (options) {
                var selectedUnit;
                options.forEach(function(opt) {
                    if (opt.selected)
                        selectedUnit = opt;
                });
                return selectedUnit;
            };

            var prepareFactorUnitsForFilter = function (factorUnit) {
                var items = [];
                units.forEach(function(u) {
                    items.push({
                        id: u.id,
                        text: u.name,
                        selected: u.factor == factorUnit,
                        value: u.factor
                    });
                });
                return items;
            };

            var getStatusSelection = function (field) {
                return field.properties.items[0].selected; //items[0] = active
            };

            var prepareRegionsForFilter = function (data) {
                var result = [];

                data.forEach(function (region, i) {
                    var newRegion = {
                        id: region.region.id,
                        text: region.region.name,
                        sname: region.region.name.replace(/\s+/g, ''),
                        items: [],
                        selected: (($scope.lastSettingsChosen.defaultRegionId && $scope.lastSettingsChosen.defaultRegionId == region.region.id)
                            || (!$scope.lastSettingsChosen.defaultRegionId && i == 0)) ? true : false
                    };
                    var countrySelected = false;
                    region.countries.forEach(function (country) {
                        var newCountry = {
                            id: country.id,
                            text: country.name,
                            name: country.name.replace(/\s+/g, '')
                        };

                        if ($scope.lastSettingsChosen.defaultCountryId && $scope.lastSettingsChosen.defaultCountryId == newCountry.id) {
                            newCountry.selected = true;
                            countrySelected = true;
                        }

                        newRegion.items.push(newCountry);
                    });
                    if (!countrySelected && newRegion.selected) {
                        newRegion.items[0].selected = true;
                    }

                    result.push(newRegion);
                });

                return result;
            };

            var filterGeographyForProduct = function () {
                var selectedProductId = getSelectedProductId();
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

                if (getSelectedCountryId() == 0) {
                    countryFilter.properties.items.forEach(function (region) {
                        region.items.forEach(function (country) {
                            country.selected = false;
                        });
                        region.selected = false;
                    });
                    countryFilter.properties.items[0].selected = true;
                    countryFilter.properties.items[0].items[0].selected = true;
                }
            };

            var prepareProductsForFilter = function (productsResponse) {
                var products = [];
                productsResponse.unshift({
                    id: 0,
                    name: "All products",
                    shortName: "All products"
                });

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
                        products[0].selected = true;
                    }
                } else {
                    productsResponse.forEach(function (p, i) {
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
            var prepareFormulationForFilter = function (data) {
                var formulations = [];
                formulations.push({
                    id: 0,
                    text: "All formulations",
                    textShort: "All formulations",
                    selected: true
                });
                data.forEach(function (f, i) {
                    formulations.push({
                        id: f.id,
                        text: f.name,
                        textShort: f.shortname,
                    });
                });

                return formulations;
            };
            var getSelectedCountryId = function () {
                for (var i = 0; i < $scope.filterOptions.header.items[1].properties.items.length; i++) {
                    var country = _.find($scope.filterOptions.header.items[1].properties.items[i].items, function (countryTmp) {
                        return countryTmp.selected == true;
                    });
                    if (country != null) break;
                }

                return (country == null) ? 0 : country.id;
            };
            var getSelectedCountryName = function () {
                for (var i = 0; i < $scope.filterOptions.header.items[1].properties.items.length; i++) {
                    var country = _.find($scope.filterOptions.header.items[1].properties.items[i].items, function (countryTmp) {
                        return countryTmp.selected == true;
                    });
                    if (country != null) break;
                }

                return (country == null) ? '' : country.name;
            };
            var getSelectedStatus = function () {
                var status = _.find($scope.filterOptions.filters[0].properties.items, function (s) {
                    return s.selected == true;
                });
                return status == null ? 0 : status.id;
            };
            var getSelectedProductId = function () {
                var product = _.find($scope.filterOptions.header.items[3].properties.items, function (productTmp) {
                    return productTmp.selected == true;
                });
                return product == null ? 0 : product.id;
            };
            var getSelectedFormulationId = function () {
                var formulation = _.find($scope.filterOptions.filters[1].properties.items, function (form) {
                    return form.selected == true;
                });
                return formulation == null ? 0 : formulation.id;
            };

            var onSearch = function () {
                resetPaging();
                $scope.searchRequest.countryId = getSelectedCountryId();
                $scope.searchRequest.productId = getSelectedProductId();
                $scope.searchRequest.status = getSelectedStatus();
                $scope.searchRequest.formulationId = getSelectedFormulationId();
                $scope.getSkus();
            };
            $scope.getSkus = function () {
                skuService.getSku($scope.searchRequest).then(function (result) {

                    $scope.searchRequest.pageNumber++;

                    if (result) {
                        $scope.skus = $scope.skus.concat(result.skus);

                        $scope.skus.forEach(function (sku) {
                            sku.statusAsString = sku.status ? "Active" : "Inactive";
                        });

                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more skus - " + $scope.skus.length + " out of " + result.totalSkus;
                        initTable();
                    }
                });
            };
            var resetPaging = function () {
                $scope.searchRequest.pageNumber = 0;
                $scope.skus = [];
            };
            $scope.noFilterChosen = function () {
                var countries = getSelectedCountryId();
                var product = getSelectedProductId();

                return (countries == undefined || countries.length == 0) && (product == undefined || product == 0);
            };

            var init = function () {
                var userPromise = userService.getLastSettingsChosen();
                var unitPromise = dimensionService.getUnitTypes();
                var promiseArray = [countryPromise, productPromise, dimensionPromise, userPromise, unitPromise];
                if (validate) {
                    var next = loadService.getNext($routeParams.loadId, helperService.LoadItemNames.Sku);
                    promiseArray.push(next);
                }

                $q.all(promiseArray).then(function (data) {
                    $scope.formulations = data[2];
                    units = data[4];
                    var formulations = prepareFormulationForFilter(data[2]);

                    $scope.filterOptions = {
                        header: {
                            items: [
                                {
                                    type: helperService.FieldTypes.Label,
                                    properties: {
                                        text: "SKU in"
                                    }
                                },
                                countryFilter,
                                {
                                    type: helperService.FieldTypes.Label,
                                    properties: {
                                        text: "for"
                                    }
                                },
                                productFilter
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
                                            { id: 0, text: "All status", textShort: "All status" },
                                            { id: 1, text: "Active", textShort: "Active", selected: true },
                                            { id: 2, text: "Inactive", textShort: "Inactive" }
                                        ],
                                    }
                                },
                                {
                                    type: helperService.FieldTypes.Select,
                                    hide: validate,
                                    name: 'formulations',
                                    properties: {
                                        class: 't-custom-select-boxed',
                                        items: formulations,
                                    }
                                }
                        ],
                        onChanged: function (sourceChanged) {
                            if (sourceChanged == countryFilter || sourceChanged == productFilter) {
                                if (sourceChanged == countryFilter) {
                                    userService.lastSettingsChosen.defaultRegionId = getSelectedRegionId();
                                    userService.lastSettingsChosen.defaultCountryId = getSelectedCountryId();

                                    if (validate) {
                                        countryPromise.then(function (result) {
                                            countryFilter.properties.items = prepareRegionsForFilter(result);
                                            filterGeographyForProduct();
                                            onSearch();
                                        });
                                    }
                                }

                                if (sourceChanged == productFilter) {
                                    var productId = getSelectedProductId();
                                    if (productId != 0)
                                        userService.lastSettingsChosen.defaultProductId = getSelectedProductId();
                                    if (validate) {
                                        onSearch();
                                    }
                                }
                            }
                            if (!validate) {
                                onSearch();
                            }
                        }
                    };
                    if (validate) {
                        usableProductsAndGeographyIds = data[5];
                    } else {
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
                                                }
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function (response) {
                                        $scope.searchRequest.databaseLike = response.databaseLike;
                                        $scope.searchRequest.allCountries = response.allCountries;
                                        $scope.searchRequest.allProducts = response.allProducts;
                                        excelService.postFilterExcel($scope.searchRequest).then(function (result) {
                                            window.location.href = 'api/sku/excel?token=' + result.token;
                                        });
                                    }, function () {

                                    });
                                }
                            }
                        });
                    }

                    $scope.lastSettingsChosen = data[3];
                    productFilter.properties.items = prepareProductsForFilter(data[1]);
                    countryFilter.properties.items = prepareRegionsForFilter(data[0]);

                    if (validate) {
                        filterGeographyForProduct();
                    }

                    onSearch();
                });

            };
            init();

            var getSelectedRegionId = function () {
                var region = _.find(countryFilter.properties.items, function (item) {
                    return item.selected;
                });
                return region.id;
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
                var data = {
                    skus: getEditedRows(),
                    validate: validate,
                    loadId: $routeParams.loadId,
                    countryId: getSelectedCountryId(),
                    productId: getSelectedProductId()
                };
                if (validate || data.skus.length > 0) {
                    skuService.save(data).then(function (result) {
                        if (validate) {
                            if (result == "true") {
                                $location.path('/data/load/' + $routeParams.loadId);
                            } else {
                                init();
                            }                                
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
                            editedRows.push($scope.table.rows[i].sku);
                            break;
                        }
                    }
                }
                return editedRows;
            };

        }
    ]);
});