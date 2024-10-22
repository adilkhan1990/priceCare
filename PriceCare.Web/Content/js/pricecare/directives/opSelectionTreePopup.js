define(['pricecare/directives/module'], function(module) {
    'use strict';
    module.directive('opSelectionTreePopup', [
        function() {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opSelectionTreePopupTemplate.html",
                scope: {
                    options: "=",
                    onApply: '&'
                },
                controller: function($scope, $element) {

                    var elementRect = $element[0].getBoundingClientRect();
                    var parentRect = $scope.options.parentRect;

                    var offset = parentRect.left - elementRect.left + (parentRect.width / 2);

                    offset = offset + 'px';
                    $scope.getArrowStyle = function() {
                        return {
                            left: offset
                        };
                    };

                    $scope.dropStyle = function() {

                        var filter = document.getElementById("m-filter-control");
                        var width = filter.offsetWidth;

                        var baseFontSize = parseInt(getComputedStyle(document.body).fontSize);

                        var finalWidth = (2.35 * baseFontSize) + width;

                        return {
                            'width': finalWidth + 'px'
                        };
                    };

                    $scope.onApplyFilters = function() {
                        if ($scope.onApply) {
                            $scope.onApply();
                        }
                    };
                }
            }
        }
    ]);
});