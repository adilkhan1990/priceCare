define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('AddItemModalController', ['$scope', '$modalInstance', 'itemName',
        function ($scope, $modalInstance, itemName) {

            $scope.itemName = itemName;
            $scope.model = {};
            $scope.formulationOptions = [];
                       
            $scope.ok = function () {
                if ($scope.model.itemName)
                    $modalInstance.close($scope.model.itemName);
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            }                       
        }]);
});