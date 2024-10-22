define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('requestAccessService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getPagedRequestAccesses = function(searchRequest) {
            var deferred = $q.defer();

            $http.post("api/requestaccess/paged", searchRequest).success(function(result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

        me.changeStatus = function(statusInfo) {
            var deferred = $q.defer();

            $http.post("api/requestaccess/changeStatus", statusInfo).success(function (result) {
                deferred.resolve(result);
            });

            return deferred.promise;
        };

    }]);
});