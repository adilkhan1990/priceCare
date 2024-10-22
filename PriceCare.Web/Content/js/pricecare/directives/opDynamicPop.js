define(['pricecare/directives/module'], function(module) {
    'use strict';
    module.directive('opDynamicPop', [
        '$timeout', '$document', 'helperService',
        function($timeout, $document, helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opDynamicPopTemplate.html",
                scope: {
                    options: "=",
                    onChange: '&'
                },
                controller: function ($scope, $element, $attrs) {

                    $scope.dynamicOptions = { directive: $scope.options.properties.directive, options: $scope.options }

                    $scope.id = helperService.makeid();
                    $element.data('opFilterId', $scope.id);

                    $scope.showMenu = false;

                    angular.element($document[0].body).on('click', function(e) {
                        // The click event on body is coming from outside the angular scope so we need to get back into the scope with $scope.$apply()
                        $scope.$apply(function() {
                            if ($scope.showMenu) {
                                var opFilterId = angular.element(e.target).inheritedData('opFilterId');
                                if (opFilterId == $scope.id) {

                                } else {
                                    $scope.showMenu = false;
                                }
                            }
                        });
                    });

                    $scope.getSelectedItemText = function() {
                        var selected = _.find($scope.items, function(item) { return item.selected; });
                        if (selected)
                            return selected.textShort || selected.text;
                        return "";
                    };

                    $scope.toggleMenu = function() {

                        $scope.showMenu = !$scope.showMenu;

                        // position the drop down properly
                        if ($scope.showMenu) {

                            var elementRect = $element[0].getBoundingClientRect();
                            $scope.options.parentRect = elementRect;


                            var dropDownSizeEm = 15; // ADD POSSIBILITY TO CONFIGURE THIS
                            var baseFontSize = 14.4;//parseFloat(getComputedStyle(document.body).fontSize);

                            var dropDownSizePx = dropDownSizeEm * baseFontSize;

                            var alignElement = $element.children(document.querySelector(".m-custom-select-dropdown-align"));
                            var width = alignElement[0].offsetWidth;

                            var shift = 0 - (dropDownSizePx / 2) + (width / 2);

                            $scope.dropStyle = { left: shift + 'px' };
                        }
                    };

                    $scope.getText = function() {
                        if ($scope.options.properties.getText)
                            return $scope.options.properties.getText();

                        return "";
                    };

                    $scope.onApply = function() {
                        if ($scope.onChange) {
                            $scope.onChange();
                        }
                        $scope.showMenu = false;
                    };
                }

            };
        }
    ]);
});