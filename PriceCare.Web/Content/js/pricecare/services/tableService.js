define(['pricecare/services/module'], function(module) {
    'use strict';
    module.service('tableService', [
        function() {
            var me = this;

            me.foreachCells = function(table, callback, includeHeader) {
                if (table && table.rows) {
                    for (var i = includeHeader ? 0 : 1; i < table.rows.length; i++) {
                        var row = table.rows[i];
                        for (var j = 1; j < row.cells.length; j++) {
                            var cell = row.cells[j];
                            callback(cell, row);
                        }
                    }
                }
            };

            me.foreachRows = function (table, callback, includeHeader) {
                if (table && table.rows) {
                    for (var i = includeHeader ? 0 : 1; i < table.rows.length; i++) {
                        var row = table.rows[i];
                            callback(row);
                    }
                }
            };
        }
    ]);
});