define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('ConfirmationModalController', ['$scope', '$modalInstance', 'infos', function ($scope, $modalInstance, infos) {

        $scope.title = infos.title;
        $scope.content = infos.content;
        $scope.hideCancel = infos.hideCancel;

        $scope.ok = function () {
            $modalInstance.close();
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };

    }]);
});