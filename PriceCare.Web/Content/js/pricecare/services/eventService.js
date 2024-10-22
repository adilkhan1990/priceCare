define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('eventService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getEvents = function (eventTypeOptions) {
            var deferred = $q.defer();
            $http.post('api/event/paged', eventTypeOptions).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.saveVersion = function (editedCells) {
            var deferred = $q.defer();
            $http.post('api/data/save', editedCells).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };

        me.getEventsForecast = function (eventOptions) {
            var deferred = $q.defer();
            $http.post('api/event/forecast', eventOptions).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };
        me.postFilterExcel = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/event/postFilterExcel', searchRequest).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };


    }]);
});