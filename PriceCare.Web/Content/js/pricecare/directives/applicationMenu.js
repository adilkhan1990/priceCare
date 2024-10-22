define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('applicationMenu', ['$timeout', 'userService',
        function ($timeout, userService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/applicationMenuTemplate.html",
                controller: function ($scope, $element, $attrs) {

                    $scope.menuItems = [];

                    $scope.levelClass = 'lvl1';
                    $scope.level = 1;

                    $scope.openMenu = function () {
                        // re-initialisation
                        $scope.levelClass = 'lvl1';
                        $scope.level = 1;
                        setShowMenu($scope.menuItems, 1, $scope.level);
                    };

                    $scope.itemClicked = function (item) {
                        if (item.children) {
                            item.showChildren = true;
                            $scope.levelClass = 'lvl' + (++$scope.level);
                        }
                    };

                    $scope.goBack = function () {
                        $scope.levelClass = 'lvl' + (--$scope.level);
                        $timeout(function () {
                            setShowMenu($scope.menuItems, 1, $scope.level);
                        }, 600);
                    };

                    // to init the userInfo
                    userService.getUserInfo().then(function (userInfo) {

                        var allAccess = function () {
                            return true;
                        };

                        var onlyAdmin = function () {
                            return userService.isSuperAdministrator() || userService.isSystemAdministrator();
                        };

                        var onlyGprmUsers = function () {
                            return userService.isGPRMAdvancedUser() || userService.isGPRMStandardUser()
                                || userService.isGPRMViewerUser || userService.isSystemAdministrator();
                        };

                        var onlyDataAdministrator = function () {
                            return userService.isDataAdministrator() || userService.isSuperAdministrator();
                        };

                        $scope.allMenuItems = [
                            {
                                class: 'dashboard',
                                link: '#/',
                                text: 'Dashboard',
                                canAccess: allAccess()
                            },
                            {
                            class: 'dataset',
                                text: 'Data',
                                canAccess: onlyDataAdministrator(),
                                children: [
                                    {
                                    class: 'price',
                                        link: '#/data/prices',
                                        text: 'Prices',
                                        canAccess: onlyDataAdministrator()
                                    },
                                    {
                                        class: 'reports',
                                        link: '#/data/volumes',
                                        text: 'Volumes',
                                        canAccess: onlyDataAdministrator()
                                    },
                                    {
                                    class: 'events',
                                        link: '#/data/events',
                                        text: 'Events',
                                        canAccess: onlyDataAdministrator()
                                    },
                                    {
                                        class: 'rules',
                                        link: '#/data/rules',
                                        text: 'Rules',
                                        canAccess: onlyDataAdministrator()
                                    },
                                    {
                                    class: 'price-map',
                                        link: '#/data/dimensions/pricemap',
                                        text: 'Price Map',
                                        canAccess: onlyDataAdministrator()
                                    },
                                    {
                                    class: 'net-data',
                                        link: '#/data/dimensions/listToSales',
                                        text: 'Net Data',
                                        canAccess: onlyDataAdministrator()
                                    },
                                    {
                                    class: 'net-impact',
                                        link: '#/data/dimensions/listToSalesImpact',
                                        text: 'Net Impact',
                                        canAccess: onlyDataAdministrator()
                                    },
                                    {
                                        class: 'dimensions',
                                        text: 'Dimensions',
                                        canAccess: onlyDataAdministrator(),
                                        children: [
                                            {
                                                class: 'countries',
                                                link: '#/data/dimensions/regions',
                                                text: 'Regions',
                                                canAccess: onlyAdmin()
                                            },
                                            {
                                                class: 'countries',
                                                link: '#/data/dimensions/countries',
                                                text: 'Countries',
                                                canAccess: onlyDataAdministrator()
                                            },
                                            {
                                                class: 'products',
                                                link: '#/data/dimensions/products',
                                                text: 'Products',
                                                canAccess: onlyDataAdministrator()
                                            },
                                            {
                                                class: 'sku',
                                                link: '#/data/dimensions/sku',
                                                text: 'SKU',
                                                canAccess: onlyDataAdministrator()
                                            },
                                            {
                                                class: 'price-type',
                                                link: '#/data/dimensions/pricetypes',
                                                text: 'Price Types',
                                                canAccess: onlyDataAdministrator()
                                            },
                                            {
                                                class: 'currencies',
                                                link: '#/data/dimensions/currencies',
                                                text: 'Currencies',
                                                canAccess: onlyDataAdministrator()
                                            },
                                            {
                                                class: 'units',
                                                link: '#/data/dimensions/units',
                                                text: 'Units',
                                                canAccess: onlyDataAdministrator()
                                            }
                                        ]
                                    },
                                    {
                                        class: 'load',
                                        link: '#/data/load',
                                        text: 'Load',
                                        canAccess: onlyDataAdministrator()
                                    }
                                ]
                            },
                            {
                                class: 'dashboard',
                                text: 'Forecast',
                                canAccess: onlyGprmUsers(),
                                children: [
                                     {
                                     class: 'price',
                                         link: '#/forecast/prices',
                                         text: 'Prices',
                                         canAccess: onlyGprmUsers()
                                     },
                                    {
                                        class: 'volumes',
                                        link: '#/forecast/volumes',
                                        text: 'Volumes',
                                        canAccess: onlyGprmUsers()
                                    },
                                    {
                                    class: 'events',
                                        link: '#/forecast/events',
                                        text: 'Events',
                                        canAccess: onlyGprmUsers()
                                    },
                                    {
                                    class: 'rules',
                                        link: '#/forecast/rules',
                                        text: 'Rules',
                                        canAccess: onlyGprmUsers()
                                    },
                                    {
                                    class: 'analyzer',
                                        link: '#/forecast/analyzer',
                                        text: 'Analyzer',
                                        canAccess: onlyGprmUsers()
                                    },
                                    {
                                    class: 'launch',
                                        link: '#/forecast/launch',
                                        text: 'Launch',
                                        canAccess: onlyGprmUsers()
                                    },
                                    {
                                        class: 'simulation',
                                        link: '#/forecast/simulations',
                                        text: 'Simulations',
                                        canAccess: onlyGprmUsers()
                                    }
                                ]
                            },

                            {
                                class: 'settings',
                                text: 'Admin',
                                canAccess: onlyAdmin(),
                                children: [
                                    {
                                        class: 'users',
                                        link: '#/admin/users',
                                        text: 'Users',
                                        canAccess: onlyAdmin()
                                    },
                                    {
                                        class: 'settings',
                                        link: '#/admin/informations',
                                        text: 'Settings',
                                        canAccess: onlyAdmin()
                                    },
                                    {
                                        class: 'settings',
                                        link: '#/admin/requestaccesses',
                                        text: 'Access',
                                        canAccess: onlyAdmin()
                                    },
                                    {
                                        class: 'upload',
                                        link: '#/admin/xlsTemplates',
                                        text: 'Templates',
                                        canAccess: onlyAdmin()
                                    }
                                ]
                            }
                        ];

                        /*
                            Recursive function which initialize the menu by role
                        */
                        var getMenu = function (menuLevel, parent) {
                            for (var i = 0; i < menuLevel.length; i++) {
                                if (menuLevel[i].canAccess) {
                                    if (parent == null) {
                                        $scope.menuItems.push(menuLevel[i]);
                                    }
                                    if (menuLevel[i].children != null) {
                                        getMenu(menuLevel[i].children, menuLevel[i]);
                                    }
                                } else {
                                    if (parent != null) {
                                        parent.children = _.reject(parent.children, function (child) {
                                            return child.text == menuLevel[i].text;
                                        });
                                    }
                                }
                            }
                        };

                        getMenu($scope.allMenuItems, null);

                    });
                }
            };
        }]);

    /*
        Recursive function which set the showChildren attribute
        of item which the level is equal to "lvlToTest"
        listChildren : the list corresponding of the level we cibling
        lvlCurrent : the current level
        lvlToTest : the level for which we do the modification
    */
    function setShowMenu(listChildren, lvlCurrent, lvlToTest) {
        if (listChildren != undefined) {
            for (var index = 0; index < listChildren.length; ++index) {
                if (lvlCurrent >= lvlToTest) {
                    listChildren[index].showChildren = false;
                }
                setShowMenu(listChildren[index].children, lvlCurrent + 1, lvlToTest);
            }
        }
    }
});
