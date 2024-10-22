define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('ExcelFilterModalController', ['$scope', '$modalInstance', 'infos',
        function ($scope, $modalInstance, infos) {

            $scope.showAllEventsOption = infos.showAllEventsOption;

            $scope.response = {
                allCountries: false,
                allProducts: false,
                allEvents: false,
                databaseLike: false,
                isBudget: infos.isBudget
            };

            // Validation
            //$scope.isProductMissing = true;

            $scope.noFilterOnCountry = infos.noFilterOnCountry;
            $scope.noFilterOnProduct = infos.noFilterOnProduct;
            $scope.noFilterOnEvent = infos.noFilterOnEvent;

            $scope.ok = function () {
                //if (!$scope.noFilterOnEvent && !$scope.noFilterOnProduct) {
                //    $scope.isProductMissing = $scope.response.allEvents && !$scope.response.allProducts;
                //    if (!$scope.isProductMissing) {
                //        $modalInstance.close($scope.response);
                //    }
                //} else {
                   
                //}
                if (infos.showAllEventsOption == false)
                    $scope.response.allEvents = true;

                $modalInstance.close($scope.response);                
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

        }]);
});