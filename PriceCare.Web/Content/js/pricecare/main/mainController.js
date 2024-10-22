define(['pricecare/main/module'], function (module) {
    'use strict';
    module.controller('MainController', ['$scope', '$rootScope', '$modal', '$http', '$timeout', '$location', 'userService',
        function ($scope, $rootScope, $modal, $http, $timeout, $location, userService) {

            $scope.copyrightYear = new Date().getFullYear();
            $scope.showLoader = true;
            $scope.showSpinner = false;
            $scope.spinnerCount = 0;

            // Spinner methods

            $rootScope.increaseSpinner = function () {
                $scope.spinnerCount++;

                if ($scope.spinnerCount > 0) {

                    $timeout(function () {
                        if ($scope.spinnerCount > 0) {
                            $scope.showSpinner = true;
                        }
                    }, 400);
                }
            };
            $rootScope.decreaseSpinner = function () {
                $scope.spinnerCount--;
                if ($scope.spinnerCount < 0) {
                    $scope.spinnerCount = 0;
                }

                if ($scope.spinnerCount == 0) {
                    $scope.showSpinner = false;
                }
            };

            var init = function() {
                userService.getUserInfo().then(function(result) {
                    $scope.user = result;
                });
            };
            init();

            $scope.antiForgeryToken = "";

            $scope.logout = function () {
                userService.logout();
            };
            $scope.setToken = function (token) {
                $http.defaults.headers.common["X-Requested-With"] = 'XMLHttpRequest';
                $http.defaults.headers.common['RequestVerificationToken'] = token;
            };

            $scope.openModal = function() {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/supportModal.html',
                    controller: "SupportModalController",
                    backdrop: 'static',
                    windowClass: 'modal-support',
                    resolve: {
                        infos: function () {
                            return {
                                title: 'none',
                                content: 'i dunno'
                            };
                        }
                    }
                });
            }

            $rootScope.$on('$routeChangeStart', function (event, next) {
                if (next.originalPath == '/') {
                    if (userService.userInfo == null) {
                        userService.getUserInfo().then(function () {
                            navigateToProperHome();
                        });
                    } else {
                        navigateToProperHome();
                    }
                };
            });

            var navigateToProperHome = function () {
                if (userService.isSystemAdministrator() && userService.userInfo.roles.length == 1) {
                    $location.path("/admin/users").replace();
                } else if (userService.isDataAdministrator() &&
                    !userService.isGPRMAdvancedUser() &&
                    !userService.isGPRMStandardUser &&
                    !userService.isGPRMViewerUser()) {
                    $location.path("/data/prices").replace();
                } else {
                    $location.path("/forecast/prices").replace();
                }
            };

            var history = [];

            $rootScope.$on('$routeChangeSuccess', function () {
                history.push($location.$$path);
            });

            $rootScope.back = function () {
                var prevUrl = history.length > 1 ? history.splice(-2)[0] : "/";
                $location.path(prevUrl);
            };
    }]);
});