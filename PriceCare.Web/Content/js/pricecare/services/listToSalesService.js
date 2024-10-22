define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('listToSalesService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getPaged = function(searchRequest) {
            var deferred = $q.defer();

            $http.post('api/listToSales/paged', searchRequest).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.saveVersion = function (listToSalesInfo) {
            var deferred = $q.defer();

            $http.post('api/listToSales/saveVersion', listToSalesInfo).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

    }]);
});