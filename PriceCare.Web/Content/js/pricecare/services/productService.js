define(["pricecare/services/module"], function(module) {
    'use strict';
    module.service('productService', [
        '$http', '$q', function($http, $q) {

            var me = this;

            me.getAllProducts = function() {
                var deferred = $q.defer();

                $http.get("api/product/all").success(function(products) {
                    deferred.resolve(products);
                });

                return deferred.promise;
            };

            me.getAllProductsForFilter = function() {
                var deferred = $q.defer();
                $http.get("api/product/products").success(function(products) {
                    deferred.resolve(products);
                });
                return deferred.promise;
            };

            me.getPagedProducts = function (search) {
                var deferred = $q.defer();

                $http.post("api/product/paged", search).success(function (products) {
                    deferred.resolve(products);
                });

                return deferred.promise;
            };

            me.saveProduct = function(data) {
                var deferred = $q.defer();

                $http.post("api/product/save", data).success(function (products) {
                    deferred.resolve(products);
                });

                return deferred.promise;
            };

            me.getProductUnits = function(productId) {
                var deferred = $q.defer();

                $http.get('api/product/units/' + productId).success(function(units) {
                    deferred.resolve(units);
                });

                return deferred.promise;
            };

            me.addProductUnit = function(productUnit) {
                var deferred = $q.defer();

                $http.post('api/product/addProductUnit', productUnit).success(function(result) {
                    deferred.resolve(result);
                });

                return deferred.promise;
            };

            me.deleteProductUnit = function(productUnit) {
                var deferred = $q.defer();

                $http.post('api/product/deleteProductUnit', productUnit).success(function (result) {
                    deferred.resolve(result);
                });

                return deferred.promise;
            }

            me.updateProductUnit = function(productUnit) {
                var deferred = $q.defer();

                $http.post('api/product/updateproductUnit', productUnit).success(function(result) {
                    deferred.resolve(result);
                });

                return deferred.promise;
            }

        }
    ]);
});