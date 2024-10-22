define(['pricecare/forecast/module'], function (module) {
    'use strict';
    module.controller('LaunchForecastController', ['$scope', '$rootScope', '$controller', '$modal', 'helperService', 'volumeService', 'eventService', 'simulationService', 'userService',
        function ($scope, $rootScope, $controller, $modal, helperService, volumeService, eventService, simulationService, userService) {
            $scope.status = {
                templateDownloaded: false,
                dataReady: false,
                launchReady: false,
                volumeReady: false,
                eventReady: false
            }
            var basisSimulation = null;
            var currentSimulation = null;
            
            $scope.initializeSimulation = function () {
                $scope.status.dataReady = false;
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/launchDownloadAssumptionsModal.html',
                    controller: 'launchDownloadAssumptionsModalController',
                    backdrop: 'static'
                });

                modalInstance.result.then(function(data) {
                    basisSimulation = {
                        productId: [data.productId],
                        saveId: data.assumptionsSaveId,
                        duration: data.simulationDuration
                    };
                    var searchRequest = {
                        assumptionsSaveId: basisSimulation.saveId,
                        simulationDuration: basisSimulation.duration,
                        simulationCurrencyId: 1,
                        productId: 15,
                        isLaunch: true
                    };
                    simulationService.create(searchRequest).then(function (data2) {
                        currentSimulation = data2;
                    }).then(function () {
                        var searchRequest2 = {
                            productId: basisSimulation.productId,
                            versionId: currentSimulation.dataVersionId,
                            allCountries: true,
                            allProducts: false,
                            geographyIds: [],
                            simulationId: currentSimulation.id,
                            scenarioTypeId: basisSimulation.saveId,
                            userId: currentSimulation.userId,
                            duration: basisSimulation.duration
                        };
                        simulationService.postFilterExcel(searchRequest2).then(function (result) {
                            window.location.href = 'api/simulation/launch/excel?token=' + result.token;
                            updateStatusDownload();
                            getSimulations();
                        });
                    });
                });
            }

            $scope.checkScenarioLoad = function() {
                simulationService.checkScenarioLoad().then(function (result) {
                    if (result.item1) {
                        $scope.status.launchReady = true;
                    };
                    if (result.item2) {
                        $scope.status.volumeReady = true;
                    };
                    if (result.item3) {
                        $scope.status.eventReady = true;
                    };
                    updateStatusReady();
                });
            }

            var updateStatusDownload = function () {
                $scope.status.launchReady = false;
                $scope.status.volumeReady = false;
                $scope.status.eventReady = true;
                $scope.status.dataReady = false;
                $scope.status.templateDownloaded = true;
            }
            var updateStatusReady = function() {
                $scope.status.dataReady = $scope.status.launchReady && $scope.status.volumeReady && $scope.status.eventReady;
            }

            var getSelectedSaveId = function () {
                for (var i = 0; i < simulationFilter.properties.items.length; i++) {
                    var save = _.find(simulationFilter.properties.items[i].items, function (saveTmp) {
                        return saveTmp.selected == true;
                    });
                    if (save != null) break;
                }

                return (save == null) ? 0 : save.id;
            };
            var loadCacheAndReloadData = function () {
                var saveId = getSelectedSaveId();
                if (saveId && saveId != 0) {
                    simulationService.loadSimulation(saveId, [15]).then(function() {
                        $scope.status.launchReady = true;
                        $scope.status.volumeReady = true;
                        $scope.status.eventReady = true;
                        $scope.status.templateDownloaded = false;
                        $scope.status.dataReady = true;
                    });
                }
            };

            var simulationFilter = {
                type: helperService.FieldTypes.SelectMultiLevel,
                name: 'simulations',
                properties: {
                    class: 't-custom-select-text',
                    items: []
                }
            };
            var getSimulations = function () {
                simulationService.getLaunchSimulations().then(function (data) {
                    simulationFilter.properties.items = simulationService.prepareSimulationForFilter(data);
                    simulationService.getFirstSimulation().then(function(result) {
                        changeSelectedSaveId(result.saveId);
                        loadCacheAndReloadData();
                    });
                });
            };
            var init = function() {
                getSimulations();
                $scope.filterOptions = {
                    header: {
                        items: [
                            {
                                type: helperService.FieldTypes.Label,
                                properties:
                                {
                                    text: "Load Existing Launch Simulation"
                                }
                            },
                            simulationFilter
                        ],
                    },
                    onChanged: function(sourceChanged) {

                        if (sourceChanged == simulationFilter) {
                            loadCacheAndReloadData();
                            
                        }
                    }
                }
            }
            init();
            var changeSelectedSaveId = function (saveId) {
                var parentSimulation = simulationFilter.properties.items;
                parentSimulation.forEach(function (parent) {
                    var childFound = false;
                    for (var i = 0; i < parent.items.length; i++) {
                        parent.items[i].selected = parent.items[i].id == saveId;
                        if (!childFound) childFound = parent.items[i].id == saveId;
                    }
                    parent.selected = childFound;
                });
            };

        }]);
});