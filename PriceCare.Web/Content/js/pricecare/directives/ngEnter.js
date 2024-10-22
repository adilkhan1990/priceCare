define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('ngEnter', [function () {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });

                    event.preventDefault();
                }
            });
        };
    }]);

    module.directive('ngSelect', ['$timeout', function ($timeout) {
        return function (scope, element, attrs) {
            if (scope.$eval(attrs.ngSelect) === true) {
                $timeout(function() { element[0].select(); });
            }
        };
    }]);

    module.directive('ngFocus', [function () {
        return function (scope, element, attrs) {
            if (scope.$eval(attrs.ngFocus) === true) {
                element[0].focus();
            }
        };
    }]);
});