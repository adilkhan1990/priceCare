define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opFilter', ['helperService',
        function (helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opFilterTemplate.html",
                scope: {
                    options: "="
                },
                controller: function ($scope, $element, $attrs) {
                    $scope.helperService = helperService;

                    $scope.getSelectedItem = function(list) {
                        return _.first(list, function(item) { return item.selected; });
                    };

                    $scope.toggleDisplayOptions = function() {
                        $scope.options.header.showDisplayOptions = !$scope.options.header.showDisplayOptions;
                    };

                    $scope.toggleAdvancedFilters = function() {
                        $scope.options.showAdvancedFilters = !$scope.options.showAdvancedFilters;
                    };
                }
            };
        }
    ]);



    module.directive('testDirective', [
        function () {
            return {
                restrict: 'A',
                //  replace: true,
                template: '<div>I am an injected test</div>',
                scope: {
                    options: "="
                },
                controller: function ($scope, $element, $attrs) {
                    console.log('from child directive');
                }
            };
        }
    ]);
});