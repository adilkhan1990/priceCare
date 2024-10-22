define(['pricecare/directives/module'], function(module) {
    'use strict';
    module.directive('opDynamicHost', [
        '$compile',
        function($compile) {
            return {
                restrict: 'A',
                replace: true,
                template: '<div></div>',
                scope: {
                    options: "=",
                    onApply: "&"
                },
                link: function (scope, iElement, iAttrs) {

                    scope.onApplyClicked = function () {
                        if (scope.onApply) {
                            scope.onApply();
                        }
                    };

                    if (scope.options.directive) {
                        iElement.append($compile('<div ' + scope.options.directive + ' options="options.options" on-apply="onApplyClicked()"></div>')(scope));
                    }
                }
            };
        }
    ]);
});