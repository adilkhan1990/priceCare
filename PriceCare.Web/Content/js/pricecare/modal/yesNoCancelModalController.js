define(['pricecare/modal/module'], function(module) {
    'use strict';
    module.controller('YesNoCancelModalController', [
        '$scope', '$modalInstance', 'infos', function($scope, $modalInstance, infos) {

            $scope.title = infos.title;
            $scope.content = infos.content;
            $scope.hideCancel = infos.hideCancel;

            $scope.yes = function() {
                $modalInstance.close(true);
            };
            $scope.no = function() {
                $modalInstance.close(false);
            };
        }
    ]);
});