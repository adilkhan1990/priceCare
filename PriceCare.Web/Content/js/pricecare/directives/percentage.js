define(['pricecare/directives/module'], function(module) {
    'use strict';
    module.directive('percentage', [
        function() {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function (scope, element, attr, ngModel) {

                    function parser(text) {
                        return text / 100;
                    }

                    function formatter(text) {
                        var decimals = 2;
                        return Math.round(text * Math.pow(10, decimals + 2)) / Math.pow(10, decimals);
                    }

                    ngModel.$parsers.push(parser);
                    ngModel.$formatters.push(formatter);
                }
            }
        }
    ]);
});