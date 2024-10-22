define(['pricecare/informations/module'], function(module) {
    'use strict';
    module.controller('InformationsController', [
        '$scope', '$rootScope', '$controller', '$http', 'helperService', 'informationService',
        function($scope, $rootScope, $controller, $http, helperService, informationService) {

            $scope.filterOptions = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties:
                            {
                                text: "General information"
                            }
                        }
                    ]
                }
            };

            $scope.filterOptions = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties:
                            {
                                text: "User settings"
                            }
                        }
                    ]
                }
            };

            $scope.update = function () {
                var data = {
                    amgenSupportContactName: $scope.infos.amgenSupportContactName,
                    amgenSupportContactEmail: $scope.infos.amgenSupportContactEmail,
                    technicalSupportEmail: $scope.infos.technicalSupportEmail
                };
                informationService.update(data).then(function(result) {

                });
            };

            var init = function() {
                informationService.getGeneralInfos().then(function(result) {
                    $scope.infos = result[0];
                });
            };
            init();

        }
    ]);
});