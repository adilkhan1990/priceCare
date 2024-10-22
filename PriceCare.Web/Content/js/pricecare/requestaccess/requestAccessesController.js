define(['pricecare/requestaccess/module'], function (module) {
    'use strict';
    module.controller('RequestAccessesController', ['$scope', 'helperService', 'cacheService', 'requestAccessService',
        function ($scope, helperService, cacheService, requestAccessService) {


            var initTable = function () {
                var rows = [{
                    title: true,
                    cells: [
                        {
                            title: true,
                            value: "Email"
                        },
                        {
                            title: true,
                            value: "Reason"
                        },
                        {
                            title: true,
                            value: "Status"
                        },
                        {
                            title: true,
                            value: "User status changed"
                        },
                        {
                            title: true,
                            value: "Date status changed"
                        },
                        {
                            title: true,
                            value: "Action"
                        }
                    ]
                }
                ];

                for (var i = 0; i < $scope.requests.length; i++) {
                    rows.push(
                    {
                        request : $scope.requests[i],
                        cells: [
                            {
                                value: $scope.requests[i].email
                            },
                            {
                                value: $scope.requests[i].reason
                            },
                            {
                                value: $scope.getStatusName($scope.requests[i].status)
                            },
                            {
                                value: $scope.requests[i].userNameStatusChanged,
                            },
                            {
                                value: $scope.requests[i].dateStatusChanged,
                                format: helperService.formatDate
                            },
                            $scope.getActions($scope.requests[i])
                    ]
                });
                };

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function () {
                            $scope.getRequests();
                        },
                        counterText: $scope.counter
                    }
                };

            };

            $scope.requests = [];

            $scope.searchRequest = {
                status: 0,
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage
            };

            $scope.getActions = function (request) {
                var cellActions = { actions: [] };

                if (request.status == 1 ) { // reject || new
                    cellActions.actions.push({
                        text: 'Reject',
                        class: 'button button-red button-small-padding text-align-center',
                        click: function (cell, row) {
                            $scope.reject(row.request);
                        }
                    });
                    cellActions.actions.push({
                        text: 'Accept',
                        class: 'button button-green button-small-padding text-align-center',
                        click: function (cell, row) {
                            $scope.accept(row.request);
                        }
                    });
                }

                return cellActions;
            };


            var getSelectedStatus = function() {
                var status = _.find($scope.filterOptions.filters[0].properties.items, function(s) {
                    return s.selected == true;
                });
                return (status == null)? 0 : status.id;
            };

            var prepareRequestAccessStatus = function(status) {
                var requestAccessStatus = [];
                requestAccessStatus.push({
                    text: "All status",
                    id: 0
                });

                _.each(status, function(s) {
                    requestAccessStatus.push({
                        text: s.name,
                        id:s.id
                    });
                });

                if (requestAccessStatus.length > 0) {
                    requestAccessStatus[1].selected = true;
                } else {
                    requestAccessStatus[0].selected = true;
                }

                return requestAccessStatus;
            };

            $scope.getStatusName = function(statusId) {
                var status = _.find($scope.filterOptions.filters[0].properties.items, function (s) {
                    return s.id == statusId;
                });
                return (status == null) ? "" : status.text;
            };

            $scope.getRequests = function() {
                requestAccessService.getPagedRequestAccesses($scope.searchRequest).then(function (result) {

                    if (result) {
                        $scope.requests = $scope.requests.concat(result.requestAccesses);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more request accesses - "+$scope.requests.length + " out of " + result.totalRequestAccess;
                        initTable();
                    }

                    $scope.searchRequest.pageNumber++;
                });

            };

            var resetPaging = function() {
                $scope.requests = [];
                $scope.searchRequest.pageNumber = 0;
            };

            $scope.onSearch = function() {
                resetPaging();
                $scope.searchRequest.status = getSelectedStatus();
                $scope.getRequests();
            };

            var initFilterOptions = function () {
                cacheService.GetRequestAccessStatus().then(function (result) {
                    var status = prepareRequestAccessStatus(result);

                    $scope.filterOptions = {
                        header: {
                            items: [
                                {
                                    type: helperService.FieldTypes.Label,
                                    properties:
                                    {
                                        text: "Request access"
                                    }
                                }
                            ]
                        },
                        filters: [
                            {
                                type: helperService.FieldTypes.Select,
                                name: 'status',
                                properties:
                                {
                                    class: 't-custom-select-boxed',
                                    items: status
                                }
                            }
                        ],
                        onChanged: function () {
                            $scope.onSearch();
                        }
                    };

                    $scope.onSearch();
                });
            };

            var init = function () {
                $scope.counter = "";
                initFilterOptions();
            };
            init();


            // accept request
            $scope.accept = function (request) {
                var info = { id: request.id, status: 2 }
                requestAccessService.changeStatus(info).then(function () {
                    $scope.onSearch();
                }, function () {
                    user.isUserLocked = !user.isUserLocked;
                });

            };

            // reject request
            $scope.reject = function(request) {
                var info = { id: request.id, status: 3 }
                requestAccessService.changeStatus(info).then(function () {
                    $scope.onSearch();
                }, function () {
                    user.isUserLocked = !user.isUserLocked;
                });
            }
           

        }]);
});