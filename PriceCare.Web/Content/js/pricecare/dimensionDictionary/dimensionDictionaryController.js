define(['pricecare/dimensionDictionary/module'], function (module) {
    'use strict';
    module.controller('DimensionDictionaryController', ['$scope', 'helperService', 'productService', 'countryService', '$q', 'dimensionDictionaryService', '$modal', '$location',
        function ($scope, helperService, productService, countryService, $q, dimensionDictionaryService, $modal, $location) {

            $scope.dimensionDictionaries = [];

            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage
            };

            $scope.$watch(function () { return $location.search(); }, function (newVal, oldVal) {
                var dimensionType = $location.search().dimensionType;
                if (!dimensionType || (dimensionType != dimensionDictionaryService.DimensionType.Geography && dimensionType != dimensionDictionaryService.DimensionType.Product)) {
                    $scope.searchRequest.dimensionType = dimensionDictionaryService.DimensionType.Geography;
                } else {
                    $scope.searchRequest.dimensionType = dimensionType;
                }
                onSearch();
            });

            var dimensionTypeFilter = {
                type: helperService.FieldTypes.Select,
                name: 'dimensionType',
                properties: {
                    class: 't-custom-select-text'
                }
            };

            var getSelectedDimensionTypeId = function() {
                var dimensionType = _.find(dimensionTypeFilter.properties.items, function(item) { return item.selected; });
                return dimensionType.id;
            };

            var resetPaging = function() {
                $scope.dimensionDictionaries = [];
                $scope.searchRequest.pageNumber = 0;
            };

            $scope.filterOptions = {
                header: {
                    items: [
                         {
                             type: helperService.FieldTypes.Label,
                             properties:
                             {
                                 text: "Dimension Dictionary For"
                             }
                         },
                        dimensionTypeFilter
                    ],
                },
                showAdvancedFilters: false,
                onChanged: function (sourceChanged) {
                    if (sourceChanged == dimensionTypeFilter) {
                        var dimensionType = getSelectedDimensionTypeId();
                        $location.search({ dimensionType: dimensionType });
                    }

                }
            };

            var onSearch = function () {
                dimensionTypeFilter.properties.items = dimensionDictionaryService.prepareDimensionTypeForFilter($scope.searchRequest.dimensionType);
                var promise = ($scope.searchRequest.dimensionType == dimensionDictionaryService.DimensionType.Product) ? productService.getAllProducts() : countryService.getAllCountries();
                promise.then(function(data) {
                    $scope.dimensionTypes = data;
                    resetPaging();
                    $scope.getDimensionDictionaries();
                });
            };

            $scope.getDimensionDictionaries = function () {
                var allDimension = dimensionDictionaryService.getAllByDimension($scope.searchRequest);
                var allGcods = dimensionDictionaryService.getAllGcodsForDimension($scope.searchRequest);
                $q.all([allDimension, allGcods]).then(function (data) {
                    $scope.allDimension = data[0].dimensionDictionary;
                    if (data[1]) {
                        $scope.dimensionDictionaries = $scope.dimensionDictionaries.concat(data[1].dimensionDictionary);
                        $scope.canLoadMore = !data[1].isLastPage;
                        $scope.counter = "Load more dimension dictionaries - " + $scope.dimensionDictionaries.length + " out of " + data[1].totalRows;
                        initTable($scope.dimensionDictionaries);
                    }
                    canSaveVersion();
                    $scope.searchRequest.pageNumber++;

                });
            };

            $scope.delete = function(synonym) {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/yesNoCancelModal.html',
                    controller: 'YesNoCancelModalController',
                    backdrop: 'static',
                    resolve: {
                        infos: function() {
                            return {
                                title: 'Warning',
                                content: 'Are you sure you want to delete this '
                            }
                        }
                    }
                });

                modalInstance.result.then(function(confirm) {
                    if (confirm) {
                        dimensionDictionaryService.deleteSynonym(synonym).then(function () {
                            window.location.reload();
                        });
                    }
                });
            };

            var initTable = function () {
                var columnNames = ["Default Name", "Synonym"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                var dimensionGroups = _.groupBy($scope.dimensionDictionaries, 'dimensionId');
                dimensionGroups = _.sortBy(dimensionGroups, function(item) { return item[0].name; }); // default asc

                for (var indice in dimensionGroups) {
                    var defaultDimension = _.find(dimensionGroups[indice], function (item) { return item.systemId == 0; }); // 0 means GCODS, it's the default name
                    if (!defaultDimension)continue;
                    var synonyms = _.filter(dimensionGroups[indice], function (item) { return item.systemId == 16; });
                    //if (!synonyms) continue;
                    var synonymRows = [];
                    var dimensionRow = {
                        cells: [
                            {
                                value: defaultDimension.name + "(" + synonyms.length + ")",
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
                            },
                            { value: "" }
                        ]
                    };
                    if (synonyms) {
                        for (var i = 0; i < synonyms.length; i++) {
                            synonymRows.push({
                                parentRow: dimensionRow,
                                oldSynonym: helperService.clone(synonyms[i]),
                                synonym: synonyms[i],
                                cells: [
                                    { value: "" },
                                    {
                                        value: synonyms[i].name,
                                        actions: [
                                            {
                                                text: 'delete',
                                                class: 'icon icon-delete',
                                                click: function(cell, row) {
                                                    $scope.delete(row.synonym);
                                                }
                                            }
                                        ]                                       
                                    }
                                ]
                            });
                        }
                    }
                    
                    rows.push(dimensionRow);
                    rows = rows.concat(synonymRows);
                }

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getDimensionDictionaries();
                        },
                        counterText: $scope.counter
                    }
                };
            };

            $scope.addSynonym = function() {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/addSynonymModal.html',
                    controller: 'AddSynonymModalController',
                    backdrop: 'static',
                    resolve: {
                        infos: function () {
                            return {
                                dimensionTypeChosen: getSelectedDimensionTypeId()
                            }
                        }
                    }
                });

                modalInstance.result.then(function (result) {
                    onSearch();

                }, function () {

                });
            };

            var getEditedRows = function () {
                var rows = [];

                for (var i = 1; i < $scope.table.rows.length; i++) { // avoid first row and cumulative row
                    for (var j = 1; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            rows.push($scope.table.rows[i].synonym);
                            break;
                        }
                    }
                }

                return rows;
            };

            var canSaveVersion = function () {
                $scope.canSave = false;

                for (var i = 0; i < $scope.table.rows.length; i++) {
                    for (var j = 0; j < $scope.table.rows[i].cells.length; j++) {
                        if ($scope.table.rows[i].cells[j].edited) {
                            $scope.canSave = true;
                            break;
                        }
                    }
                }
            };

            $scope.saveVersion = function () {
                var datas = getEditedRows();
                if (datas.length > 0)
                    dimensionDictionaryService.update(datas).then(function() {
                        onSearch();
                    });
            };
       }]);
});