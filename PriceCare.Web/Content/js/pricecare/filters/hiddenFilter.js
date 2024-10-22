define(['pricecare/filters/module'], function(module) {
    'use strict';
    module.filter("hiddenFilter", function () {
        return function (input, options) {
            if (!input)
                return input;

            var filtered = [];

            for (var x = 0; x < input.length; x++) {
                var current = input[x];
                if (!current.hide) {
                    filtered.push(current);
                }
            }

            return filtered;
        }
    });
});