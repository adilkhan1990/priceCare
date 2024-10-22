define(['pricecare/app'], function (app) {
    'use strict';
    return app.config([
        '$routeProvider', function ($routeProvider) {
            $routeProvider.
                // HOME
                when('/', {
                    templateUrl: 'Content/js/pricecare/home/home.html',
                    controller: 'HomeController'
                }).                
                // DESIGN
                when('/design1', {
                    templateUrl: 'Content/js/pricecare/design/design1.html',
                    controller: 'Design1Controller'
                }).
                when('/design2', {
                    templateUrl: 'Content/js/pricecare/design/design2.html',
                    controller: 'Design2Controller'
                }).
                when('/design3', {
                    templateUrl: 'Content/js/pricecare/design/design3.html',
                    controller: 'Design3Controller'
                }).
                when('/design4', {
                    templateUrl: 'Content/js/pricecare/design/design4.html',
                    controller: 'Design4Controller'
                }).
                when('/design5', {
                    templateUrl: 'Content/js/pricecare/design/design5.html',
                    controller: 'Design5Controller'
                }).
                //DATA
                // PRICES
                when('/data/prices/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/prices/prices.html',
                    controller: 'PricesController',
                    resolve: {
                        validate: function() {
                            return true;
                        }
                    }
                }).
                when('/data/prices', {
                    templateUrl: 'Content/js/pricecare/prices/prices.html',
                    controller: 'PricesController',
                    resolve: {
                        validate: function () {
                            return false;
                        }
                    }
                }).
                when('/data/volumes/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/volumes/volumes.html',
                    controller: 'VolumeController',
                    resolve: {
                        validate: function () {
                            return true;
                        }
                    }
                }).
                when('/data/volumes', {
                    templateUrl: 'Content/js/pricecare/volumes/volumes.html',
                    controller: 'VolumeController',
                    resolve: {
                        validate: function () {
                            return false;
                        }
                    }
                })
                .when('/data/events/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/events/events.html',
                    controller: 'EventController',
                    resolve: {
                        validate: function() {
                            return true;
                        }
                    }
                })
                .when('/data/events', {
                    templateUrl: 'Content/js/pricecare/events/events.html',
                    controller: 'EventController',
                    resolve: {
                        validate: function() {
                            return false;
                        }
                    }
                })
                .when('/data/rules/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/rules/rules.html',
                    controller: 'RulesController',
                    resolve: {
                        forecast: function () {
                            return false;
                        },
                        validate: function () {
                            return true;
                        }
                    }
                })
                .when('/data/rules', {
                    templateUrl: 'Content/js/pricecare/rules/rules.html',
                    controller: 'RulesController',
                    resolve: {
                        forecast: function () {
                            return false;
                        },
                        validate: function () {
                            return false;
                        }
                    }
                })
                .when('/forecast/rules', {
                    templateUrl: 'Content/js/pricecare/rules/rules.html',
                    controller: 'RulesController',
                    resolve: {
                        forecast: function () {
                            return true;
                        },
                        validate: function () {
                            return false;
                        }
                    }
                }).
                when('/data/load', {
                    templateUrl: 'Content/js/pricecare/load/load.html',
                    controller: 'LoadController'
                }).
                when('/data/load/:id', {
                    templateUrl: 'Content/js/pricecare/load/loadDetail.html',
                    controller: 'LoadDetailController'
                }).
                //DIMENSION DICTIONARY
                when('/data/dimensionDictionary', {
                    templateUrl: 'Content/js/pricecare/dimensionDictionary/dimensionDictionary.html',
                    controller: 'DimensionDictionaryController',
                    reloadOnSearch: false
                }).
                //VOLUME SCENARIO
                when('/data/loadVolumeScenario', {
                    templateUrl: 'Content/js/pricecare/load/volumeScenario.html',
                    controller: 'VolumeScenarioController'
                }).
                //DIMENSIONS
                when('/data/dimensions/products/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/products/products.html',
                    controller: 'ProductsController',
                    resolve: {
                        validate: function () {
                            return true;
                        }
                    }
                }).
                when('/data/dimensions/products', {
                    templateUrl: 'Content/js/pricecare/products/products.html',
                    controller: 'ProductsController',
                    resolve: {
                        validate: function () {
                            return false;
                        }
                    }
                }).
                when('/data/dimensions/countries/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/countries/countries.html',
                    controller: 'CountriesController',
                    resolve: {
                        validate: function () {
                            return true;
                        }
                    }
                }).
                when('/data/dimensions/countries', {
                    templateUrl: 'Content/js/pricecare/countries/countries.html',
                    controller: 'CountriesController',
                    resolve: {
                        validate: function () {
                            return false;
                        }
                    }
                }).
                // REGION
                when('/data/dimensions/regions', {
                    templateUrl: 'Content/js/pricecare/regions/deleteRegions.html',
                    controller: 'DeleteRegionsController'
                }).
                // PRICE TYPES
                when('/data/dimensions/pricetypes/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/pricetypes/priceTypes.html',
                    controller: 'PriceTypesController',
                    resolve: {
                        validate: function () {
                            return true;
                        }
                    }
                }).
                when('/data/dimensions/pricetypes', {
                    templateUrl: 'Content/js/pricecare/pricetypes/priceTypes.html',
                    controller: 'PriceTypesController',
                    resolve: {
                        validate: function () {
                            return false;
                        }
                    }
                }).
                // PRICE GUIDANCE
                when('/data/dimensions/pricemap', {
                    templateUrl: 'Content/js/pricecare/priceguidance/priceGuidance.html',
                    controller: 'PriceGuidanceController'
                }).
                
                // LIST TO SALES
                when('/data/dimensions/listToSales/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/listToSales/listToSales.html',
                    controller: 'ListToSalesController',
                    resolve: {
                        validate: function () {
                            return true;
                        }
                    }
                }).
                when('/data/dimensions/listToSales', {
                    templateUrl: 'Content/js/pricecare/listToSales/listToSales.html',
                    controller: 'ListToSalesController',
                    resolve: {
                        validate: function() {
                            return false;
                        }
                    }
                }).
                // SKU
                when('/data/dimensions/sku/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/sku/sku.html',
                    controller: 'SkuController',
                    resolve: {
                        validate: function () {
                            return true;
                        }
                    }
                }).
                when('/data/dimensions/sku', {
                    templateUrl: 'Content/js/pricecare/sku/sku.html',
                    controller: 'SkuController',
                    resolve: {
                        validate: function () {
                            return false;
                        }
                    }
                }).
                when('/data/dimensions/currencies/validate/:loadId', {
                    templateUrl: 'Content/js/pricecare/currencies/currencies.html',
                    controller: 'CurrenciesController',
                    resolve: {
                        validate: function() {
                            return true;
                        }
                    }
                }).
                when('/data/dimensions/currencies', {
                    templateUrl: 'Content/js/pricecare/currencies/currencies.html',
                    controller: 'CurrenciesController',
                    resolve: {
                        validate: function () {
                            return false;
                        }
                    }
                }).
                when('/data/dimensions/units', {
                    templateUrl: 'Content/js/pricecare/units/units.html',
                    controller: 'UnitsController',
                }).
                when('/user/settings', {
                    templateUrl: 'Content/js/pricecare/users/userSettings.html',
                    controller: 'UserSettingsController'
                }).
                when('/user/changePassword', {
                    templateUrl: 'Content/js/pricecare/users/changePassword.html',
                    controller: 'ChangePasswordController'
                }).
                // LIST TO SALES IMPACT
                when('/data/dimensions/listToSalesImpact/:geographyId?/:productId?/:versionId?', {
                    templateUrl: 'Content/js/pricecare/listToSalesImpact/listToSalesImpact.html',
                    controller: 'ListToSalesImpactController'
                }).
                //ADMIN
                when('/admin/users', {
                    templateUrl: 'Content/js/pricecare/users/users.html',
                    controller: 'UsersController'
                }).
                when('/admin/users/create', {
                    templateUrl: 'Content/js/pricecare/users/addUser.html',
                    controller: 'AddUserController'
                }).
                when('/admin/informations', {
                    templateUrl: 'Content/js/pricecare/informations/informations.html',
                    controller: 'InformationsController'
                }).
                 when('/admin/requestaccesses', {
                     templateUrl: 'Content/js/pricecare/requestaccess/requestAccesses.html',
                     controller: 'RequestAccessesController'
                 }).
                when('/admin/xlsTemplates', {
                    templateUrl: 'Content/js/pricecare/excelTemplates/excelTemplates.html',
                    controller: 'ExcelTemplatesController'
                }).
                //FORECAST
                when('/forecast/prices', {
                    templateUrl: 'Content/js/pricecare/forecast/pricesForecast.html',
                    controller: 'PricesForecastController'
                }).
                 when('/forecast/volumes', {
                     templateUrl: 'Content/js/pricecare/forecast/volumesForecast.html',
                     controller: 'VolumesForecastController'
                 }).
                when('/forecast/events', {
                    templateUrl: 'Content/js/pricecare/forecast/eventsForecast.html',
                    controller: 'EventsForecastController'
                }).
                when('/forecast/analyzer', {
                    templateUrl: 'Content/js/pricecare/forecast/analyzerForecast.html',
                    controller: 'AnalyzerForecastController'
                }).
                when('/forecast/launch', {
                    templateUrl: 'Content/js/pricecare/forecast/launchForecast.html',
                    controller: 'LaunchForecastController'
                }).
                when('/forecast/simulations', {
                    templateUrl: 'Content/js/pricecare/forecast/simulations.html',
                    controller: 'SimulationsController'
                }).
                //ERRORS
                when('/notFound', {
                    templateUrl: 'Content/js/pricecare/errors/notFound.html',
                    controller: 'NotFoundController'
                }).
                otherwise({
                    redirectTo: '/notFound'
                });
        }
    ]);
});