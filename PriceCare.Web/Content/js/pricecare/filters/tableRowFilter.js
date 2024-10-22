define(['pricecare/filters/module'], function(module) {
    'use strict';
    module.filter("tableRowFilter", function() {
        return function(input, options) {
            if (!input)
                return input;

            var filtered = [];

            for (var x = 0; x < input.length; x++) {
                var current = input[x];
                if (!current.hide) {
                    if (current.parentRow) {
                        if (current.parentRow.expanded) {
                            if (!current.hide)
                                filtered.push(current);
                        }
                    } else {
                        if (!current.hide)
                            filtered.push(current);
                    }
                }
            }

            return filtered;
        }
    });
});