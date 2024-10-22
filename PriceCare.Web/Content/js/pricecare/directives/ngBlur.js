define(['pricecare/directives/module'], function(module) {
    'use strict';
    module.directive('ngBlur', function() {
        return function(scope, elem, attrs) {
            elem.bind('blur', function() {
                scope.$apply(attrs.ngBlur);
            });
        };
    });
});