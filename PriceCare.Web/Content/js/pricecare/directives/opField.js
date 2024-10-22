define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opField', [
        'helperService',
        function(helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opFieldTemplate.html",
                scope: {
                    options: "=",
                    valueChanged: '&'
                },
                controller: function($scope, $element, $attrs) {
                    $scope.helperService = helperService;

                    var bubbleChangeEvent = function() {
                        if ($scope.valueChanged) {
                            $scope.valueChanged();
                        }
                    };
                    
                    //$scope.showField = $scope.options && ($scope.options.show == undefined || $scope.options.show);

                    // SELECT
                    $scope.selectChanged = function(item) {
                        bubbleChangeEvent();
                    };

                    // TEXTBOX
                    $scope.onEnter = function() { 
                        if (($scope.options.properties.trigger & helperService.FieldTriggers.OnEnter) == helperService.FieldTriggers.OnEnter) {
                            bubbleChangeEvent();
                        }
                    };

                    $scope.onBlur = function() {
                        if (($scope.options.properties.trigger & helperService.FieldTriggers.OnBlur) == helperService.FieldTriggers.OnBlur) {
                            bubbleChangeEvent();
                        }
                    };

                    $scope.onTextChanged = function() {
                        if (($scope.options.properties.trigger & helperService.FieldTriggers.OnChange) == helperService.FieldTriggers.OnChange) {
                            bubbleChangeEvent();
                        }
                    };

                    // CHECKBOX
                    $scope.onCheckedChanged = function() {
                        bubbleChangeEvent();
                    };

                    // DYNAMIC POP
                    $scope.onDynamicChanged = function() {
                        bubbleChangeEvent();
                    }
                }
            }
        }
    ]);
});