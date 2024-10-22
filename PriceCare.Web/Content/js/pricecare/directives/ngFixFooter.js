define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('ngFixFooter', ['$window', function ($window) {
        return function (scope, element, attrs) {

            var w = angular.element($window);
            scope.getWindowDimensions = function () {
                return {
                    'h': $window.innerHeight,
                    'w': $window.innerWidth
                };
            };
            scope.$watch(scope.getWindowDimensions, function (newValue, oldValue) {
                scope.windowHeight = newValue.h;
                scope.windowWidth = newValue.w;

                scope.style = function () {
                    return {
                        'min-height': (newValue.h - 91) + 'px'
                    };
                };

            }, true);

            w.bind('resize', function () {
                scope.$apply();
            });

        };
    }]);
});