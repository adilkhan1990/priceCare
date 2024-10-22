define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opFilterSelect', ['$timeout', '$document', 'helperService',
        function ($timeout, $document, helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opFilterSelectTemplate.html",
                scope: {
                    items: "=",
                    smallEnabled: "=",
                    selectionChanged: '&'
                },
                controller: function ($scope, $element, $attrs) {

                    if ($scope.smallEnabled) {
                        $scope.mode = 1;
                    } else {
                        $scope.mode = 0;
                    }

                    $scope.id = helperService.makeid();
                    $element.data('opFilterId', $scope.id);

                    $scope.showMenu = false;

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
                            var dropDownSizeEm = 15; // ADD POSSIBILITY TO CONFIGURE THIS
                            var baseFontSize = 14.4;//parseInt(getComputedStyle(document.body).fontSize);

                            var dropDownSizePx = dropDownSizeEm * baseFontSize;

                            var alignElement = $element.children(document.querySelector(".m-custom-select-dropdown-align"));
                            var width = alignElement[0].offsetWidth;

                            var shift = 0 - (dropDownSizePx / 2) + (width / 2);

                            $scope.dropStyle = { left: shift + 'px' };
                        }
                        toggleDocumentClickEvent();
                    };

                    $scope.onItemClicked = function (selectedItem) {

                        var previousItem = _.find($scope.items, function(item) {
                            return item.selected;
                        });

                        if (previousItem !== selectedItem) {

                            $scope.items.forEach(function (item) {
                                if (item === selectedItem) {
                                    item.selected = true;
                                } else {
                                    item.selected = false;
                                }
                            });

                            if ($scope.selectionChanged) {
                                $scope.selectionChanged({ item: selectedItem });
                            }
                        }

                        $scope.showMenu = false;
                    };

                    var onDocumentClick = function (e) {
                        // The click event on body is coming from outside the angular scope so we need to get back into the scope with $scope.$apply()
                        $scope.$apply(function () {
                            if ($scope.showMenu) {
                                var opFilterId = angular.element(e.target).inheritedData('opFilterId');
                                if (opFilterId == $scope.id) {

                                } else {
                                    $scope.showMenu = false;
                                    toggleDocumentClickEvent();
                                }
                            }
                        });
                    }

                    var toggleDocumentClickEvent = function () {
                        if ($scope.showMenu) {
                            if (!$scope.clickAttached) {
                                angular.element($document[0].body).on('click', onDocumentClick);
                                angular.element($document[0].body).on('contextmenu', onDocumentClick);
                                $scope.clickAttached = true;
                            }
                        } else {
                            angular.element($document[0].body).off('click', onDocumentClick);
                            angular.element($document[0].body).off('contextmenu', onDocumentClick);
                            $scope.clickAttached = false;
                        }
                    };
                }
            }
        }
    ]);
});