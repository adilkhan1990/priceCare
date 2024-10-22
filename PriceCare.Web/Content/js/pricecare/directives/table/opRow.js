define(['pricecare/directives/table/module'], function (module) {
    'use strict';
    module.directive('opRow', [
        '$document', 'helperService',
        function ($document, helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/table/opRowTemplate.html",
                scope: {
                    row: "=",
                    table: "="
                },
                controller: function ($scope, $element, $attrs) {
                   
                }
            }
        }
    ]);
});