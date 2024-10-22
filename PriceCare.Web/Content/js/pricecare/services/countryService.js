define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('countryService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getRegionsAndCountries = function () {
            var deferred = $q.defer();
            $http.get('api/country/regioncountries').success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.updateRegionCountries = function(data) {
            var deferred = $q.defer();
            $http.post('api/country/update', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getAllCountries = function () {
            var deferred = $q.defer();

            $http.get('api/country/all').success(function (countries) {
                deferred.resolve(countries);
            });

            return deferred.promise;
        };
        me.getRuleCountries = function(idsToIgnore) {
            var deferred = $q.defer();

            $http.post('api/country/countries', idsToIgnore).success(function (countries) {
                deferred.resolve(countries);
            });

            return deferred.promise;
        }
        me.getPagedRegionsAndCountries = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/country/pagedRegionAndCountries', searchRequest).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };
        me.getAllRegions = function () {
            var deferred = $q.defer();

            $http.get('api/country/allRegions').success(function (countries) {
                deferred.resolve(countries);
            });

            return deferred.promise;
        };
        me.addRegion = function(data) {
            var deferred = $q.defer();

            $http.post('api/country/addRegion', data).success(function(region) {
                deferred.resolve(region);
            });

            return deferred.promise;
        }

        me.deleteRegion = function(geographyId) {
            var deferred = $q.defer();
            $http.post('api/country/deleteRegion', geographyId).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.getCountriesPaged = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/country/paged', searchRequest).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.save = function (data) {
            var deferred = $q.defer();

            $http.post("api/country/save", data).success(function (products) {
                deferred.resolve(products);
            });

            return deferred.promise;
        };

    }]);
});