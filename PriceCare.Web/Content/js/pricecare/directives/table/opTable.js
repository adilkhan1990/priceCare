define(['pricecare/directives/table/module'], function (module) {
    'use strict';
    module.directive('opTable', [
        '$document', 'helperService',
        function ($document, helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/table/opTableTemplate.html",
                scope: {
                    table: "="
                },
                controller: function ($scope, $element, $attrs) {

                    var updateHeader = function () {
                        if ($scope.table && $scope.table.columnFilteringOptions) {
                            var headerCells = $scope.table.rows[0].cells;
                            var first = headerCells[$scope.table.columnFilteringOptions.fixed];
                            var last = headerCells[headerCells.length - 1];

                            var firstVisible = headerCells[$scope.table.columnFilteringOptions.fixed + $scope.table.columnFilteringOptions.offset];
                            var lastVisible = headerCells[$scope.table.columnFilteringOptions.fixed + $scope.table.columnFilteringOptions.offset + $scope.table.columnFilteringOptions.count - 1];

                            $scope.paginationHeader = firstVisible.value + " > " + lastVisible.value;
                            if (first != firstVisible) {
                                $scope.paginationHeaderPre = first.value + " ... ";
                            } else {
                                $scope.paginationHeaderPre = "";
                            }
                            if (last != lastVisible) {
                                $scope.paginationHeaderPost = " ... " + last.value;
                            } else {
                                $scope.paginationHeaderPost = "";
                            }
                        }
                    }

                    $scope.goLeft = function() {
                        if ($scope.table.columnFilteringOptions) {
                            if ($scope.table.columnFilteringOptions.offset > 0) {
                                $scope.table.columnFilteringOptions.offset--;

                                updateHeader();
                            }
                        }
                    };

                    $scope.goRight = function () {
                        if ($scope.table.columnFilteringOptions) {
                            var totalItems = $scope.table.rows[0].cells.length;
                            var maxOffset = totalItems - $scope.table.columnFilteringOptions.fixed - $scope.table.columnFilteringOptions.count;

                            if ($scope.table.columnFilteringOptions.offset < maxOffset) {
                                $scope.table.columnFilteringOptions.offset++;

                                updateHeader();
                            }
                        }
                    };

                    $scope.$watch('table', function () {
                        updateHeader();
                    });
                    updateHeader();

                    angular.element($document[0].body).on('keydown keypress', function(evt) {
                        $scope.$apply(function() {
                            if (evt.keyCode == 37) {
                                $scope.goLeft();

                            } else if (evt.keyCode == 39) {
                                $scope.goRight();
                            }
                        });
                    });
                }
            }
        }
    ]);
});