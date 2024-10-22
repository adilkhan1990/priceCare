define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('skuService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getAllSku = function () {
            var deferred = $q.defer();

            $http.get('api/sku/all').success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.getSku = function(data) {
            var deferred = $q.defer();
            $http.post('api/sku', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.addSku = function(data) {
            var deferred = $q.defer();
            $http.post('api/sku/addSku', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.save = function (data) {
            var deferred = $q.defer();

            $http.post("api/sku/save", data).success(function (products) {
                deferred.resolve(products);
            });

            return deferred.promise;
        };

    }]);
});