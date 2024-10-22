define(['pricecare/load/module'], function (module) {
    'use strict';
    module.controller('LoadController', [
        '$scope', '$rootScope', '$controller', '$http', 'helperService', 'userService', 'loadService',
        function ($scope, $rootScope, $controller, $http, helperService, userService, loadService) {

            $scope.searchRequest = {
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage,                
            }

            $scope.loads = [];

            $scope.refresh = function() {
                getLoads();
            };

            $scope.create = function() {
                var loadId = loadService.postNewLoad('New load');
                onSearch();
            };

            var getSelectedStatusId = function () {
                var statusSelected = _.find($scope.filterOptions.filters[0].properties.items, function (status) {
                    return status.selected == true;
                });
                return statusSelected.id;
            };

            var getLoads = function() {
                loadService.getLoads($scope.searchRequest).then(function(result) {
                    $scope.loads = result.loads;
                    $scope.canStartLoad = !_.some($scope.loads, function (l) { return l.status == 2; });
                    var canLoadMore = !result.isLastPage;
                    var counterText = "More loads - " + $scope.loads.length + " out of " + result.totalLoads;
                    setPaginationOptions(canLoadMore, counterText);
                });
            };

            $scope.cancel = function (load)
            {
                loadService.cancelLoad(load.id).then(function (result) {
                    load.status = 5;
                });
            }

            var onSearch = function () {
                resetPaging();
                $scope.searchRequest.statusId = getSelectedStatusId();
                getLoads();
                //var statusId = getSelectedStatusId();
                //$scope.resetPaging();
                //getLoads(statusId);                
            };

            var resetPaging = function () {
                $scope.searchRequest.pageNumber = 0;
                $scope.loads = [];
            }

            var setPaginationOptions = function(canLoadMore, counterText) {
                $scope.paginationOptions = {
                    canLoadMore: canLoadMore,
                    getData: function() {
                        $scope.searchRequest.pageNumber++;
                        getLoads();
                    },
                    counterText: counterText
                }                    
            };

            var init = function () {
                loadService.getLoadStatus().then(function (result) {
                    getLoads(0);
                    $scope.filterOptions = {
                        header: {
                            items: [
                                 {
                                     type: helperService.FieldTypes.Label,
                                     properties:
                                     {
                                         text: "Loads for..."
                                     }
                                 }
                            ]

                        },
                        filters: [
                            {
                                type: helperService.FieldTypes.Select,
                                name: 'loadstatus',
                                properties: {
                                    class: 't-custom-select-boxed',
                                    items: result
                                }
                            }
                        ],
                        showAdvancedFilters: false,
                        onChanged: function () {
                            onSearch();
                        }
                    };
                });
                
            };

            init();
        }
    ]);
});
