define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opSelectionTree', [
        function () {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opSelectionTreeTemplate.html",
                scope: {
                    options: "="
                },
                controller: function ($scope, $element, $attrs) {
                    var anyChildrenChecked = function(items) {
                        if (items) {
                            var response = { total: items.length, checked: 0 };
                            for (var i = 0; i < items.length; i++) {
                                var currentItem = items[i];
                                if (currentItem.value) {
                                    response.checked++;
                                }
                                
                                var childrenResponse = anyChildrenChecked(currentItem.items);
                                response.total = response.total + childrenResponse.total;
                                response.checked = response.checked + childrenResponse.checked;
                            }

                            return response;
                        }
                        return {total: 0, checked: 0};
                    }

                    var setChildren = function(items, value) {
                        if (items) {
                            for (var i = 0; i < items.length; i++) {
                                var currentItem = items[i];
                                currentItem.indeterminate = false;
                                currentItem.value = value;
                                setChildren(currentItem.items, value);
                            }
                        }
                    }

                    var traverseTree = function(items, targetItem) {
                        if (items) {
                            for (var i = 0; i < items.length; i++) {
                                var currentItem = items[i];

                                if (currentItem == targetItem) {
                                    currentItem.indeterminate = false;
                                    setChildren(currentItem.items, targetItem.value);
                                    return true;
                                } else {
                                    var isChildren = traverseTree(currentItem.items, targetItem);
                                    if (isChildren) {
                                        var response = anyChildrenChecked(currentItem.items);
                                        if (response.checked == 0) {
                                            currentItem.indeterminate = false;
                                            currentItem.value = false;
                                        } else if (response.checked < response.total) {
                                            currentItem.value = false;
                                            currentItem.indeterminate = true;
                                        } else if (response.checked == response.total) {
                                            currentItem.indeterminate = false;
                                            currentItem.value = true;
                                        }
                                        return true;
                                    }
                                }
                            }
                        }
                        return false;
                    }

                    $scope.onValueChanged = function (item) {
                        traverseTree($scope.options.properties.items, item);
                    };
                }
            }
        }
    ]);
});