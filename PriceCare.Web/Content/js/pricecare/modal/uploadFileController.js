define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('UploadFileController', ['$scope', '$modalInstance', '$timeout', 'infos', '$q', '$upload',
        function ($scope, $modalInstance, $timeout, infos, $q, $upload) {

            $scope.isValidName = true;
            $scope.isValidFile = true;
            $scope.isValidFileExtension = true;
            $scope.isValidFileSize = true;
            $scope.siteId = infos.siteId;
            $scope.extentionLimited = false;
            $scope.extensions = infos.extensions;

            $scope.validation = function () {
                var deferred = $q.defer();

                $scope.isValidFile = $scope.fileUpload != null;
                if ($scope.isValidFile) {
                    $scope.isValidFileExtension = $scope.extensions.indexOf('.' + $scope.fileUpload.name.split('.').pop().toLowerCase()) != -1;
                    deferred.resolve($scope.isValidFile === true && $scope.isValidFileExtension === true);
                }

                return deferred.promise;
            };

            $scope.ok = function () {
                $scope.validation().then(function (result) {
                    if (result == true) {
                        $upload.upload({
                            url: infos.url,
                            method: 'POST',
                            file: $scope.fileUpload,
                            data: infos.data,
                            headers: { "Content-Type": 'multipart/form-data' }
                        }).progress(function (evt) {
                            $scope.progress = parseInt(100.0 * evt.loaded / evt.total);
                        }).success(function (cloudItemId) {
                            $modalInstance.close(cloudItemId);
                        }).error(function () {
                        });
                    }
                });
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

            $scope.onFileChange = function (file) {
                $scope.fileUpload = file;
            }
        }]);
});