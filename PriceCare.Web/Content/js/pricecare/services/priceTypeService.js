define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('priceTypeService',['$http', '$q', function($http, $q) {

        var me = this;

        me.getAllPriceTypes = function(currencyId) {
            var deferred = $q.defer();

            $http.get('api/pricetype/all/'+currencyId).success(function(result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.addPriceType = function(data) {
            var deferred = $q.defer();

            $http.post('api/pricetype/add', data).success(function(result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.getPagedPriceTypes = function (searchRequest, canceler) {
            var deferred = $q.defer();
            
            $http({
                method: 'POST',
                url: 'api/pricetype/paged',
                data: searchRequest,
                timeout: canceler.promise
            }).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };
        me.save = function (data) {
            var deferred = $q.defer();

            $http.post("api/pricetype/save", data).success(function (products) {
                deferred.resolve(products);
            });

            return deferred.promise;
        };

        me.isValid = function (data) {
            var deferred = $q.defer();

            $http.post("api/pricetype/isValid", data).success(function (products) {
                deferred.resolve(products);
            });

            return deferred.promise;
        };

    }]);
});