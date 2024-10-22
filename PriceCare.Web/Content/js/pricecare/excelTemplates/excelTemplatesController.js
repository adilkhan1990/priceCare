define(['pricecare/excelTemplates/module'], function (module) {
    'use strict';
    module.controller('ExcelTemplatesController', [
        '$scope', '$rootScope', '$controller', '$q', 'helperService', 'excelService', '$modal',
    function ($scope, $rootScope, $controller, $q, helperService, excelService, $modal) {

        $scope.upload = function(template) {
            $scope.message = "";
            var modalInstance = $modal.open({
                templateUrl: 'Content/js/pricecare/modal/uploadFile.html',
                controller: "UploadFileController",
                backdrop: 'static',
                resolve: {
                    infos: function() {
                        return {
                            data: template,
                            url: 'api/excel/postXlsTemplate',
                            extensions: [template.extension]
                            //extensions: ['.xlsx', '.xlsm']
                        }
                    }
                }
            });

            modalInstance.result.then(function() {
                $scope.message = "File successfully uploaded.";
            }, function() {
                
            });
        };

        $scope.download = function(template) {            
            window.location.href = "api/excel/downloadTemplate?fileName=" + template.fileName;
        }

        var init = function() {
            excelService.getXlsTemplates().then(function(templates) {
                $scope.templates = templates;
            });
        };
        init();

    }
    ]);
});
