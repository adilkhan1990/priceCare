define(['pricecare/design/module'], function(module) {
    'use strict';
    module.controller('Design4Controller', [
        '$scope', '$rootScope', '$controller', 'helperService', function ($scope, $rootScope, $controller, helperService) {

            var ruleTypes = [
                {id: 0, text: "<"},
                {id: 2, text: "=<"},
                {id: 3, text: "="},
                {id: 4, text: ">"},
                {id: 5, text: ">="}
            ];

            $scope.rulesData = [
                {
                    type: 0,
                    value: 5
                },
                {
                    type:3,
                    value: 5
                },
                {
                    type: 5,
                    value: 5
                }
            ];

            var initRules = function () {
                var rules = [];

                $scope.rulesData.forEach(function(rule) {
                    rules.push({
                        selectOptions: {
                            type: helperService.FieldTypes.Select,
                            //text: 'Compare to',
                            //name: 'compareTo',
                            properties:
                            {
                                class: 't-custom-select-boxed',
                                items: getRules(rule.type)
                            }
                        },
                            valueOptions: {
                                type: helperService.FieldTypes.NumericTextbox,
                                //text: 'Highlight when more than',
                                value: rule.value,
                                //name: 'highlightWhenMore',
                                properties: {
                                    //trigger: helperService.FieldTriggers.OnEnter | helperService.FieldTriggers.OnBlur,
                                    min: 0,
                                    max: 200,
                                    unit: '%'
                                }
                            }
                        }
                    );
                });

                $scope.rules = rules;
            };


            var getRules = function (typeId) {
                var result = [];

                ruleTypes.forEach(function (ruleType) {
                    var newRuleType = { id: ruleType.id, text: ruleType.text };
                    
                    if (ruleType.id == typeId) {
                        newRuleType.selected = true;
                    }
                    result.push(newRuleType);
                });

                return result;
            }

            initRules();
        }
    ]);
});