define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('excelService', ['$http', '$q', function ($http, $q) {

        var me = this;

        me.postFilterExcel = function (searchRequest) {
            var deferred = $q.defer();
            $http.post('api/excel/postFilterExcel', searchRequest).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };

        me.getXlsTemplates = function() {
            var deferred = $q.defer();
            $http.get('api/excel/getXlsTemplates').success(function(data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        }

    }]);
});