define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('invitationService', ['$http', '$window', '$q', function ($http, $window, $q) {

        var me = this;

        me.isEmailUnique = function (email) {
            var deferred = $q.defer();
            $http.post('api/invitation/isEmailUnique', email).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getRoles = function () {
            var deferred = $q.defer();
            $http.get('api/invitation/roles').success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.create = function (invitation) {
            var deferred = $q.defer();
            $http.post('api/invitation/create', invitation).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

        me.createForAnyUser = function (invitation) {
            var deferred = $q.defer();
            $http.post('api/invitation/createForAnyUser', invitation).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        }

    }]);

});