define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('SupportModalController', ['$scope', '$modalInstance', 
        function ($scope, $modalInstance) {

            $scope.ok = function() {

            };

            $scope.cancel = function() {
                $modalInstance.dismiss('cancel');
            };

        }]);
});