define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('cacheService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.requestAccessStatus = null;
        me.requestAccessStatusPromise = null;

        me.GetRequestAccessStatus = function() {
            if (me.requestAccessStatusPromise == null) {
                var deferred = $q.defer();
                if (me.requestAccessStatus != null) {
                    deferred.resolve(me.requestAccessStatus);
                } else {
                    $http.get("/api/requestaccess/status").success(function(result) {
                        deferred.resolve(result);
                    });
                }
                me.requestAccessStatusPromise = deferred.promise;
            }

            return me.requestAccessStatusPromise;
        };


    }]);
});