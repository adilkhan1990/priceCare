define(['pricecare/directives/module'], function(module) {
    'use strict';
    module.directive('ngContextMenu', function() {
        return function(scope, elem, attrs) {
            elem.bind('contextmenu', function (event) {
                scope.$apply(attrs.ngContextMenu);
             
                event.preventDefault();
            });
        };
    });
});