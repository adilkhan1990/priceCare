define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('launchDownloadAssumptionsModalController', ['$scope', '$modalInstance', 'simulationService', 'helperService', 'productService',
        function ($scope, $modalInstance, simulationService, helperService, productService) {

            // Model
            $scope.model = {
                simulationDuration: 1
            };

            // Validation
            $scope.isValidDuration = true;

            var init = function () {

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
                });

                $scope.productOptions = {
                    type: helperService.FieldTypes.Select,
                    name: "products: ",
                    text: 'Products',
                    properties: {
                        class: 't-custom-select-boxed'
                    }
                };

                productService.getAllProducts().then(function (products) {
                    $scope.productOptions.properties.items = prepareProductsForFilter(products);
                });
            };

            init();

            $scope.ok = function () {
                $scope.isValidDuration = $scope.model.simulationDuration && $scope.model.simulationDuration > 0 && $scope.model.simulationDuration < 11;
                if ($scope.isValidDuration) {
                    $modalInstance.close({
                        assumptionsSaveId: getSelectedAssumptionScenario(),
                        productId: getSelectedProductId(),
                        simulationDuration: $scope.model.simulationDuration
                
                    });
                }
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

            var prepareProductsForFilter = function (data) {
                var products = [];

                data.forEach(function (p, i) {
                    products.push({
                        id: p.id,
                        text: p.name,
                        textShort: p.shortname,
                        selected: i==0
                    });

                });

                return products;
            };

            var getSelectedProductId = function () {
                var product = _.find($scope.productOptions.properties.items, function (item) { return item.selected; });
                return product.id;
            };

            var getSelectedAssumptionScenario = function () {
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