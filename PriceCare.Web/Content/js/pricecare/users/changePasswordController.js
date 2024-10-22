define(['pricecare/users/module'], function (module) {
    'use strict';
    module.controller('ChangePasswordController', [
        '$scope', '$rootScope', '$controller', '$http', 'helperService', '$location',
        function ($scope, $rootScope, $controller, $http, helperService, $location) {

            var init = function() {
                // Validation
                $scope.isValidOldPassword = true;
                $scope.isValidNewPassword = true;
                $scope.isValidConfirmPassword = true;
                // Error messages
                $scope.errorOldPassword = "";
                $scope.errorNewPassword = "";
                $scope.errorConfirmPassword = "";
            };

            init();

            $scope.filterOptions = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties: {
                                text: "Change password"
                            }
                        }
                    ]
                }
            }

            $scope.submit = function () {
                init();
                var data = {
                    oldPassword: $scope.oldPassword,
                    newPassword: $scope.newPassword,
                    confirmPassword: $scope.confirmPassword
                };

                $http.post('api/user/password', data).success(function(result) {
                    if (result) {
                        $scope.response = result;
                        if (!$scope.response.isError) {
                            $location.path('user/settings');
                        } else {
                            result.sourceErrors.forEach(function (err) {
                                switch (err.source) {
                                    case "OldPassword":
                                        $scope.isValidOldPassword = false;
                                        $scope.errorOldPassword = err.message;
                                        break;
                                    case "NewPassword":
                                        $scope.isValidNewPassword = false;
                                        $scope.errorNewPassword = err.message;
                                        break;
                                    case "ConfirmPassword":
                                        $scope.isValidConfirmPassword = false;
                                        $scope.errorConfirmPassword = err.message;
                                        break;
                                }
                            });
                        }
                    } 
                });

            };


            $scope.cancel = function() {
                $location.path('user/settings');
            };
        }
    ]);

});