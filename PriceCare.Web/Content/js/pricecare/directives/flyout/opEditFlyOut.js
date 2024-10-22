define(['pricecare/directives/flyout/module'], function(module) {
    'use strict';
    module.directive('opEditFlyOut', [
        '$document', 'helperService', '$q',
        function($document, helperService, $q) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/flyout/opEditFlyOutTemplate.html",
                scope: {
                    cell: "=",
                    row: "=",
                    table: "=",
                    options: '=',
                    close: "&"
                },
                controller: function($scope, $element, $attrs) {

                    $scope.defaultRequiredMessage = "Required";
                    $scope.defaultInvalidMessage = "Invalid";

                    $scope.errorRequired = false;
                    $scope.errorInvalid = false;

                    var init = function() {
                        for (var i = 0; i < $scope.options.fields.length; i++) {
                            var field = $scope.options.fields[i];

                            field.value = field.cellValue;
                            if (!field.properties) {
                                field.properties = {};
                            }
                        }
                    };

                    init();

                    $element.bind("keydown keypress", function(event) {
                        if (event.keyCode == 27) {
                            $scope.$apply(function() {
                                $scope.close();
                            });
                        } else if (event.keyCode == 13) {
                            $scope.$apply(function() {
                                $scope.onApply();
                            });
                        }
                        event.stopPropagation();
                    });
                    $element.bind("click", function(event) {
                        event.stopPropagation();
                    });
                    $element.bind("contextmenu", function(event) {
                        event.stopPropagation();
                    });

                    $scope.onReverseEdit = function() {
                        $scope.cell.edited = false;
                        if ($scope.options.properties.reverseEdit) {
                            $scope.options.properties.reverseEdit($scope.options.fields, $scope.cell, $scope.row, $scope.table);
                            $scope.close();
                        }
                    };

                    $scope.onApply = function() {
                        var hasErrors = false;
                        var backendCallsPromises = [];
                        for (var i = 0; i < $scope.options.fields.length; i++) {
                            var field = $scope.options.fields[i];

                            field.properties.errorRequired = false;
                            field.properties.errorInvalid = false;
                            field.properties.errorUnique = false;

                            if (field.properties.parse) {
                                field.value = field.properties.parse(field.value);
                            }
                            if (field.type == helperService.FieldTypes.Select) {
                                var valueId = _.find(field.properties.items, function (item) { return item.selected; }).id;
                                field.value = valueId;
                            }

                            if (field.type == helperService.FieldTypes.Select) {
                                var valueId = _.find(field.properties.items, function (item) { return item.selected; }).id;
                                field.value = valueId;
                            }

                            var currentValue = field.value;

                            if (field.properties.required) {
                                if (currentValue === undefined || currentValue == null || currentValue === ''
                                        || (field.type == helperService.FieldTypes.NumericTextbox && isNaN(currentValue) )) {
                                    field.properties.errorRequired = true;
                                    hasErrors = true;
                                    continue;
                                }
                            }

                            if (field.properties.isValid) {
                                if (!field.properties.isValid(field, $scope.cell, $scope.row, $scope.table)) {
                                    field.properties.errorInvalid = true;
                                    hasErrors = true;
                                    continue;
                                }
                            }

                            // Test which needs to call the backend
                            if (field.properties.isValidAsync) {
                                var promise = field.properties.isValidAsync(field, $scope.cell, $scope.row, $scope.table);
                                backendCallsPromises.push(promise);
                            }
                        }

                        if (hasErrors) {
                            return;
                        }

                        if (backendCallsPromises.length > 0) {
                            $q.all(backendCallsPromises).then(function(data) {

                                var allOk = _.all(data, function(d) {
                                    return d == true || d == "true";
                                });

                                if (allOk) {
                                    $scope.launchEdit();
                                } else {
                                    return;
                                }
                            });
                        } else {
                            $scope.launchEdit();
                        }

                    };

                    var elementRect = $element[0].getBoundingClientRect();
                    var bodyRect = document.body.getBoundingClientRect();

                    var offset = bodyRect.width - elementRect.left - elementRect.width;
                    var arrowOffset;
                    if (offset < 0) {
                        offset = (offset - 20);
                        arrowOffset = -offset;

                        arrowOffset = arrowOffset + (elementRect.width / 10);

                        offset = offset + 'px';
                        arrowOffset = arrowOffset + 'px';
                    } else {
                        offset = '0px';
                        arrowOffset = '10%';
                    }

                    $scope.getElementStyle = function() {
                        return { left: offset };
                    }

                    $scope.getArrowStyle = function() {
                        return { left: arrowOffset };
                    }

                    $scope.launchEdit = function () {
                        var noneChanged = _.all($scope.options.fields, function (field) {
                            if (field.properties.areEquals) {
                                return field.properties.areEquals(field.value, field.cellValue);
                            }
                            return field.value == field.cellValue;
                        });

                        if (noneChanged) {
                            $scope.close();
                        } else {
                            $scope.cell.edited = true;
                            if ($scope.options.properties.apply) {
                                $scope.options.properties.apply($scope.options.fields, $scope.cell, $scope.row, $scope.table);
                            }

                            $scope.close();
                        }
                    };
                }
            }
        }
    ]);
});