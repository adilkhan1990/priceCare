define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('AddSkuModalController', ['$scope', '$modalInstance', 'dimensionService', 'helperService',
        function ($scope, $modalInstance, dimensionService, helperService) {

            $scope.sku = {};
            $scope.formulationOptions = {
                type: helperService.FieldTypes.Select,
                name: 'formulations',
                properties: {
                    class: 't-custom-select-boxed',

                }
            };

            $scope.factorUnitOptions = {
                type: helperService.FieldTypes.Select,
                name: 'factorUnit',
                properties: {
                    class: 't-custom-select-boxed',

                }
            };
            $scope.factorUnitOptions.properties.items = [{
                id: 1,
                text: "mg",
                textShort: "mg",
                selected: true
            },
            {
                id: 1000,
                text: "mcg",
                textShort: "mcg",
                selected: false
            }];

            var getSelectedFactorUnit = function () {
                var factor = _.find($scope.factorUnitOptions.properties.items, function (item) {
                    return item.selected == true;
                });
                return factor == null ? 0 : factor.id;
            };

            var canSave = function() {
                if ($scope.sku.name && $scope.sku.productNumber && $scope.sku.dosage && $scope.sku.packSize) {
                    return true;
                } else {
                    return false;
                }
            };

            var getSelectedFormulation = function () {
                var result;
                $scope.formulationOptions.properties.items.forEach(function (item) {
                    if (item.selected)
                    {
                        result = item;
                    }
                        
                });
                return result;
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

            $scope.ok = function () {
                if (canSave()) {
                    $scope.sku.formulationId = getSelectedFormulation().id;
                    $scope.sku.factorUnit = getSelectedFactorUnit();
                    $modalInstance.close($scope.sku);
                }                    
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            }

            var init = function () {
                dimensionService.getFormulationTypes().then(function (result) {
                    var formulations = prepareFormulationForFilter(result);

                    //set formula options for new sku
                    var options = formulations;
                    options.shift();
                    options[0].selected = true;

                    $scope.formulationOptions.properties.items = options;                    
                });
            };
            init();

    }]);
});