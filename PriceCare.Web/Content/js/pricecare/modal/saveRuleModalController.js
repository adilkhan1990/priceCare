define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('SaveRuleModalController', ['$scope', '$modalInstance', 'infos', 'helperService', function ($scope, $modalInstance, infos, helperService) {

        $scope.year = parseInt(infos.year);
        $scope.month = parseInt(infos.month);

        $scope.yearOptions = {
            type: helperService.FieldTypes.Select,
            name: 'mathOptions',
            properties: {
                class: 't-custom-select-boxed',
                items: []
            }
        };

        $scope.monthOptions = {
            type: helperService.FieldTypes.Select,
            name: 'mathOptions',
            properties: {
                class: 't-custom-select-boxed',
                items: [
                    {id: 1, text: "January"},
                    {id: 2, text: "February"},
                    {id: 3, text: "March"},
                    {id: 4, text: "April"},
                    {id: 5, text: "May"},
                    {id: 6, text: "June"},
                    {id: 7, text: "July"},
                    {id: 8, text: "August"},
                    {id: 9, text: "September"},
                    {id: 10, text: "October"},
                    {id: 11, text: "November"},
                    {id: 12, text: "December"}
                ]
            }
        };
        $scope.monthOptions.properties.items.forEach(function(item) {
            if (item.id == $scope.month) {
                item.selected = true;
            }
        });

        for (var i = 0; i < 10; i++) {
            $scope.yearOptions.properties.items.push({ id: $scope.year + i, text: $scope.year + i, selected: i == 0 });
        }

        $scope.onYearChanged = function() {
            var selectedMonth = getSelectedMonth();
            var selectedYear = getSelectedYear();

            if (selectedMonth.id < $scope.month && selectedYear.id == $scope.year) {
                $scope.monthOptions.properties.items.forEach(function (item) {
                    if (item.id == $scope.month) {
                        item.selected = true;
                    } else {
                        item.selected = false;
                    }
                });
            }
        };

        $scope.onMonthChanged = function() {
            var selectedMonth = getSelectedMonth();
            var selectedYear = getSelectedYear();

            if (selectedMonth.id < $scope.month && selectedYear.id == $scope.year) {
                $scope.yearOptions.properties.items.forEach(function (item) {
                    if (item.id == $scope.year + 1) {
                        item.selected = true;
                    } else {
                        item.selected = false;
                    }
                });
            }
        };

        var getSelectedMonth = function() {
            return _.find($scope.monthOptions.properties.items, function(month) { return month.selected; });
        };

        var getSelectedYear = function() {
            return _.find($scope.yearOptions.properties.items, function(month) { return month.selected; });
        };

        $scope.ok = function () {
            var month = getSelectedMonth().id;
            month = ("0" + month).slice(-2);
            $modalInstance.close(getSelectedYear().id + '-' + month + '-01 00:00:00Z');
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }]);
});