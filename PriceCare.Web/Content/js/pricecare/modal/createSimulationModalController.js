define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('CreateSimulationModalController', ['$scope', '$modalInstance', 'simulationService', 'helperService', 'infos',
        function ($scope, $modalInstance, simulationService, helperService, infos) {

            // Model
            $scope.model = {
                simulationDuration: 1
            };

            // Validation
            $scope.isValidDuration = true;

            var init = function () {
                $scope.currencyOptions = helperService.getCurrencyFilter('t-custom-select-boxed');
                $scope.currencyOptions.properties.items.shift(); // remove first item
                $scope.currencyOptions.properties.items[0].selected = true;
                $scope.currencyOptions.text = "Currency: ";
                $scope.model.simulationCurrencyId = $scope.currencyOptions.properties.items[0].id;

                $scope.scenarioOptions = {
                    type: helperService.FieldTypes.Select,
                    name: "scenario: ",
                    text: 'Assumption scenario',
                    properties: {
                        class: 't-custom-select-boxed'
                    }
                };

                simulationService.getAssumptionsScenarios().then(function (scenarios) {
                    $scope.scenarioOptions.properties.items = prepareScenarios(scenarios);
                    $scope.model.assumptionsSaveId = $scope.scenarioOptions.properties.items[0].id;
                });
            };

            init();

            $scope.ok = function () {
                $scope.isValidDuration = $scope.model.simulationDuration && $scope.model.simulationDuration > 0 && $scope.model.simulationDuration < 11;
                if ($scope.isValidDuration) {
                    $scope.model.assumptionsSaveId = getSelectedAssumptionScenario();
                    $scope.model.simulationCurrencyId = getSelectedCurrency();
                    $scope.model.productId = infos.productId;

                    $modalInstance.close($scope.model);

                }
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

            var getSelectedCurrency = function() {
                var currency = _.find($scope.currencyOptions.properties.items, function (item) { return item.selected; });
                return currency.id;
            };

            var getSelectedAssumptionScenario = function() {
                var scenario = _.find($scope.scenarioOptions.properties.items, function (item) { return item.selected });
                return scenario.id;
            };

            var prepareScenarios = function (datas) {
                var result = [];

                datas.forEach(function (data, i) {
                    result.push({
                        id: data.saveId,
                        text: data.name,
                        selected: i == 0
                    });
                });

                return result;
            };

        }]);
});