define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opSelectionMultiLevel', ['$timeout', '$document', 'helperService',
        function ($timeout, $document, helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opSelectionMultiLevelTemplate.html",
                scope: {
                    items: "=",
                    selectionChanged: '&'
                },
                controller: function ($scope, $element, $attrs) {

                    $scope.showParent = true;

                    $scope.onParentClick = function(parent) {
                        $scope.lastParentSelected = parent;
                        $scope.showParent = false;
                    };

                    $scope.goBackToParents = function() {
                        $scope.showParent = true;
                    };

                    $scope.id = helperService.makeid();
                    $element.data('opFilterId', $scope.id);

                    $scope.showMenu = false;

                    $scope.getSelectedItemText = function () {
                        var selectedParent = _.find($scope.items, function (item) { return item.selected; });
                        var selectedChild;
                        if (selectedParent) {
                            selectedChild = _.find(selectedParent.items, function (item) { return item.selected; });
                        }
                        return (selectedParent ? (selectedParent.textShort || selectedParent.text) : "/") + " - " + (selectedChild ? (selectedChild.textShort || selectedChild.text) : "/");
                    };

                    $scope.toggleMenu = function () {

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

                        var previousChild = _.find($scope.lastParentSelected.items, function (item) {
                            return item.selected;
                        });
                        if (previousChild !== selectedItem) {
                            $scope.lastParentSelected.selected = true;
                            for (var i = 0; i < $scope.items.length; i++) { 
                                if ($scope.items[i] != $scope.lastParentSelected) {
                                    $scope.items[i].selected = false;
                                }
                                $scope.items[i].items.forEach(function (item) {
                                    if (item === selectedItem) {
                                        item.selected = true;
                                    } else {
                                        item.selected = false;
                                    }
                                });
                            }

                            $scope.showParent = true;
                            
                        }

                        if ($scope.selectionChanged) {
                            $scope.selectionChanged({ item: selectedItem });
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
                                    $scope.showParent = true;
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