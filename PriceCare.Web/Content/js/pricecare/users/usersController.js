define(['pricecare/users/module'], function (module) {
    'use strict';
    module.controller('UsersController', [
        '$scope', '$rootScope', '$controller', '$http', 'helperService', 'userService', 'cacheService', '$q', '$modal', 'invitationService',
        function ($scope, $rootScope, $controller, $http, helperService, userService, cacheService, $q, $modal, invitationService) {

            var canceler; // to cancel previous search

            var statusFilter = {
                type: helperService.FieldTypes.Select,
                name: 'status',
                properties:
                {
                    class: 't-custom-select-boxed',
                    items: [
                        { text: "All status", id: 0 },
                        { text: "Deleted", id: 1 },
                        { text: "Locked", id: 2 },
                        { text: "Active", id: 3, selected: true }
                    ]
                }
            };

            var searchBox = {
                type: helperService.FieldTypes.Textbox,
                name: 'searchBox',
                class: 'search',
                properties: {
                    trigger: helperService.FieldTriggers.OnEnter | helperService.FieldTriggers.OnChange
                }
            };

            var roleFilter = {
                type: helperService.FieldTypes.Select,
                name: 'roles',
                properties:
                {
                    class: 't-custom-select-boxed',
                    items: []
                }
            };

            var prepareStatusForFilter = function(statusSelected) {
                var items = [
                    {
                        id: 0,
                        text: "Active",
                        textShort: "Active",
                        selected: statusSelected == "Active"
                    },
                    {
                        id: 1,
                        text: "Locked",
                        textShort: "Locked",
                        selected: statusSelected == "Locked"
                    },
                    {
                        id: 2,
                        text: "Deleted",
                        textShort: "Deleted",
                        selected: statusSelected == "Deleted"
                    }
                ];

                return items;
            };

            var initTable = function () {
                var rows = [{
                    title: true,
                    cells: [
                        {
                            title: true,
                            value: "Name"
                        },
                        {
                            title: true,
                            value: "Email"
                        },
                        {
                            title: true,
                            value: "Last connection date"
                        },
                        {
                            title: true,
                            value: "User status"
                        },
                        {
                            title: true,
                            value: "Action"
                        },
                        {
                            title: true,
                            value:"Roles"
                        }
                    ]
                }
                ];

                for (var i = 0; i < $scope.users.length; i++) {
                    var userStatus = $scope.users[i].isDeleted ? 'Deleted' : $scope.users[i].isUserLocked ? "Locked" : "Active";
                    rows.push(
                    {
                        originalUser: $scope.users[i],
                        user: helperService.clone($scope.users[i]),
                        cells: [
                            {
                                value: $scope.users[i].username
                            },
                            {
                                value: $scope.users[i].email,
                                rightMenu: {
                                    fields: [
                                        {
                                            type: helperService.FieldTypes.Textbox,
                                            name: "Email:",
                                            cellValue: $scope.users[i].email,
                                            properties: {
                                                required: true,
                                                focus: true,
                                                select:true,
                                                isValid: function(field, cell, row, table) {
                                                    return helperService.validateEmail(field.value);
                                                },
                                                isValidAsync: function(field, cell, row, table) {
                                                    var user = {
                                                        id: row.user.id,
                                                        email: field.value
                                                    };
                                                    return userService.isEmailUnique(user);
                                                }
                                            }
                                        }
                                    ],
                                    properties: {
                                        apply: function(fields, cell, row, table) {
                                            var user = {
                                                id: row.user.id,
                                                email: fields[0].value
                                            };
                                            cell.value = fields[0].value;
                                            userService.changeEmail(user).then(function(result) {
                                                $scope.onSearch();
                                            });
                                        },
                                        showReverseEdit: false
                                    }
                                }
                            },
                            {
                                value: $scope.users[i].lastConnectionDate,
                                format: helperService.formatDate
                            },
                            {
                                value: userStatus,
                                rightMenu: {
                                    fields: [
                                        {
                                            type: helperService.FieldTypes.Select,
                                            name: "status:",
                                            cellValue: userStatus,
                                            properties: {
                                                class: 't-custom-select-boxed',
                                                items: prepareStatusForFilter(userStatus)
                                            }
                                        }
                                    ],
                                    properties: {
                                        apply: function (fields, cell, row, table) {
                                            var userStatusSelected = fields[0].value == 0 ? "Active" : fields[0].value == 1 ? "Locked" : "Deleted";
                                            if (cell.value != userStatusSelected) {
                                                if (fields[0].value == 0) {
                                                    activateUser(row.user);
                                                } else if (fields[0].value == 1) {
                                                    lockUser(row.user);
                                                } else { // Deleted
                                                    deleteUser(row.user);
                                                }
                                            }
                                        },
                                        showReverseEdit: false
                                    }
                                }
                            },
                            {
                                actions: [
                                    {
                                        text: 'Send request password email',
                                        class: '',
                                        click: function(cell, row) {
                                            var passwordModel = {
                                                email: row.user.email
                                            };
                                            userService.requestResetPassword(passwordModel);
                                        }
                                    }
                                ]
                            },
                            {
                                actions: [
                                    {
                                        text: 'Details (' + $scope.users[i].roles.length + ")",
                                        class: '', 
                                        click: function (cell, row) {
                                            var modalInstance = $modal.open({
                                                templateUrl: 'Content/js/pricecare/modal/manageRoleModal.html',
                                                controller: "ManageRoleModalController",
                                                backdrop: 'static',
                                                resolve: {
                                                    infos: function () {
                                                        return {
                                                            user: row.user
                                                        };
                                                    }
                                                }
                                            });

                                            modalInstance.result.then(function (roles) {
                                                userService.updateRoles({ userId: row.user.id, roles: roles }).then(function () {
                                                    userService.getUserInfo().then(function(user) {
                                                        if (row.user.id == user.id) {
                                                            userService.loadUserInfo();
                                                        }
                                                    });
                                                    
                                                    $scope.onSearch();
                                                });
                                            }, function () {

                                            });
                                        }
                                    }
                                ]
                            }
                        ]
                    });
                };

                $scope.table = {
                    rows: rows,
                    paginationOptions: {
                        canLoadMore: $scope.canLoadMore,
                        getData: function() {
                            $scope.getUsers();
                        },
                        counterText: $scope.counter
                    }
                };

            };
            
            $scope.users = [];

            var searchRequest = {
                searchText: '',
                status: 0,
                pageNumber: 0,
                itemsPerPage: helperService.itemsPerPage,
            };

            var getSearchText = function () {
                var text = "";
                if (searchBox.value != null || searchBox.value != "")
                    text = searchBox.value;

                return text;
            };

            var getSelectedStatus = function () {
                var statusSelected = _.find(statusFilter.properties.items, function (statusSelectedTmp) {
                    return statusSelectedTmp != undefined && statusSelectedTmp.selected == true;
                });

                return statusSelected == null ? 0 : statusSelected.id;
            };

            var getSelectedRole = function () {
                var role = _.find(roleFilter.properties.items, function (roleTmp) {
                    return roleTmp.selected == true;
                });

                return role == null ? 0 : role.id;
            };

            $scope.onSearch = function() {
                searchRequest.pageNumber = 0;
                searchRequest.searchText = getSearchText();
                searchRequest.status = getSelectedStatus();
                searchRequest.roleId = getSelectedRole();
                $scope.getUsers();
            };
            
            $scope.getUsers = function () {
                if (canceler) {
                    canceler.resolve();
                    canceler = null;
                }
                canceler = $q.defer();
                
                userService.getUsers(searchRequest, canceler).then(function (result) {

                    searchRequest.pageNumber++;

                    if (result) {
                        if (result.page == 1) {
                            $scope.users = [];
                        }
                        $scope.users = $scope.users.concat(result.users);
                        $scope.canLoadMore = !result.isLastPage;
                        $scope.counter = "Load more users - " + $scope.users.length + " out of " + result.totalUsers;

                        initTable();
                    }

                    canceler = null;
                });
            };

            var initFilterOptions = function() {
                $scope.filterOptions = {
                    header: {
                        items: [
                            {
                                type: helperService.FieldTypes.Label,
                                properties:
                                {
                                    text: "Users"
                                }
                            }
                        ]
                    },
                    filters: [
                        roleFilter,
                        statusFilter,
                        searchBox
                    ],
                    onChanged: function() {
                        $scope.onSearch();
                    }
                };
            };

            var prepareRolesForFilter = function(datas) {
                var result = [];
                result.push({
                    id: 0,
                    text: "All roles",
                    name: "All roles",
                    selected: true
                });

                datas.forEach(function (data, i) {
                    var newRole = {
                        id: data.id,
                        text: data.name,
                        name: data.name.replace(/\s+/g, '')
                    };

                    result.push(newRole);
                });

                return result;
            };

            var init = function () {
                userService.getAllRoles().then(function(roles) {
                    roleFilter.properties.items = prepareRolesForFilter(roles);
                    initFilterOptions();
                    $scope.onSearch();
                });
            };
            init();

            var activateUser = function(user) {
                userService.activate(user).then(function () {
                    $scope.onSearch();
                });
            };

            var lockUser = function (user) {
                var lockInfo = { userId: user.id }
                userService.lock(lockInfo).then(function () {
                    $scope.onSearch();
                }, function () {
                    user.isUserLocked = !user.isUserLocked;
                });

            };
            // delete
            var deleteUser = function (user) {
                userService.delete(user).then(function () {
                    $scope.onSearch();
                });
            };

            $scope.invite = function () {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/invitationModal.html',
                    controller: 'InvitationModalController',
                    backdrop: 'static'
                });

                modalInstance.result.then(function (content) {
                    invitationService.create(content).then(function (result) {

                    });

                }, function () {

                });
            };

        }
    ]);
});

