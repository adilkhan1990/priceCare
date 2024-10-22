define(['pricecare/modal/module'], function (module) {
    'use strict';
    module.controller('AddSynonymModalController', ['$scope', '$modalInstance', 'dimensionDictionaryService', 'infos', 'productService' , 'countryService', 'helperService',
        function ($scope, $modalInstance, dimensionDictionaryService, infos, productService, countryService, helperService) {

            var list = infos.dimensionTypeChosen;

            $scope.dimensionTypeOptions = {
                type: helperService.FieldTypes.Select,
                name: 'dimensionType',
                properties: {
                    class: 't-custom-select-boxed',
                    items: dimensionDictionaryService.prepareDimensionTypeForFilter(infos.dimensionTypeChosen)
                }
            };

            $scope.dimensionTargetOptions = {
                type: helperService.FieldTypes.Select,
                name: 'countries',
                properties:
                {
                    class: 't-custom-select-boxed',
                    items: [],
                }
            };

            // Model
            $scope.synonym = {};

            // Validation
            $scope.isRequiredName = true;

            $scope.ok = function () {
                $scope.errorMessage = "";
                $scope.isRequiredName = $scope.synonym.name && $scope.synonym.name != "";
                var dimensionType = getSelectedDimensionTypeId();
                var dimensionTarget = getSelectedDimensionTargetId();
                if ($scope.isRequiredName && dimensionType && dimensionTarget) {
                    $scope.synonym.dimension = dimensionType;
                    $scope.synonym.dimensionId = dimensionTarget;
                    dimensionDictionaryService.create($scope.synonym).then(function (success) {
                        if(success == "true")
                            $modalInstance.close($scope.synonym);
                        else {
                            $scope.errorMessage = "This synonym already exists. Please choose another one.";
                        }
                    });                    
                }
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

            var getSelectedDimensionTypeId = function () {
                var dimensionType = _.find($scope.dimensionTypeOptions.properties.items, function (item) { return item.selected; });
                return dimensionType.id;
            };

            var getSelectedDimensionTargetId = function(){
                var dimensionTarget = _.find($scope.dimensionTargetOptions.properties.items, function (item) { return item.selected; });
                return dimensionTarget.id;
            };

            var prepareDimensionForFilter = function(data) {
                var result = [];

                data.forEach(function(item, i) {
                    result.push({
                        id: item.id,
                        text: item.name,
                        name: item.name,
                        selected : i == 0
                    });
                });

                return result;
            };

            $scope.dimensionTypeChanged = function() {
                var dimensionSelected = getSelectedDimensionTypeId();
                var promise = (dimensionSelected == 'Product') ? productService.getAllProducts() : countryService.getAllCountries();
                promise.then(function (data) {
                    $scope.dimensionTargetOptions.properties.items = prepareDimensionForFilter(data);
                });
            };

            $scope.dimensionTypeChanged();

        }]);
});