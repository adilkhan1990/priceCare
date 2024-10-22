define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('ManageRoleModalController', ['$scope', '$modalInstance', 'userService', 'infos', '$q',
        function ($scope, $modalInstance, userService, infos, $q) {

            // Validation
            $scope.isValidRole = true;
            $scope.isValidRolePersonalChange = true;

            var userInfo = infos.user;
            var userRoles = [];
            for (var roleIndice in userInfo.roles) {
                userRoles.push(userInfo.roles[roleIndice]);
            }

            var rolePromise = userService.getAllRoles();
            var userPromise = userService.getUserInfo();

            $q.all([rolePromise,userPromise]).then(function (data) {
                $scope.roles = data[0];
                $scope.user = data[1];
                $scope.rolesSelected = [];
                
                for (var j = 0; j < $scope.roles.length; j++) {
                    var roleFound = false;
                    for (var i = 0; i < userRoles.length && !roleFound; i++) {
                        roleFound = ($scope.roles[j].name == userRoles[i]);
                    }
                    $scope.rolesSelected.push(roleFound);
                }
            });

            $scope.ok = function () {
                $scope.isValidRole = _.any($scope.rolesSelected, function(roleSelected) { return roleSelected; });
                if ($scope.isValidRole) {
                    var result = [];
                    for (var i = 0; i < $scope.roles.length; i++) {
                        if ($scope.rolesSelected[i]) result.push($scope.roles[i]);
                    }
                    if (userInfo.id == $scope.user.id) {
                        var isAdmin = _.any($scope.user.roles, function(role) { return role == "Super Admin"; });
                        var adminChosen = _.any(result, function(role) { return role.name == "Super Admin"; });
                        var isSystemAdministrator = _.any($scope.user.roles, function (role) { return role == "System Admin"; });
                        var systemAdministratorChosen = _.any(result, function (role) { return role.name == "System Admin"; });
                        $scope.isValidRolePersonalChange = ((isAdmin && adminChosen) || !isAdmin)
                            && ((isSystemAdministrator && systemAdministratorChosen) || !isSystemAdministrator);
                    }
                    
                    if ($scope.isValidRolePersonalChange) {
                        $modalInstance.close(result);
                    }
                }
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

        }]);
});