define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('fileChange', function () {
        return {
            link: function (scope, element, attrs) {
                element[0].onchange = function () {
                    scope[attrs['fileChange']](element[0]);
                }
            }

        }
    }
    );
});