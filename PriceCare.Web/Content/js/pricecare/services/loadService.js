define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('loadService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getLoads = function(request) {
            var deferred = $q.defer();
            $http.post('api/load/get/', request).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.cancelLoad = function (loadId) {
            var deferred = $q.defer();
            $http.post('api/load/cancel/' + loadId).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getLoadStatus = function () {
            var deferred = $q.defer();

            $http.get('api/load/getStatus').success(function (countries) {
                deferred.resolve(countries);
            });

            return deferred.promise;
        };

        me.postNewLoad = function (load) {
            var deferred = $q.defer();

            $http.post('api/load/newLoad', { name: load }).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.startLoad = function (loadId) {
            var deferred = $q.defer();

            $http.post('api/load/start/' + loadId).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.getLoadDetail = function (loadId) {
            var deferred = $q.defer();

            $http.get('api/load/detail/' + loadId).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.validateLoadItemDetail = function(loadId, loadItemName, productId, geographyIds) {
            var deferred = $q.defer();

            $http.post('api/load/validateLoadItemDetail', {
                loadId: loadId,
                loadItemName: loadItemName,
                productId: productId,
                geographyIds: geographyIds
            }).success(function(result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.validateLoadItem = function (loadItemId) {
            var deferred = $q.defer();

            $http.post('api/load/validateLoadItemId/' + loadItemId).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.getNext = function (loadId, loadItemName) {
            var deferred = $q.defer();
            $http.get('api/load/next/' + loadId + '/' + loadItemName).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getVolumeScenario = function ()
        {
            var deferred = $q.defer();

            $http.get('api/load/volumeScenario').success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        }

        me.updateVolumeScenario = function (data) {
            var deferred = $q.defer();

            $http.post('api/load/updateVolumeScenario', data).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        }

    }]);
});