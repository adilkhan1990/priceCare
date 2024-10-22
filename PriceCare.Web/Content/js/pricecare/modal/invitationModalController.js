define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('InvitationModalController', ['$scope', '$modalInstance', 'invitationService',
        function ($scope, $modalInstance, invitationService) {
            // User
            $scope.invitation = {};
            $scope.invitation.roles = [];

            $scope.rolesSelected = [];

            invitationService.getRoles().then(function (result) {
                $scope.roles = result;
                //$scope.rolesSelected
            });

            // Validation
            $scope.isValidEmail = true;
            $scope.isRequiredEmail = true;
            $scope.isUniqueEmail = true;
            $scope.isValidRole = true;

            $scope.ok = function () {
                $scope.isRequiredEmail = $scope.invitation.email && $scope.invitation.email != "";
                $scope.isValidRole = $scope.invitation.roles != null && $scope.invitation.roles.length > 0;
                if ($scope.isRequiredEmail) {
                    $scope.isValidEmail = validateEmail($scope.invitation.email);
                    if ($scope.isValidEmail) {
                        invitationService.isEmailUnique({ email: $scope.invitation.email }).then(function (result) {
                            $scope.isUniqueEmail = JSON.parse(result);
                            if ($scope.isUniqueEmail === true && $scope.isValidRole === true) {
                                $modalInstance.close($scope.invitation);
                            }
                        });
                    }
                }
            };

            function validateEmail(email) {
                var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                return re.test(email);
            }

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

            $scope.selectRole = function (role, index) {
                if ($scope.rolesSelected[index] == true) {
                    $scope.invitation.roles.push(role);
                } else {
                    $scope.invitation.roles = _.reject($scope.invitation.roles, function(r) {
                        return r.id == role.id;
                    });
                }
                
            };

        }]);
});