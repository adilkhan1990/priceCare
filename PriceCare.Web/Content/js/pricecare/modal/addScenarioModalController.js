define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('AddScenarioModalController', ['$scope', '$modalInstance', 'helperService', 'simulationService',
        function ($scope, $modalInstance, helperService, simulationService) {

            // Variables
            $scope.model = {};

            // Validation
            $scope.isNameEmpty = false;
            $scope.isNameUnique = true;

            $scope.ok = function () {
                $scope.isNameEmpty = !$scope.model.name || $scope.model.name == "";
                if (!$scope.isNameEmpty) {
                    var save = {
                        name: $scope.model.name,
                        comment: $scope.model.comment
                    };
                    simulationService.isValid(save).then(function (result) {
                        $scope.isNameUnique = result.isNameUnique;
                        $scope.isNameEmpty = result.isNameEmpty;
                        if (!$scope.isNameEmpty && $scope.isNameUnique) {
                            $modalInstance.close(save);
                        }
                    });
                }
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };
        }]);
});