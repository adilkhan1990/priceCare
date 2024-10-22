define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opSelectionTreePopupLimited', [
        function () {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opSelectionTreePopupLimitedTemplate.html",
                scope: {
                    options: "=",
                    onApply: '&'
                },
                controller: function ($scope, $element) {
                    $scope.options.checkAtLeastOneItemSelected = function () {
                        $scope.atLeastOneSelected = _.any($scope.options.properties.items, function (item) {
                            return _.any(item.items, function (it) {
                                return it.value == true;
                            });
                        });
                    };

                    var elementRect = $element[0].getBoundingClientRect();
                    var parentRect = $scope.options.parentRect;

                    var offset = parentRect.left - elementRect.left + (parentRect.width / 2);

                    offset = offset + 'px';
                    $scope.getArrowStyle = function () {
                        return {
                            left: offset
                        };
                    };

                    $scope.dropStyle = function () {

                        var filter = document.getElementById("m-filter-control");
                        var width = filter.offsetWidth;

                        var baseFontSize = parseInt(getComputedStyle(document.body).fontSize);

                        var finalWidth = (2.35 * baseFontSize) + width;

                        return {
                            'width': finalWidth + 'px'
                        };
                    };
                    
                    $scope.onApplyFilters = function () {
                        if ($scope.onApply) {
                            if ($scope.atLeastOneSelected) {
                                $scope.onApply();
                            }
                        }
                    };

                }
            }
        }
    ]);
});