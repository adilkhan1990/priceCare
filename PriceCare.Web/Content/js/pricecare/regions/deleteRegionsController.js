define(['pricecare/regions/module'], function(module) {
    'use strict';
    module.controller('DeleteRegionsController', ['$scope', '$rootScope', 'helperService', 'countryService', '$modal',
        function($scope, $rootScope, helperService, countryService, $modal) {

            $scope.delete = function (region) {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/yesNoCancelModal.html',
                    controller: 'YesNoCancelModalController',
                    backdrop: 'static',
                    resolve: {
                        infos: function () {
                            return {
                                title: 'Warning',
                                content: "Are you sure you want to delete this ?"
                            }
                        }
                    }
                });
                modalInstance.result.then(function (confirm) {
                    if (confirm) {
                        countryService.deleteRegion(region).then(function (result) {
                            init();
                        });
                    }
                });
            }
            $scope.addRegion = function () {
                var modalR = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/addItemModal.html',
                    controller: 'AddItemModalController',
                    backdrop: 'static',
                    resolve: {
                        itemName: function () {
                            return 'Region';
                        }
                    }
                });
                modalR.result.then(function (regionName) {
                    countryService.addRegion({ name: regionName }).then(function (result) {
                        if (result == 'true') {
                            init();
                        }
                    });
                });
            };



            var setTable = function() {
                var rows = [];

                rows.push({
                    title: true,
                    cells: [
                        { title: true, value: "Name" },
                        { title: true, value: "Actions" }
                    ]
                });

                $scope.regions.forEach(function(region) {
                    rows.push({
                        region: region,
                        cells: [
                            { title: false, value: region.name },
                           getCellActions(region)
                        ]
                    });
                });

                $scope.table = {
                    rows: rows
                }

            };
            var getCellActions = function (region) {
                var cellActions = {
                    actions: []
                }

                if (region.name != 'Undefined') {
                    cellActions.actions.push({
                        text: 'delete',
                        class: 'icon icon-delete',
                        click: function(cell, row) {
                            $scope.delete(row.region);
                        }
                    });
                }

                return cellActions;
            };
            
            var init = function () {
                countryService.getAllRegions().then(function (regions) {
                    $scope.regions = regions;
                    setTable();
                });                
            };
            init();
        }
    ]);
});