define(['pricecare/services/module'], function(module) {
    'use strict';
    module.service('userService', [
        '$http', '$window', '$q', function($http, $window, $q) {

            var me = this;

            me.userInfo = null;
            var userInfoDeferred = [];
            me.userSettings = null;
            me.lastSettingsChosen = null;


            me.loadUserInfo = function () {
                $http.get("api/user/info").success(function (result) {
                    me.userInfo = result;

                    for (var i = 0, len = userInfoDeferred.length; i < len; i++) {
                        userInfoDeferred[i].resolve(me.userInfo);
                    }                    
                }).error(function () {
                    console.log("error");
                });
            };
            me.loadUserInfo();

            me.forceLoadUsers = function () {
                var deferred = $q.defer();
                $http.get("api/user/info").success(function (result) {
                    me.userInfo = result;
                    deferred.resolve(result);
                });
                return deferred.promise;
            };
            
            me.getUserInfo = function () {
                var deferred = $q.defer();

                $http.get("api/user/info").success(function (result) {
                    me.userInfo = result;
                    deferred.resolve(me.userInfo);
                });

                return deferred.promise;
            };
            me.getUsers = function (data, canceler) {
                var deferred = $q.defer();
                
                $http({
                    method: 'POST',
                    url: 'api/user/users/',
                    data: data,
                    timeout: canceler.promise
                }).success(function (result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            };

            me.userMappingPromise = null;
            me.userMapping = null;
            me.getUserMapping = function() {
                if (me.userMappingPromise == null) {
                    var deferred = $q.defer();
                    if (me.userMapping != null) {
                        deferred.resolve(me.userMapping);
                    } else {
                        $http.get("api/user/usermapping").success(function (result) {
                            me.siteTypes = result;
                            deferred.resolve(me.siteTypes);
                        });
                    }

                    me.userMappingPromise = deferred.promise;
                }
                return me.userMappingPromise;
            };

            var internalMapUser = function(userId, userMapping) {
                var user = _.find(userMapping, function(u) {
                    return u.id == userId;
                });

                if (user) {
                    if (user.firstName && user.lastName) {
                        return user.firstName[0] + ". " + user.lastName;
                    } else {
                        return user.email.split("@")[0];
                    }
                } else {
                    return userId;
                }
            }

            me.mapUser = function(userId, userMapping) {
                if (userMapping) {
                    return internalMapUser(userId, userMapping);
                } else {
                    var deferred = $q.defer();

                    me.getUserMapping().then(function(data) {
                        deferred.resolve(internalMapUser(userId, data));
                    });

                    return deferred.promise;
                }
            };

            me.logout = function () {
                $http.post('/account/logoff').success(function () {
                    $window.location.reload();
                });
            };
            me.lock = function (lockInfo) {
                var deferred = $q.defer();

                $http.post('api/user/lock', lockInfo).success(function (result) {
                    me.userInfo.isUserLocked = lockInfo.isLocked;
                    deferred.resolve(result);
                }).error(function (result) {
                    deferred.reject(result);
                });

                return deferred.promise;
            };
            me.delete = function (user) {
                var deferred = $q.defer();

                $http.post('api/user/delete', user).success(function(result) {
                    deferred.resolve(result);
                });

                return deferred.promise;
            };

            me.activate = function (user) {
                var deferred = $q.defer();

                $http.post('api/user/activate', user).success(function (result) {
                    deferred.resolve(result);
                });

                return deferred.promise;
            };
            me.isEmailUnique = function(user) {
                var deferred = $q.defer();

                $http.post('api/user/isEmailUnique', user).success(function (result) {
                    deferred.resolve(result);
                }).error(function (result) {
                    deferred.reject(result);
                });

                return deferred.promise;
            };
            me.changeEmail = function (user) {
                var deferred = $q.defer();

                $http.post('api/user/changeEmail', user).success(function (result) {
                    deferred.resolve(result);
                }).error(function(result) {
                    deferred.reject(result);
                });

                return deferred.promise;
            };

            me.saveUserInfo = function(userInfo) {
                var deferred = $q.defer();
                $http.post('api/user/save', userInfo).success(function(result) {
                    me.userInfo = result;
                    deferred.resolve(result);
                });
                return deferred.promise;
            };

            me.saveSettings = function(settings) {
                var deferred = $q.defer();
                $http.post('api/user/saveSettings', settings).success(function(result) {
                    //me.userInfo = result;
                    deferred.resolve(result);
                });
                return deferred.promise;
            }

            me.getUserSettings = function() {
                var deferred = $q.defer();
                $http.get('api/user/settings').success(function(result) {
                    me.userSettings = result;
                    me.lastSettingsChosen = result;
                    deferred.resolve(result);
                });
                return deferred.promise;
            }

            me.getLastSettingsChosen = function () {
                var deferred = $q.defer();

                if (me.lastSettingsChosen != null) {
                    deferred.resolve(me.lastSettingsChosen);
                } else {
                    me.getUserSettings().then(function (result) {
                        me.userSettings = result;
                        me.lastSettingsChosen = result;
                        deferred.resolve(result);
                    });
                }

                return deferred.promise;
            }

            me.createUserAndAssignRoles = function (data) {
                var deferred = $q.defer();

                $http.post('api/user/create', data).success(function(response) {
                    deferred.resolve(response);
                });

                return deferred.promise;
            }

            me.getAllRoles = function () {
                var deferred = $q.defer();

                $http.get('api/user/roles').success(function (response) {
                    deferred.resolve(response);
                });

                return deferred.promise;
            }
            
            
            me.updateRoles = function (data) {
                var deferred = $q.defer();

                $http.post('api/user/updateRoles', data).success(function (response) {
                    deferred.resolve(response);
                });

                return deferred.promise;
            };
            me.requestResetPassword = function (data) {
                var deferred = $q.defer();

                $http.post('api/user/requestResetPassword', data).success(function (response) {
                    deferred.resolve(response);
                });

                return deferred.promise;
            };
            
            me.isGPRMAdvancedUser = function() {
                for (var i = 0; i < me.userInfo.roles.length; i++) {
                    if (me.userInfo.roles[i] == "Advanced User")
                        return true;
                }
                return false;
            };
            me.isGPRMStandardUser = function() {
                for (var i = 0; i < me.userInfo.roles.length; i++) {
                    if (me.userInfo.roles[i] == "Standard User")
                        return true;
                }
                return false;
            };
            me.isGPRMViewerUser = function() {
                for (var i = 0; i < me.userInfo.roles.length; i++) {
                    if (me.userInfo.roles[i] == "Viewer")
                        return true;
                }
                return false;
            };
            me.isSystemAdministrator = function() {
                for (var i = 0; i < me.userInfo.roles.length; i++) {
                    if (me.userInfo.roles[i] == "System Admin")
                        return true;
                }
                return false;
            };
            me.isSuperAdministrator = function() {
                for (var i = 0; i < me.userInfo.roles.length; i++) {
                    if (me.userInfo.roles[i] == "Super Admin")
                        return true;
                }
                return false;
            };
            
             me.isDataAdministrator = function () {
                for (var i = 0; i < me.userInfo.roles.length; i++) {
                    if (me.userInfo.roles[i] == "Data Admin")
                        return true;
                }
                return false;
             };

             me.isValid = function (model) {
                 var deferred = $q.defer();

                 $http.post('api/user/isValid', model).success(function (result) {
                     deferred.resolve(result);
                 });

                 return deferred.promise;
             };
        }
    ]);
});