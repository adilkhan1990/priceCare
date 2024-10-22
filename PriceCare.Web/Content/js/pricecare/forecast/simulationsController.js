define(['pricecare/forecast/module'], function (module) {
    'use strict';
    module.controller('SimulationsController', ['$scope', '$q', 'helperService', 'simulationService',
        function ($scope, $q, helperService, simulationService) {

            var statusFilter = {
                type: helperService.FieldTypes.Select,
                name: 'status',
                properties: {
                    class: 't-custom-select-boxed',
                    items: [                        
                        { text: "Active", id:1, selected: true },
                        { text: "Inactive", id:0 }
                    ]
                }
            };

            var simulationFilter = {
                type: helperService.FieldTypes.Select,
                name: 'simulation',
                properties:
                {
                    class: 't-custom-select-text',
                    items: [
                    {
                        id: helperService.SimulationTypes.Budget,
                        text: "Budget"
                    },
                    {
                        id: helperService.SimulationTypes.Reference,
                        text: "Reference",
                        selected: true
                    },
                    {
                        id: helperService.SimulationTypes.Public,
                        text: "Public"
                    },
                    {
                        id: helperService.SimulationTypes.User,
                        text: "User"
                    }
                    ]
                }                
            };

            $scope.filterOptions = {
                header: {
                    items: [
                    {
                        type: helperService.FieldTypes.Label,
                        properties: { text: "Simulations for "}
                    },
                    simulationFilter
                    ],                    
                },
                filters: [statusFilter],
                onChanged: function () {
                    getSimulations();
                }
            };

            var getSelectedSimulationType = function() {
                var simulationType;
                simulationFilter.properties.items.forEach(function(st) {
                    if (st.selected)
                        simulationType = st;
                });
                return simulationType;
            };

            var getStatusCell = function (simulation) {

                return {
                    value: simulation.active ? "Active" : "Inactive",
                    rightMenu: {
                        fields: [
                            {
                                type: helperService.FieldTypes.Select,
                                cellValue: simulation.active,
                                properties: {
                                    class: 't-custom-select-boxed',
                                    items: [
                                    {
                                        id: 1,
                                        text: "Active",
                                        selected: simulation.active
                                    },
                                    {
                                        id: 0,
                                        text: "Inactive",
                                        selected: !simulation.active
                                    }]
                                }
                            }],
                        properties: {
                            apply: function(fields, cell, row, table) {
                                row.simulation.active = fields[0].value;
                                cell.rightMenu.fields[0].cellValue = row.simulation.active;
                                cell.value = row.simulation.active ? "Active" : "Inactive";
                                updateSimulation(simulation);
                            },
                            showReverseEdit: false
                        }
                    }
                }
            };

            var setTable = function (data) {
                var columnNames = ["Name", "Comment", "Date", "Status"];
                var headerCells = [];
                var rows = [];

                columnNames.forEach(function(name) {
                    headerCells.push({ title: true, value: name });
                });

                rows.push({ title: true, cells: headerCells });

                for (var i = 0; i < $scope.simulations.length; i++) {
                    var cells = [
                        { value: $scope.simulations[i].name },
                        { value: $scope.simulations[i].comment},
                        { value: $scope.simulations[i].saveTime },
                        getStatusCell($scope.simulations[i])
                    ];
                   
                    rows.push({
                        simulation: $scope.simulations[i],
                        cells: cells
                    });
                }

                $scope.table = {
                    rows: rows
                }
            };

            var updateSimulation = function(simulation) {
                simulationService.updateSimulation(simulation).then(function(result) {
                    getSimulations();
                });
            };

            var getSelectedStatus = function() {
                return statusFilter.properties.items[0].selected; 
            };

            var getSimulations = function () {
                var isActive = getSelectedStatus();
                var simulationType = getSelectedSimulationType();
                var simulationPromise = simulationService.getSimulationsByType(simulationType.id, isActive);

                $q.all([simulationPromise]).then(function (data) {
                    $scope.simulations = data[0];
                    setTable($scope.simulations);
                });
            };

            var init = function() {
                getSimulations();
            };
            init();
        }
    ]);
});