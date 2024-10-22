define(['pricecare/directives/flyout/module'], function(module) {
    'use strict';
    module.directive('opInfoFlyOut', [
        '$document', 'helperService', '$q', '$sce',
        function($document, helperService, $q, $sce) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/flyout/opInfoFlyOutTemplate.html",
                scope: {
                    cell: "=",
                    row: "=",
                    table: "=",
                    options: '=',
                    close: "&"
                },
                controller: function ($scope, $element, $attrs) {

                    $scope.htmlContent = $sce.trustAsHtml($scope.options.content);
                    var elementRect = $element[0].getBoundingClientRect();
                    var bodyRect = document.body.getBoundingClientRect();

                    var offset = bodyRect.width - elementRect.left - elementRect.width;
                    var arrowOffset;
                    if (offset < 0) {
                        offset = (offset - 20);
                        arrowOffset = -offset;

                        arrowOffset = arrowOffset + (elementRect.width / 10);

                        offset = offset + 'px';
                        arrowOffset = arrowOffset + 'px';
                    } else {
                        offset = '0px';
                        arrowOffset = '10%';
                    }

                    $scope.getElementStyle = function () {
                        return { left: offset };
                    }

                    $scope.getArrowStyle = function () {
                        return { left: arrowOffset };
                    }
                }
            }
        }
    ]);
});