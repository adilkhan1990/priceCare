define(['pricecare/services/module'], function (module) {
    'use strict';

    module.service('versionService', [
        '$http', '$window', '$q', 'helperService', 'userService', function ($http, $window, $q, helperService, userService) {

            var me = this;

            me.getCurrencyVersions = function (rateType) {
                var deferred = $q.defer();

                $http.get('api/version/currency/' + rateType).success(function (currencies) {
                    deferred.resolve(currencies);
                });

                return deferred.promise;
            };

            me.getRulesVersions = function(data) {
                var deferred = $q.defer();
                $http.post('api/version/rules', data).success(function(result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            };

            me.getPriceMapVersions = function (data) {
                var deferred = $q.defer();
                $http.post('api/version/priceMap', data).success(function (result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            };
            
            me.getEventTypeVersions = function (eventTypeInfo) {
             var deferred = $q.defer();

                $http.post('api/version/eventType', eventTypeInfo).success(function (data) {
                    deferred.resolve(data);
                });

                return deferred.promise;
            };

            me.getListToSalesVersions = function (versionInfo) {
                var deferred = $q.defer();

                $http.post('api/version/listToSales', versionInfo).success(function (data) {
                    deferred.resolve(data);
                });

                return deferred.promise;
            };

            me.getVolumeVersions = function (versionInfo) {
                var deferred = $q.defer();

                $http.post('api/volume/versions', versionInfo).success(function (data) {
                    deferred.resolve(data);
                });

                return deferred.promise;
            };

            me.getVersionsForFilter = function (data, userMapping) {                
                if (data.length > 0) {
                    data[0].selected = true;
                }

                data.forEach(function (version) {
                    version.text = version.information + " / " + helperService.formatDate(version.versionTime) + " / " + userService.mapUser(version.userName, userMapping);
                });

                return data;
            };

            me.getCompareVersionsForFilter = function(data, userMapping) {
                var versions = [];

                data.forEach(function(version) {
                    versions.push({
                        versionId: version.versionId,
                        text: version.information + " / " + helperService.formatDate(version.versionTime) + " / " + userService.mapUser(version.userName, userMapping)
                    });
                });

                return versions;
            }
        }
    ]);
});