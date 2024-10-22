define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('volumeService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getVolumes = function(geographyIds, productId,versionId, validate, compareToVersionId) {
            var deferred = $q.defer();
            $http.post('api/volume/paged', {                
                geographyIds: geographyIds,
                productId: productId,
                validate: validate,
                versionId: versionId,
                compareToVersionId: compareToVersionId
            }).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };        

        me.getForecastVolumes = function (data) {
            var deferred = $q.defer();
            $http.post('api/volume/pagedForecast', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.postFilterExcel = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/volume/postFilterExcel', searchRequest).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };

        me.getVolumeValue = function (volume, consolidation) {
            var value = volume.value;
            // Apply consolidation
            value = value / (consolidation.factorScreen * consolidation.factor);
            return value;
        };

        me.reversePriceValue = function (value, consolidation) {
            var returnValue = value;

            // Remove consolidation
            returnValue = returnValue * consolidation.factorScreen;
            returnValue = returnValue * consolidation.factor;

            

            return returnValue;

        };
    }]);
});