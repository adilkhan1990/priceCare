define(['pricecare/directives/table/module'], function (module) {
    'use strict';
    module.directive('opCell', [
        '$document', 'helperService',
        function ($document, helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/table/opCellTemplate.html",
                scope: {
                    cell: "=",
                    row: "=",
                    table: "="
                },
                controller: function ($scope, $element, $attrs) {

                    $scope.cell.onClose = function() {
                        $scope.closeFlyouts();
                    };

                    $scope.showLeftMenu = false;
                    $scope.showRightMenu = false;

                    $scope.onclick = function (evt) {
                        $scope.showLeftMenu = !$scope.showLeftMenu;
                        $scope.showRightMenu = false;

                        if ($scope.cell && $scope.cell.leftMenu) {
                            $scope.cell.leftMenu.targetRect = evt.target.getBoundingClientRect();
                        }

                        toggleDocumentClickEvent();
                    };

                    $scope.onrightclick = function (evt) {
                        $scope.showRightMenu = !$scope.showRightMenu;
                        $scope.showLeftMenu = false;

                        if ($scope.cell && $scope.cell.rightMenu) {
                            //$scope.cell.rightMenu.targetRect = evt.target.getBoundingClientRect();
                        }

                        toggleDocumentClickEvent();
                    };

                    $scope.id = helperService.makeid();
                    $element.data('opCellId', $scope.id);

                    var onDocumentClick = function(e) {
                        // The click event on body is coming from outside the angular scope so we need to get back into the scope with $scope.$apply()
                        $scope.$apply(function() {
                            if ($scope.showLeftMenu || $scope.showRightMenu) {
                                var opCellId = angular.element(e.target).inheritedData('opCellId');
                                if (opCellId == $scope.id) {

                                } else {
                                    $scope.showLeftMenu = false;
                                    $scope.showRightMenu = false;
                                    toggleDocumentClickEvent();
                                }
                            }
                        });
                    }

                    $scope.closeFlyouts = function() {
                        $scope.showLeftMenu = false;
                        $scope.showRightMenu = false;
                        toggleDocumentClickEvent();
                    }

                    var toggleDocumentClickEvent = function () {
                        if ($scope.showLeftMenu || $scope.showRightMenu) {
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

                    $scope.actionClick = function (cell, row, table, action) {
                        closeFlyouts();
                        action.click(cell, row, table, action);
                    }

                    $scope.getCellValue = function() {
                        if ($scope.cell.format) {
                            return $scope.cell.format($scope.cell.value);
                        } else if ($scope.row.format) {
                            return $scope.row.format($scope.cell.value);
                        } else if ($scope.table.format) {
                            return $scope.table.format($scope.cell.value);
                        }
                        return $scope.cell.value;
                    };

                    var perceivedBrightness = function(r, g, b) {
                        return Math.sqrt(
                            r * r * .241 +  // I hate those magic numbers
                            g * g * .691 +  // but I found it on the web
                            b * b * .068);  // and it works very well...
                    }

                    var getForegroundForBackground = function(background) {
                        if (background && background[0] == '#' && background.length == 7) {
                            var rT = background[1] + background[2];
                            var gT = background[3] + background[4];
                            var bT = background[5] + background[6];

                            var percievedBright = perceivedBrightness(parseInt(rT, 16), parseInt(gT, 16), parseInt(bT, 16));

                            if (percievedBright <= 130) {
                                return "white";
                            }
                        }

                        return '#37485B';
                    };

                    $scope.cellStyle = function() {
                        var style = {};

                        if ($scope.cell.background) {
                            style.background = $scope.cell.background;
                            style.color = getForegroundForBackground($scope.cell.background);
                        }
                        if ($scope.cell.border) {
                            style['border-color'] = $scope.cell.border;
                        }

                        if ($scope.cell.fontStyle) {
                            style['font-style'] = $scope.cell.fontStyle;
                        }

                        if ($scope.cell.styleLoad) {
                            if ($scope.cell.styleLoad.border) {
                                if ($scope.cell.styleLoad.border.left) {
                                    style['border-left'] = "2px solid red";
                                }

                                if ($scope.cell.styleLoad.border.bottom) {
                                    style['border-bottom'] = "2px solid red";
                                }

                                if ($scope.cell.styleLoad.border.top) {
                                    style['border-top'] = "2px solid red";
                                }

                                if ($scope.cell.styleLoad.border.right) {
                                    style['border-right'] = "2px solid red";
                                }
                            }
                            if ($scope.cell.styleLoad.background) {
                                style['background'] = "#E77471";
                                style['color'] = "white";
                            }
                        } else {
                            if (!style.background) {
                                style['background-color'] = null;
                                style['color'] = null;
                            }
                            style['border-right'] = null;
                            style['border-left'] = null;
                            style['border-bottom'] = null;
                            style['border-top'] = null;
                        }

                        style.color = getForegroundForBackground(style.background);
                        
                        return style;
                    }
                }
            }
        }
    ]);
});