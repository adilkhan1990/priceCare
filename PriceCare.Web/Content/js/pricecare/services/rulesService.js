define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('rulesService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getDefinition = function (status) {
            var deferred = $q.defer();
            $http.get('api/rule/definition').success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };
        me.getReferencedCountries = function(geographyId) {
            var deferred = $q.defer();
            $http.get('api/rule/referencedCountries/'+geographyId).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }
        me.getRules = function(data) {
            var deferred = $q.defer();
            $http.post('api/rule/rules', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }
        me.getApplicableFromList = function(data) {
            var deferred = $q.defer();
            $http.post('api/rule/applicableFrom', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.getRuleMath = function() {
            var deferred = $q.defer();
            $http.get('api/rule/math').success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.saveRule = function(data) {
            var deferred = $q.defer();
            $http.post('api/rule/save', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.cacheRule = function (data) {
            var deferred = $q.defer();
            $http.post('api/rule/updateCache', data).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.getSubRules = function(data) {
            var deferred = $q.defer();
            $http.post('api/rule/subRules', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getRulePriceTypes = function(data) {
            var deferred = $q.defer();
            $http.post('api/rule/priceTypes', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

    }]);
});