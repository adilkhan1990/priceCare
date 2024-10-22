define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('analyzerService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getAnalyzerData = function (simulationId,georgaphyIds, productId, dataTypeId) {
            var deferred = $q.defer();

            $http.post('api/analyzer/data/', {
                geographyIds: georgaphyIds,
                productId: productId,
                dataTypeId: dataTypeId,
                simulationId: simulationId
            }).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };
        me.GetSalesImpact = function (simulationId, georgaphyIds, productId, dataTypeId, events) {
            var deferred = $q.defer();

            $http.post('api/analyzer/salesImpact/', {
                geographyIds: georgaphyIds,
                productId: productId,
                dataTypeId: dataTypeId,
                simulationId: simulationId,
                events: events
            }).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };

    }]);
});