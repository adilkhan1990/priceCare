define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('simulationService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getSimulations = function() {
            var deferred = $q.defer();

            $http.get('api/simulation/all/').success(function(simulations) {
                deferred.resolve(simulations);
            });
            return deferred.promise;
        };
        me.getLaunchSimulations = function () {
            var deferred = $q.defer();

            $http.get('api/simulation/launch/').success(function (simulations) {
                deferred.resolve(simulations);
            });
            return deferred.promise;
        };

        me.getSimulationsByType = function(simulationType, isActive) {
            var deferred = $q.defer();

            $http.get('api/simulation/' + simulationType+'/'+isActive).success(function(simulations) {
                deferred.resolve(simulations);
            });
            return deferred.promise;
        }

        me.loadSimulation = function (saveId, productId) {
            var deferred = $q.defer();

            $http.get('api/simulation/load/'+saveId + '/' + productId).success(function (simulations) {
                deferred.resolve(simulations);
            });
            return deferred.promise;
        };

        me.getAssumptionsScenarios = function () {
            var deferred = $q.defer();

            $http.get('api/simulation/assumptionsScenarios').success(function (simulations) {
                deferred.resolve(simulations);
            });
            return deferred.promise;
        };

        me.create = function (data) {
            var deferred = $q.defer();

            $http.post('api/simulation/create', data).success(function (simulations) {
                deferred.resolve(simulations);
            });
            return deferred.promise;
        };

        me.saveSimulation = function(data) {
            var deferred = $q.defer();

            $http.post('api/simulation/save', data).success(function(result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        }


        me.getFirstSimulation = function () {
            var deferred = $q.defer();
            $http.get('api/simulation/first').success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.updateSimulation = function(simulation) {
            var deferred = $q.defer();
            $http.post('api/simulation/update', simulation).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.updateSimulationCache = function (simulation) {
            var deferred = $q.defer();
            $http.post('api/simulation/updateSimulation', simulation).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.prepareSimulationForFilter = function (data) {
            var result = [];

            data.forEach(function (simulationType, i) {
                var newSimulationType = {
                    id: simulationType.name,
                    text: simulationType.name,
                    sname: simulationType.name.replace(/\s+/g, ''),
                    items: [],
                    selected: i == 0
                };

                simulationType.simulations.forEach(function (simulation, j) {
                    var newSimulation = {
                        id: simulation.saveId,
                        text: simulation.name,
                        name: simulation.name.replace(/\s+/g, ''),
                        startTime: simulation.startTime,
                        isLaunch: simulation.isLaunch,
                        selected: i == 0 && j == (simulationType.simulations.length - 1) // last budget
                    };

                    newSimulationType.items.push(newSimulation);
                });

                result.push(newSimulationType);
            });

            return result;
        };

        me.prepareSimulationCompareForFilter = function (data) {
            var result = [];

            data.forEach(function (simulationType, i) {
                var newSimulationType = {
                    id: simulationType.name,
                    text: simulationType.name,
                    sname: simulationType.name.replace(/\s+/g, ''),
                    items: [],
                };

                simulationType.simulations.forEach(function (simulation, j) {
                    var newSimulation = {
                        id: simulation.saveId,
                        text: simulation.name,
                        name: simulation.name.replace(/\s+/g, ''),
                        startTime: simulation.startTime,
                    };

                    newSimulationType.items.push(newSimulation);
                });

                result.push(newSimulationType);
            });

            return result;
        };
        me.postFilterExcel = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/simulation/postFilterExcel', searchRequest).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };
        me.checkScenarioLoad = function() {
            var deferred = $q.defer();
            $http.post('api/simulation/launch/checkScenarioLoad').success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };

        me.createAssumptionsScenario = function(saveModel) {
            var deferred = $q.defer();
            $http.post('api/simulation/createAssumptionsScenario', saveModel).success(function(saveId) {
                deferred.resolve(saveId);
            });
            return deferred.promise;
        };

        me.isValid = function (saveModel) {
            var deferred = $q.defer();
            $http.post('api/simulation/isValid', saveModel).success(function (saveId) {
                deferred.resolve(saveId);
            });
            return deferred.promise;
        }

    }]);
});