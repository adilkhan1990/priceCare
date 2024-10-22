define(['pricecare/directives/module'], function (module) {
    'use strict';
    module.directive('opSmartResolution', [
        '$document', 'helperService',
        function ($document, helperService) {
            return {
                restrict: 'A',
                replace: true,
                templateUrl: "Content/js/pricecare/directives/opSmartResolutionTemplate.html",
                scope: {
                    options: "="
                },
                controller: function ($scope, $element, $attrs) {
                    $scope.showFlyout = false;

                    var generateLinkText = function () {
                        if ($scope.options.rulesData && $scope.options.rulesData.length > 0) {
                            var texts = [];
                            $scope.options.rulesData.forEach(function(rule) {
                                var type = _.find(helperService.RuleTypes, function(ruleType) {
                                    return ruleType.id == rule.type;
                                });

                                texts.push(type.text + " " + rule.value + "%");
                            });
                            $scope.linkText = texts.join(" or ");
                        } else {
                            $scope.linkText = 'no rules...';
                        }
                        if ($scope.options.callback) {
                            $scope.options.callback();
                        }
                    };

                    $scope.apply = function() {
                        if ($scope.options.apply) {
                            $scope.options.apply();
                        }
                    }

                    $scope.reject = function () {
                        if ($scope.options.reject) {
                            $scope.options.reject();
                        }
                    }

                    $scope.flyoutOptions = {
                        rulesData: $scope.options.rulesData
                    };
                    generateLinkText();
                    $scope.toggleFlyout = function(evt) {
                        $scope.showFlyout = !$scope.showFlyout;

                        if ($scope.showFlyout) {
                            if (!linkTagged) {
                                $(evt.target).data('opLinkSmart', $scope.id);
                            }
                            var elementRect = evt.target.getBoundingClientRect();
                            $scope.flyoutOptions.parentOffset = elementRect.left;
                        }

                        toggleDocumentClickEvent();
                    }

                    $scope.closeFlyouts = function() {
                        $scope.showFlyout = false;
                        toggleDocumentClickEvent();
                    }

                    $scope.id = helperService.makeid();
                    var linkTagged = false;

                    var onDocumentClick = function (e) {
                        // The click event on body is coming from outside the angular scope so we need to get back into the scope with $scope.$apply()
                        $scope.$apply(function () {
                            if ($scope.showFlyout) {
                                var opCellId = angular.element(e.target).inheritedData('opLinkSmart');
                                if (opCellId == $scope.id) {

                                } else {
                                    $scope.showFlyout = false;
                                    toggleDocumentClickEvent();
                                }
                            }
                        });
                    }

                    var toggleDocumentClickEvent = function () {
                        if ($scope.showFlyout) {
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

                    $scope.saveRules = function () {
                        $scope.flyoutOptions.result.forEach(function(rule) {
                            if (!rule.value) {
                                rule.value = 0;
                            }
                        });

                        $scope.options.rulesData = $scope.flyoutOptions.result;
                        $scope.flyoutOptions.rulesData = $scope.options.rulesData;
                        generateLinkText();
                        $scope.closeFlyouts();
                    };
                }
            }
        }
    ]);
});