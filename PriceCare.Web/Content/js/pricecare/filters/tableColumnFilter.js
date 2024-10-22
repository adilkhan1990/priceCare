define(['pricecare/filters/module'], function(module) {
    'use strict';
    module.filter("tableColumnFilter", function() {
        return function (input, options) {
            if (options) {
                var filtered = [];
                var actualCount = 0;
                var actualOffset = 0;
                for (var x = 0; x < input.length; x++) {
                    var current = input[x];
                    if (x < options.fixed) {
                        filtered.push(current);
                    } else {
                        if (actualOffset >= options.offset) {
                            if (actualCount < options.count) {
                                filtered.push(current);
                                actualCount++;
                            }
                        } else {
                            actualOffset++;
                        }
                    }
                }
                return filtered;
            }
            return input;
        }
    });
});