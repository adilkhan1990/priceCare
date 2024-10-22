define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('priceGuidanceService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getRuleTypes = function() {
            var deferred = $q.defer();
            $http.get('api/rule/ruleTypes').success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getAllPriceGuidances = function (data) {
            var deferred = $q.defer();            
            $http.post('api/priceguidance/all', data).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getApplicableFromList = function(data) {
            var deferred = $q.defer();
            $http.post('api/priceguidance/from', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.savePriceMap = function(data) {
            var deferred = $q.defer();
            $http.post('api/priceguidance/savePriceMap', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

    }]);
});