define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('AddPriceTypeModalController', ['$scope', '$modalInstance', 'currencyService', 'helperService', 'priceTypeService',
        function ($scope, $modalInstance, currencyService, helperService, priceTypeService) {

            $scope.priceType = {};

            // Validation
            $scope.isUniqueShortName = true;
            $scope.isRequiredShortName = true;
            $scope.isRequiredName = true;

            $scope.currencyOptions = {
                type: helperService.FieldTypes.Select,
                name: 'currency',
                properties: {
                    class: 't-custom-select-boxed',
                }
            }
            
            var getSelectedCurrencyId = function () {
                var id = 0;
                $scope.currencyOptions.properties.items.forEach(function (currency) {
                    if (currency.selected)
                        id = currency.id;
                });
                return id;
            };

            $scope.ok = function () {
                $scope.isRequiredShortName = $scope.priceType.shortName && $scope.priceType.shortName != "";
                $scope.isRequiredName = $scope.priceType.name && $scope.priceType.name != "";
                if ($scope.isRequiredName && $scope.isRequiredShortName) {
                    priceTypeService.isValid($scope.priceType).then(function (result) {
                        $scope.isUniqueShortName = result == "true" ? true : false;
                        if (result == "true") {
                            $scope.priceType.currencyId = getSelectedCurrencyId();
                            $modalInstance.close($scope.priceType);
                        }
                    });
                }
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            }

            var init = function () {
                currencyService.getCurrenciesForFilter().then(function(result) {
                    var currencyOptions = result;
                    currencyOptions.shift();
                    currencyOptions[0].selected = true;

                    $scope.currencyOptions.properties.items = currencyOptions;
                });                
            };
            init();

        }]);
});