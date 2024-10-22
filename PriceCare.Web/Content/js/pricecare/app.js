define([
   'angular',
   './main/index',
   './home/index',
   './errors/index',
   './directives/index',
   './services/index',
   './filters/index',
   './design/index',
   './products/index',
   './pricetypes/index',
   './priceguidance/index',
   './listToSales/index',
   './sku/index',
   './countries/index',
   './users/index',
   './currencies/index',
   './informations/index',
   './forecast/index',
   './prices/index',
   './volumes/index',
   './events/index',
   './rules/index',
   './load/index',   
   './forecast/index',
   './modal/index',
   './requestaccess/index',
   './listToSalesImpact/index',
   './dimensionDictionary/index',
   './units/index',
   './regions/index',
   './excelTemplates/index'
],
function (angular) {
    'use strict';
    var app = angular.module('app', [
        'ngRoute',
        'ui.bootstrap',
        'ui.sortable',
        'ui.utils',
        'angular-md5',
        'app.main',
        'app.home',
        'app.errors',
        'app.directives',
        'app.directives.table',
        'app.directives.flyout',
        'app.services',
        'app.filters',
        'app.design',
        'app.products',
        'app.pricetypes',
        'app.priceguidance',
        'app.listToSales',
        'app.sku',
        'app.countries',
        'app.users',
        'app.currencies',
        'app.informations',
        'app.forecast',
        'app.prices',
        'app.volumes',
        'app.events',
        'app.rules',
        'app.load',
        'app.forecast',
        'app.requestaccess',
        'app.regions',
        'app.modal',
        'app.listToSalesImpact',
        'app.dimensionDictionary',
        'app.units',
        'app.excelTemplates',
        'angularFileUpload'
]);

    app.factory('authHttpResponseInterceptor', [
        '$q', '$injector', '$rootScope', function ($q, $injector, $rootScope) {

            var me = this;

            me.calls = 0;
            var fact = {};

            fact.request = function (config) {
                $rootScope.increaseSpinner();
                return config;
            };

            fact.response = function (response) {
                $rootScope.decreaseSpinner();
                if (response.status === 401) {
                    console.log("Response 401");
                }
                return response || $q.when(response);
            };

            fact.responseError = function (rejection) {
                $rootScope.decreaseSpinner();
                if (rejection.status === 401) {
                    console.log("Response Error 401", rejection);
                    $injector.invoke(function ($modal, $window) {
                        //todo
                    });
                }
                return $q.reject(rejection);
            };

            return fact;
        }
    ]);

    app.config([
        '$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push('authHttpResponseInterceptor');
        }
    ]);

    return app;
});
