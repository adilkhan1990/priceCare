define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('dimensionService', [
        '$http', '$q', function ($http, $q) {

            var me = this;

            me.eventTypesPromise = null;
            var eventTypes;

            me.eventTypes = {
                launchWithTargetPrice: 24
            };

            me.getEventTypes = function () {
                if (me.eventTypesPromise == null) {
                    var deferred = $q.defer();
                    $http.get('api/dimension/events').success(function (result) {
                        deferred.resolve(result);
                    });
                    me.eventTypesPromise = deferred.promise;
                }

                return me.eventTypesPromise;
            };

            me.getFormulationTypes = function () {
                var deferred = $q.defer();
                $http.get('api/dimension/formulations').success(function (result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            };

            me.getUnitTypes = function () {
                var deferred = $q.defer();
                $http.get('api/dimension/units').success(function (result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            };


        }]);
});