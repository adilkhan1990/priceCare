define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('AddCountryModalController', ['$scope', '$modalInstance', 'countryService', 'helperService',
        function ($scope, $modalInstance, countryService, helperService) {

            $scope.country = {};

            $scope.fieldOptions = {
                type: helperService.FieldTypes.Select,
                name: 'country',
                properties: {
                    class: 't-custom-select-boxed',
                }
            }

            var getSelectedCountry = function () {
                return _.find($scope.fieldOptions.properties.items, function (country) {
                    return country.selected;
                });
            };

            $scope.ok = function () {
                $scope.country = getSelectedCountry();
                $modalInstance.close($scope.country);
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            }

            var init = function () {
                countryService.getAllCountries().then(function (result) {
                    result.forEach(function (country) {
                        country.text = country.name;
                    });
                    result.shift();
                    result[0].selected = true;

                    $scope.fieldOptions.properties.items = result;
                });
            };
            init();

        }]);
});