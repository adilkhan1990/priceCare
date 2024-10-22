define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('inputFile', ['$timeout', function ($timeout) {
            return {
                restrict: 'A',
                replace: true,
                template:
                    '<div class="upload">'+
                    '   <button ng-click="FileClick()" type="button" class="button fileupload">Upload</button>'+
                    '   <input type="text" value="{{filename}}"  disabled="disabled">'+
                    '   <div style="position:absolute;left:-1000px;top:-1000px;">'+
                    '       <input id="input-file" type="file" file-change="onFileChange" style="visibility: hidden;" />' +
                    '   </div>'+
                    '</div>',
                scope: {
                    onfilechange: "&"
                },
                controller: function ($scope, $element, $attrs) {

                    $scope.filename = "No file chosen";

                    $scope.FileClick = function () {
                        $timeout(function () {
                            angular.element(document.querySelector("#input-file")).focus().click();
                        });
                    };

                    $scope.onFileChange = function (element) {
                        if (element != null && element.files.length > 0) {
                            $scope.$apply(function() {
                                $scope.file = element.files[0];
                            });
                            $scope.filename = $scope.file.name;
                            $scope.onfilechange({file: $scope.file});
                        }
                    };

                }
            }
        }
    ]);
});