define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('informationService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.getGeneralInfos = function () {
            var deferred = $q.defer();

            $http.get('api/informations/general').success(function (infos) {
                deferred.resolve(infos);
            });

            return deferred.promise;
        };

        me.update = function(data) {
            var deferred = $q.defer();
            $http.post('api/informations/update', data).success(function(result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

    }]);
});