define(['pricecare/directives/flyout/module'], function(module) {
    'use strict';
    module.directive('opSmartResolutionFlyOut', [
        '$document', 'helperService', '$q',
        function($document, helperService, $q) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/flyout/opSmartResolutionFlyoutTemplate.html",
                scope: {
                    options: '=',
                    close: "&",
                    save: '&'
                },
                controller: function($scope, $element, $attrs) {
                    $scope.rules = [];

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

                    var generateRule = function(ruleData) {
                        return {
                            selectOptions: {
                                type: helperService.FieldTypes.Select,
                                properties:
                                {
                                    class: 't-custom-select-boxed',
                                    items: getRules(ruleData.type)
                                }
                            },
                            valueOptions: {
                                type: helperService.FieldTypes.NumericTextbox,
                                value: ruleData.value,
                                properties: {
                                    min: 0,
                                    unit: '%'
                                }
                            }
                        };
                    };
                    var initRules = function () {
                        var rules = [];

                        $scope.options.rulesData.forEach(function (rule) {
                            var newRule = generateRule(rule);
                            rules.push(newRule);
                        });

                        $scope.rules = rules;
                    };

                    var getRules = function (typeId) {
                        var result = [];

                        helperService.RuleTypes.forEach(function (ruleType) {
                            var newRuleType = { id: ruleType.id, text: ruleType.text };

                            if (ruleType.id == typeId) {
                                newRuleType.selected = true;
                            }
                            result.push(newRuleType);
                        });

                        return result;
                    }
                  
                    var elementRect = $element[0].getBoundingClientRect();

                    var offset = $scope.options.parentOffset - elementRect.left;

                    $scope.getElementStyle = function() {
                        return { left: offset };
                    }

                    $scope.getArrowStyle = function() {
                        return {
                            left: '1em'
                    };
                    };

                    $scope.deleteRule = function(rule) {
                        var index = $scope.rules.indexOf(rule);
                        $scope.rules.splice(index, 1);
                    };

                    $scope.addRule = function() {
                        $scope.rules.push(generateRule({ type: 0, value: 0 }));
                    };

                    initRules();

                    $scope.onApply = function () {
                        $scope.options.result = [];
                        $scope.rules.forEach(function (rule) {
                            $scope.options.result.push({ type: _.find(rule.selectOptions.properties.items, function (r) { return r.selected; }).id, value: rule.valueOptions.value });
                        });
                        $scope.save();
                    };
                }
            }
        }
    ]);
});