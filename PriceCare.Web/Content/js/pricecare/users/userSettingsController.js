define(['pricecare/users/module'], function (module) {
    'use strict';
    module.controller('UserSettingsController', [
        '$scope', '$rootScope', '$controller', '$http', 'helperService', 'userService', 'cacheService', '$q', 'countryService', 'productService',
        function ($scope, $rootScope, $controller, $http, helperService, userService, cacheService, $q, countryService, productService) {

            $scope.filterOptions = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties: {
                                text: "General informations"
                            }
                        }
                    ]
                }
            }

            $scope.filterOptions2 = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties: {
                                text: "User settings"
                            }
                        }
                    ]
                }
            }

            // Validation
            $scope.isValidFirstName = true;
            $scope.isValidLastName = true;
            $scope.isValidEmail = true;
            $scope.isValidFormatEmail = true;
            $scope.isUniqueEmail = true;

            var getItemsForFilter = function (items, selectedId) {
                var result = [];
                items.forEach(function(item) {
                    var tmp = { id: item.id, text: item.name };
                    tmp.selected = item.id == selectedId;
                    result.push(tmp);
                });
                return result;
            };

            var userPromise = userService.getUserInfo();
            var regionPromise = countryService.getAllRegions();
            var countryPromise = countryService.getAllCountries();
            var productPromise = productService.getAllProducts();
            var settingsPromise = userService.getUserSettings();

            $q.all([userPromise, regionPromise, countryPromise, productPromise, settingsPromise]).then(function (data) {
                //user infos
                $scope.user = helperService.clone(data[0]);
                $scope.user.roles = _.values($scope.user.roles); //small fix for clone 
                $scope.orignalFirstName = data[0].firstName;
                $scope.originalLastName = data[0].lastName;
                $scope.originalEmail = data[0].email;
                $scope.password = "********";
                //settings
                $scope.regions = data[1];
                $scope.countries = data[2];
                $scope.products = data[3];

                $scope.selectedRegionId = data[4].defaultRegionId;
                $scope.selectedCountryId = data[4].defaultCountryId;
                $scope.selectedProductId = data[4].defaultProductId;

                var regionOptions = getItemsForFilter($scope.regions, $scope.selectedRegionId);
                var countryOptions = getItemsForFilter($scope.countries, $scope.selectedCountryId);
                var productOptions = getItemsForFilter($scope.products, $scope.selectedProductId);
                
                $scope.regionOptions = {
                    type: helperService.FieldTypes.Select,
                    name: 'regions',
                    properties: {
                        class: 't-custom-select-boxed',
                        items: regionOptions
                    }
                }

                $scope.countryOptions = {
                    type: helperService.FieldTypes.Select,
                    name: 'countries',
                    properties: {
                        class: 't-custom-select-boxed',
                        items: countryOptions
                    }
                }

                $scope.productOptions = {
                    type: helperService.FieldTypes.Select,
                    name: 'products',
                    properties: {
                        class: 't-custom-select-boxed',
                        items: productOptions
                    }
                }
            });

            $scope.saveInfos = function () {
                $scope.isValidEmail = $scope.user.email && $scope.user.email != "";
                $scope.isValidFirstName = $scope.user.firstName && $scope.user.firstName != "";
                $scope.isValidLastName = $scope.user.lastName && $scope.user.lastName != "";

                if ($scope.isValidEmail) {
                    $scope.isValidFormatEmail = helperService.validateEmail($scope.user.email);
                    if ($scope.isValidFormatEmail) {
                        var model = { user: $scope.user, createPurpose: false };
                        userService.isValid(model).then(function (result) {
                            $scope.isUniqueEmail = result.emailUnique === true;
                            if ($scope.isUniqueEmail && $scope.isValidFirstName && $scope.isValidLastName) {
                                userService.saveUserInfo($scope.user).then(function (res) {
                                    userService.forceLoadUsers();
                                    $scope.$parent.user = helperService.clone(userService.userInfo);
                                });
                            }
                        });
                    }
                }
            };

            var getSelectedItemId = function(items) {
                var id;
                items.forEach(function(item) {
                    if (item.selected)
                        id = item.id;
                });
                return id;
            };

            $scope.saveSettings = function () {
                var regionId = getSelectedItemId($scope.regionOptions.properties.items);
                var countryId = getSelectedItemId($scope.countryOptions.properties.items);
                var productId = getSelectedItemId($scope.productOptions.properties.items);

                var data = { defaultRegionId: regionId, defaultCountryId: countryId, defaultProductId: productId }
                userService.saveSettings(data).then(function (result) {
                    if (result == "true" ) {
                        userService.getUserSettings().then(function (settings) {});
                    }
                        
                });
            }                       
        }
    ]);
});

