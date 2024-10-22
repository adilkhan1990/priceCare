define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opSelectionTreeLimited', [
        function () {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opSelectionTreeLimitedTemplate.html",
                scope: {
                    options: "="
                },
                controller: function ($scope, $element, $attrs) {

                    $scope.options.checkAtLeastOneItemSelected();

                    $scope.upItem = _.find($scope.options.properties.items, function(up) {
                        return up.value == true;
                    });

                    $scope.onValueChanged = function (item) {
                        $scope.options.properties.items.forEach(function (region) {
                            region.value = (region.id != item.id) ? null : item;
                            region.items.forEach(function (country) {
                                country.value = (region.id != item.id) ? false : true; 
                            });
                        });
                    };

                    $scope.selectAllChildren = function(item) {
                        item.items.forEach(function(it) {
                            it.value = true;
                        });
                        $scope.options.checkAtLeastOneItemSelected();
                    };

                    $scope.deSelectAllChildren = function (item) {
                        item.items.forEach(function (it) {
                            it.value = false;
                        });
                        $scope.options.checkAtLeastOneItemSelected();
                    };
                }
            }
        }
    ]);
});