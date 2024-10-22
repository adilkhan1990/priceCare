
define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('priceService', ['$http', '$q','helperService', function ($http, $q, helperService) {
        var me = this;

        me.getPrices = function (geographyIds, productId, versionId, validate, compareToSimulation, compareTo) {
            var deferred = $q.defer();
            $http.post('api/price/paged', {
                geographyIds: geographyIds,
                productId: productId,
                versionId: versionId,
                compareToSimulation: !compareToSimulation,
                validate: validate,
                compareTo: compareTo
            }).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.getPricesForecast = function (data) {
            var deferred = $q.defer();
            $http.post('api/price/forecast', data).success(function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        };

        me.saveVersion = function (editedCells) {
            var deferred = $q.defer();
            $http.post('api/data/save', editedCells).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };


        me.getPriceValue = function (price, currency, consolidation) {
            var value = price.value;
            // Get value for selected currency
            if (currency == helperService.CurrencyTypes.EurosSpot) {
                value = value * price.eurSpot;
            } else if (currency == helperService.CurrencyTypes.EurosBudget) {
                value = value * price.eurBudget;
            } else if (currency == helperService.CurrencyTypes.DollarsSpot) {
                value = value * price.usdSpot;
            } else if (currency == helperService.CurrencyTypes.DollarsBudget) {
                value = value * price.usdBudget;
            }

            // Apply consolidation
            if(consolidation != null)
                value = value * consolidation.factorScreen * consolidation.factor;
            return value;
        };

        me.reversePriceValue = function (value, price, currency, consolidation) {
            var returnValue = value;

            // Remove consolidation
            returnValue = returnValue / consolidation.factorScreen;
            returnValue = returnValue / consolidation.factor;

            // Remove currency (if applicable)
            if (currency == helperService.CurrencyTypes.EurosSpot) {
                returnValue = returnValue / price.eurSpot;
            } else if (currency == helperService.CurrencyTypes.EurosBudget) {
                returnValue = returnValue / price.eurBudget;
            } else if (currency == helperService.CurrencyTypes.DollarsSpot) {
                returnValue = returnValue / price.usdSpot;
            } else if (currency == helperService.CurrencyTypes.DollarsBudget) {
                returnValue = returnValue / price.usdBudget;
            }

            return returnValue;

        };


    }]);
});