define(['pricecare/services/module'], function(module) {
    'use strict';
    module.service('dataService', [
        '$http', '$q', function($http, $q) {
            var me = this;

            me.saveVersion = function(editedCells) {
                var deferred = $q.defer();
                $http.post('api/data/save', editedCells).success(function(data) {
                    deferred.resolve(data);
                });
                return deferred.promise;
            }
        }
    ]);
});