define(["pricecare/users/module"], function (module) {
    'use strict';
    module.controller("AddUserController",
        ['$scope', 'userService', '$location', 'helperService',
            function ($scope, userService, $location, helperService) {

                $scope.filterOptions = {
                    header: {
                        items: [
                            {
                                type: helperService.FieldTypes.Label,
                                properties: {
                                    text: "Create user"
                                }
                            }
                        ]
                    }
                }

                userService.getAllRoles().then(function (result) {
                    $scope.roles = result;
                });
                $scope.rolesSelected = [];
                $scope.register = {
                    roles: []
                };

                var init = function () {
                    // Validations
                    $scope.isValidUserName = true;
                    $scope.isValidFirstName = true;
                    $scope.isValidLastName = true;
                    $scope.isValidEmail = true;
                    $scope.isValidRole = true;
                    $scope.isValidPassword = true;
                    $scope.isValidConfirmPassword = true;
                    // Error text
                    $scope.usernameErrorText = "";
                    $scope.firstNameErrorText = "";
                    $scope.lastNameErrorText = "";
                    $scope.emailErrorText = "";
                    $scope.rolesErrorText = "";
                    $scope.passwordErrorText = "";
                    $scope.passwordConfirmErrorText = "";


                };

                init();


                $scope.save = function () {
                    userService.createUserAndAssignRoles($scope.register).then(function (response) {
                        init();
                        if (response) {
                            if (!response.isError) {
                                $location.path('/admin/users');
                            } else {
                                response.sourceErrors.forEach(function (err) {
                                    switch (err.source) {
                                        case "Username":
                                            $scope.isValidUserName = false;
                                            $scope.usernameErrorText = err.message;
                                            break;
                                        case "FirstName":
                                            $scope.isValidFirstName = false;
                                            $scope.firstNameErrorText = err.message;
                                            break;
                                        case "LastName":
                                            $scope.isValidLastName = false;
                                            $scope.lastNameErrorText = err.message;
                                            break;
                                        case "Email":
                                            $scope.isValidEmail = false;
                                            $scope.emailErrorText = err.message;
                                            break;
                                        case "Roles":
                                            $scope.isValidRole = false;
                                            $scope.rolesErrorText = err.message;
                                            break;
                                        case "Password":
                                            $scope.isValidPassword = false;
                                            $scope.passwordErrorText = err.message;
                                            break;
                                        case "ConfirmPassword":
                                            $scope.isValidConfirmPassword = false;
                                            $scope.passwordConfirmErrorText = err.message;
                                            break;
                                    }
                                });
                            }
                        }
                    });
                };

                $scope.selectRole = function (role, index) {
                    if ($scope.rolesSelected[index] == true) {
                        $scope.register.roles.push(role);
                    } else {
                        $scope.register.roles = _.reject($scope.register.roles, function (r) {
                            return r.id == role.id;
                        });
                    }

                };

                $scope.cancel = function() {
                    $location.path('/admin/users');
                };


            }]);
});