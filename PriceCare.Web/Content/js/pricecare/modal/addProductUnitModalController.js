define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('AddProductUnitModalController', ['$scope', '$modalInstance', 'helperService', 'items', 
        function ($scope, $modalInstance, helperService, items) {


            $scope.productName = items.productName;
            $scope.model = {
                productId: items.productId,
                active: true,
                isDefault: false,
                factorScreen: 1
            };

            $scope.unitOptions = {
                type: helperService.FieldTypes.Select,
                name: 'units',
                properties: {
                    class: 't-custom-select-boxed',
                    items: items.unitOptions
                }
            }
            
            $scope.ok = function () {
                $scope.model.unitId = getSelectedUnit();
                if($scope.model.factorScreen)
                    $modalInstance.close($scope.model);
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            }

            var getSelectedUnit = function() {
                var unitId;
                $scope.unitOptions.properties.items.forEach(function(option) {
                    if (option.selected)
                        unitId = option.id;
                });
                return unitId;
            };

            //var init = function () {

            //};
            //init();

        }]);
});