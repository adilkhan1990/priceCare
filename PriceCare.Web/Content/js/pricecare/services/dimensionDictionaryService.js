define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('dimensionDictionaryService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.DimensionType = {
            Product: 'Product',
            Geography: 'Geography'
        };

        Object.freeze(me.DimensionType);

        me.getAllByDimension = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/dimensionDictionary/all', searchRequest).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getAllGcodsForDimension = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/dimensionDictionary/allGcods', searchRequest).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.create = function (model) {
            var deferred = $q.defer();

            $http.post('api/dimensionDictionary/create', model).success(function (identifier) {
                deferred.resolve(identifier);
            });

            return deferred.promise;
        };

        me.update = function (models) {
            var deferred = $q.defer();

            $http.post('api/dimensionDictionary/update', models).success(function (identifier) {
                deferred.resolve(identifier);
            });

            return deferred.promise;
        };

        me.deleteSynonym = function (synonym) {
            var deferred = $q.defer();

            $http.post('api/dimensionDictionary/synonyms/delete', synonym).success(function(result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        }

        me.prepareDimensionTypeForFilter = function (defaultDimension) {
            var result = [];

            result.push({
                id: me.DimensionType.Product,
                name: me.DimensionType.Product,
                text: me.DimensionType.Product,
                selected: !defaultDimension || defaultDimension == me.DimensionType.Product
            }, {
                id: me.DimensionType.Geography,
                name: me.DimensionType.Geography,
                text: me.DimensionType.Geography,
                selected: defaultDimension && defaultDimension == me.DimensionType.Geography
            });

            return result;
        };
        
    }]);
});