define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('currencyService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getAllCurrencies = function (data) {
            var deferred = $q.defer();

            $http.post('api/currency/all', data).success(function (currencies) {
                deferred.resolve(currencies);
            });

            return deferred.promise;
        };
        me.getCurrenciesForFilter = function() {
            var deferred = $q.defer();

            $http.get('api/currency/filter').success(function(currencies) {
                deferred.resolve(currencies);
            });
            return deferred.promise;
        };

        me.getPaged = function (currencyInfo) {
            var deferred = $q.defer();

            $http.post('api/currency/paged', currencyInfo).success(function (currencies) {
                deferred.resolve(currencies);
            });
            return deferred.promise;
        };

        me.saveVersion = function(data) {
            var deferred = $q.defer();

            $http.post('api/currency/save', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

    }]);
});