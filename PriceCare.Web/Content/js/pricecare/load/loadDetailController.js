define(['pricecare/load/module'], function (module) {
    'use strict';
    module.controller('LoadDetailController', [
        '$scope', '$rootScope', '$controller', '$routeParams', '$http', 'helperService', 'userService', 'loadService', '$modal',
        function ($scope, $rootScope, $controller, $routeParams, $http, helperService, userService, loadService, $modal) {

            $scope.loadId = $routeParams.id;
           
            $scope.skip = function (itemId)
            {
                loadService.validateLoadItem(itemId).then(function () {
                    init();
                });
            }

            $scope.upload = function () {
                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/uploadFile.html',
                    controller: "UploadFileController",
                    backdrop: 'static',
                    resolve: {
                        infos: function () {
                            return {
                                siteId: -1,
                                url: 'api/load/excel/post/sku',
                                extensions: ['.xlsx']
                            }
                        }
                    }
                });

                modalInstance.result.then(function () {
                    init();                   
                }, function () {

                });
            };

            var getLoadDetails = function() {
                loadService.getLoadDetail($routeParams.id).then(function (result) {
                    $scope.detail = result;
                    var validated = _.filter(result.items, function (data) {
                        return data.status == 3;
                    });

                    if (validated.length == 0)
                        $scope.maxValidated = 1;
                    else {
                        var max = _.max(validated, function (data) {
                            return data.step;
                        }).step;
                        if (_.every(_.filter(result.items, function (i) { return i.step == max; }), function (d) { return d.status == 3; })) {
                            $scope.maxValidated = max + 1;
                        }
                        else {
                            $scope.maxValidated = max;
                        }

                    }
                });
            };
 
            var init = function () {
                getLoadDetails();
            };
            init();
        }
    ]);
});
