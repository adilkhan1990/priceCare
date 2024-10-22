define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('SaveSimulationModalController', ['$scope', '$modalInstance', 'helperService','infos', 'userService',
        function ($scope, $modalInstance, helperService, infos, userService) {

            $scope.model = { name: infos.simulationName };
            $scope.isCurrentUser = infos.isCurrentUser;
            $scope.isNewSimulation = infos.simulationName == null;

            $scope.simulationOptions = {
                type: helperService.FieldTypes.Select,
                name: 'simulation',
                properties: {
                    class: 't-custom-select-boxed',
                    items: []
                }
            };

            var initSimulationOptions = function() {
                userService.getUserInfo().then(function() {
                    var simulations = [];

                    if (!userService.isGPRMStandardUser()) {
                        simulations = simulations.concat([
                            { id: 1, text: 'Budget', selected: infos.simulationOption == 'Budget' },
                            { id: 2, text: 'Reference', selected: infos.simulationOption == 'Reference' }
                        ]);
                    }

                    simulations = simulations.concat([
                        { id: 3, text: 'Public', selected: infos.simulationOption == 'Public' },
                        { id: 4, text: 'User', selected: infos.simulationOption == 'User' }
                    ]);

                    if (!_.any(simulations, function(sim) { return sim.selected; }))
                        simulations[simulations.length - 1].selected = true;

                    $scope.simulationOptions.properties.items =  simulations;
                });
            };

            initSimulationOptions();

            var getSelectedSimulationOption = function() {
                var result = {};
                $scope.simulationOptions.properties.items.forEach(function(opt) {
                    if (opt.selected)
                        result = opt;
                });
                return result.id;
            };

            var getSave = function () {
                var optionId = getSelectedSimulationOption();

                var save = {
                    name: $scope.model.name,
                    comment: $scope.model.comment,
                    overrideValue: $scope.model.overrideValue,
                };

                if (optionId == 1) { //budget
                    save.isBudget = true;
                    save.isApproved = true;
                    save.isReference = true;
                    save.isPublic = true;
                } else if (optionId == 2) { //reference
                    save.isPublic = true;
                    save.isApproved = true;
                    save.isReference = true;
                    save.isBudget = false;
                } else if (optionId == 3) { //public
                    save.isPublic = true;
                    save.isApproved = false;
                    save.isReference = false;
                    save.isBudget = false;
                } else { //
                    save.isPublic = false;
                    save.isApproved = false;
                    save.isReference = false;
                    save.isBudget = false;
                }
                return save;
            };

            $scope.ok = function () {
                if ($scope.model.name) {                                        
                    var save = getSave();
                    $modalInstance.close(save);
                }                
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };           
        }]);
});