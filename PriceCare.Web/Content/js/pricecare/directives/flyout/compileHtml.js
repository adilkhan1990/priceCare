define(['pricecare/directives/flyout/module'], function (module) {
    'use strict';
    module.directive("compileHtml",['$parse','$sce','$compile', function ($parse, $sce, $compile) {
        return {
            restrict: "A",
            link: function (scope, element, attributes) {

                var expression = $sce.parseAsHtml(attributes.compileHtml);

                var getResult = function () {
                    return expression(scope);
                };

                scope.$watch(getResult, function (newValue) {
                    var linker = $compile(newValue);
                    element.append(linker(scope));
                });
            }
        }
    }]);
});