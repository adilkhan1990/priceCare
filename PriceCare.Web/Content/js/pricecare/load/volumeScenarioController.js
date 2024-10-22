define(['pricecare/load/module'], function (module) {
    'use strict';
    module.controller('VolumeScenarioController', ['$scope', 'helperService', '$q', 'loadService', '$timeout',
        function ($scope, helperService, $q, loadService, $timeout) {

            $scope.volumeScenario = [];
            $scope.canSave = false;

            $scope.filterOptions = {
                header: {
                    items: [
                         {
                             type: helperService.FieldTypes.Label,
                             properties:
                             {
                                 text: "Volume scenarios"
                             }
                         },
                    ],
                },
                showAdvancedFilters: false,
                onChanged: function (sourceChanged) {
                   onSearch();
                }
            };

            $scope.saveVersion = function () {
                if ($scope.canSave) {
                    var editedData = _.pluck(_.filter($scope.table.rows, function(r) {
                        return r.cells[1].edited;
                    }), 'scenario');
                    loadService.updateVolumeScenario(editedData);
                }
            };


            var init = function(){
                loadService.getVolumeScenario().then(function (data) {
                    if (data) {
                        var ordered = _.sortBy(data, function (volumeScenario) { return volumeScenario.weight; });
                        $scope.volumeScenario = ordered;
                        initTable();
                    }
                    
                });
            }
            
            init();


            var initTable = function () {
                var columnNames = ["Scenario", "Weight"];
                var rows = [{ title: true, cells: [] }];
                columnNames.forEach(function (name) {
                    rows[0].cells.push({ title: true, value: name });
                });

                for (var i = 0; i < $scope.volumeScenario.length; i++)
                {
                    var nameCell = {
                        value: $scope.volumeScenario[i].name
                    };
                    var weightCell = {
                        value: $scope.volumeScenario[i].weight
                    };

                    var newRow = { scenario: $scope.volumeScenario[i], cells: [nameCell, weightCell] };
                    initRowCellsRightMenu(newRow);
                    rows.push(newRow);
                    
                }

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: false,
                        getData: function () {
                        },
                        counterText: $scope.volumeScenario.length + ' scenarios'
                    }
                };
            };

            var initRowCellsRightMenu = function (row) {

                row.cells[1].rightMenu = {
                    fields: [
                        {
                            type: helperService.FieldTypes.Textbox,
                            cellValue: row.scenario.weight,
                            properties: {
                                required: true,
                                focus: true,
                                select: true
                            }
                        }
                    ],
                    properties: {
                        apply: function (fields, cell, row, table) {
                            row.scenario.weight = fields[0].value;
                            cell.rightMenu.fields[0].cellValue = row.scenario.weight;
                            cell.value = row.scenario.weight;
                            for (var i = 0; i < $scope.table.rows.length; i++) {
                                if (fields[0].value == $scope.table.rows[i].cells[1].value && $scope.table.rows[i] != row) {
                                    $scope.table.rows[i].scenario.weight = 0;
                                    $scope.table.rows[i].cells[1].value = 0;
                                    $scope.table.rows[i].cells[1].rightMenu.fields[0].cellValue = 0;
                                } 
                            }
                            canSaveVersion();
                            $timeout(function() {
                                var rowsOrdered = _.sortBy($scope.table.rows, function (volumeScenario, i) {
                                    return (i != 0) ? volumeScenario.scenario.weight : false;
                                });
                                $scope.table.rows = rowsOrdered;
                            });
                            
                        },
                        showReverseEdit: false
                    }

                }
                
            };

            var canSaveVersion = function() {
                $scope.canSave = false;
                var nbRowsEdited = 0;
                var nbRowsWithNoValues = 0;
                for (var i = 0; i < $scope.table.rows.length; i++) {
                    nbRowsEdited += $scope.table.rows[i].cells[1].edited ? 1 : 0;
                    nbRowsWithNoValues += $scope.table.rows[i].cells[1].value == 0 ? 1 : 0;
                }
                $scope.canSave = nbRowsEdited > 0;// && nbRowsWithNoValues == 0;
            };

        }]);
});