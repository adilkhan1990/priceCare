define(['pricecare/rules/module'], function(module) {
    'use strict';
    module.controller('RulesController', [
        '$scope', '$rootScope', '$controller', '$http', 'helperService', 'userService', 'rulesService', 'countryService', 'priceTypeService', 'productService', 'versionService', 'simulationService', '$q', '$modal', 'forecast', 'validate', '$routeParams', 'loadService','$location',
        function ($scope, $rootScope, $controller, $http, helperService, userService, rulesService, countryService, priceTypeService, productService, versionService, simulationService, $q, $modal, forecast, validate, $routeParams, loadService, $location) {

            $scope.loadId = $routeParams.loadId;

            $scope.simulationId = null;
            $scope.isCurrentUser = false;
            $scope.forAllProducts = false;
            
            $scope.forecast = forecast;
            $scope.validate = validate;
            var usableProductsAndGeographyIds;

            $scope.showAddSubRule = false;
            $scope.priceTypeOptions = [];

            $scope.currentlyShownBasket = [];

            $scope.canSave = false;
            $scope.mathOptions = {
                type: helperService.FieldTypes.Select,
                name: 'mathOptions',
                properties: {
                    class: 't-custom-select-boxed'
                }
            };

            $scope.allPriceTypeOptions = {
                type: helperService.FieldTypes.Select,
                name: 'priceTypes',
                properties: {
                    class: 't-custom-select-boxed'
                }
            };

            $scope.countryOptions = {
                type: helperService.FieldTypes.SelectMultiLevel,
                name: 'countries',
                properties: {
                    class: 't-custom-select-boxed'
                }
            };

            var regionCountryOptions = {
                type: helperService.FieldTypes.SelectMultiLevel,
                name: 'countryRegion',
                properties: {
                    class: 't-custom-select-text',
                }
            };

            var versionOptions = {                
                type: helperService.FieldTypes.Select,
                name: 'versions',
                hide:true,
                properties: {
                    class: 't-custom-select-text'                                
                }                
            };

            $scope.firstVersionSelected = function () {
                if (forecast)
                    return true;
                if (versionOptions.properties.items && versionOptions.properties.items.length > 0) {
                    return versionOptions.properties.items[0].selected == true;
                }

                return false;
            }

            var simulationOptions = {
                type: helperService.FieldTypes.SelectMultiLevel,
                name: 'simulations',
                hide: true,
                properties: {
                    class: 't-custom-select-text',
                    items: []
                }
            };

            var applicableFromFilter = {                
                type: helperService.FieldTypes.Select,
                properties: { class: 't-custom-select-text' },
                name: 'applicableFromFilter'                
            };

            var ruleTypeOptions = {
                type: helperService.FieldTypes.QuickSwitch,
                name: 'ruleType',
                properties: {
                    class: 't-custom-select-boxed',
                    items: [
                        {
                            id: 30,
                            text: 'Default',
                            selected: true
                        },
                        {
                            id: 32,
                            text: 'Launch'
                        }
                    ]
                }
            };

            $scope.filterOptions = {
                header: {
                    items: [
                        {
                            type: helperService.FieldTypes.Label,
                            properties: {
                                text: "Rules for "
                            }
                        },
                        regionCountryOptions,
                        {
                            type: helperService.FieldTypes.Label,
                            properties: {
                                text: "from "
                            }
                        },
                        applicableFromFilter,
                        versionOptions,
                        simulationOptions
                    ]
                },
                filters: [
                    ruleTypeOptions
                ],
                onChanged: function (sourceChanged) {
                    if (sourceChanged == regionCountryOptions || sourceChanged == ruleTypeOptions) {
                        getVersions();
                    } else if (sourceChanged == versionOptions) {
                        getApplicableFroms();
                    } else if (sourceChanged == simulationOptions) {
                        if (getSelectedSimulationOptionItem().isLaunch)
                            changeSelectedProductId(15);
                        loadCacheAndReloadData();
                    } else {
                        getData();
                    }
                }
            }

            var getSelectedProductId = function () {
                var productId = 0;
                $scope.products.forEach(function(product) {
                    if (product.selected)
                        productId = product.id;
                });
                return productId;
            };

            var changeSelectedProductId = function (productId) {
                $scope.products.forEach(function (product) {
                    if (product.id == productId)
                        product.selected = true;
                    else
                        product.selected = false;
                });
            };

            var saveRule = function (callback) {

                if (validate) {

                    var now = new Date();
                    var nowYear = now.getUTCFullYear();
                    var nowMonth = now.getUTCMonth() + 1;
                    if (now.getUTCMonth() == 11) {
                        nowYear = nowYear + 1;
                        nowMonth = 0;
                    }
                        var appFrom = nowYear + '-' + nowMonth + '-01 00:00:00Z';
                        var data = {
                            ruleDefinition: $scope.definition,
                            applicableFrom: appFrom,
                            geographyId: getSelectedCountryId(),
                            productId: getSelectedProductId(),
                            gprmRuleTypeId: getSelectedRuleTypeId()
                        };

                        rulesService.saveRule(data).then(function (result) {
                            if (result) {
                                validateLoadItemDetail();
                            }
                        });
                   

                } else {
                    if ($scope.canSave) {
                        if ($scope.forecast) {
                            var modalInstance = $modal.open({
                                templateUrl: 'Content/js/pricecare/modal/saveSimulationModal.html',
                                controller: 'SaveSimulationModalController',
                                backdrop: 'static',
                                resolve: {
                                    infos: function() {
                                        return {
                                            simulationOption: getSelectedSimulationOption() != null ? getSelectedSimulationOption().id : 'Budget',
                                            simulationName: getSelectedSimulationOptionItem() != null ? getSelectedSimulationOptionItem().name : '',
                                            isCurrentUser: $scope.isCurrentUser,
                                        };
                                    }
                                }
                            });
                            modalInstance.result.then(function(save) {
                                var data = { save: save, simulationId: $scope.simulationId }
                                simulationService.saveSimulation(data).then(function(result) {
                                    $scope.canSave = false;
                                    getSimulations();


                                });
                            });
                        } else {
                            var appFrom = getApplicableFrom().substring(0, 4) + '-' + getApplicableFrom().substring(5, 7) + '-01 00:00:00Z';
                                var data = {
                                    ruleDefinition: $scope.definition,
                                    applicableFrom: appFrom,
                                    geographyId: getSelectedCountryId(),
                                    productId: getSelectedProductId(),
                                    gprmRuleTypeId: getSelectedRuleTypeId()
                                };

                                rulesService.saveRule(data).then(function(result) {
                                    if (result) {
                                        if (callback) {
                                            callback();
                                        } else {
                                            getVersions();
                                        }
                                    }
                                });
                        }

                    } 
                }
            };
            var validateLoadItemDetail = function () {
                loadService.validateLoadItemDetail($routeParams.loadId, helperService.LoadItemNames.Rule, getSelectedProductId(), [getSelectedCountryId()]).then(function (result) {
                    if (result.url)
                        $location.path(result.url);
                    else
                        getBaseScreenInfo();
                });
            }

            $scope.saveRule = function () {
                saveRule();                
            };

            var setStyleForItems = function(items, selectedId) {
                items.forEach(function(item) {                    
                    item.selected = item.id == selectedId;                    
                });
            };
            var getNewCountry = function () {
                var newCountry = {};
                $scope.countryOptions.properties.items.forEach(function (region) {
                    if (region.selected) {
                        region.items.forEach(function (country) {
                            if (country.selected)
                                newCountry = country;
                        });
                    }
                });
                return newCountry;

            };
            var getNewPriceType = function() {
                var priceType = {};
                $scope.allPriceTypeOptions.properties.items.forEach(function (item) {
                    if (item.selected)
                        priceType = item;
                });
                return priceType;
            };
            var getSubRuleIndex = function(gprmMathId) {
                var n = 1;
                $scope.definition.referencedData.forEach(function(data) {
                    if (data.gprmMathId == gprmMathId)
                        n++;
                });
                return n;
            };

            $scope.selectProduct = function (productId) {

                if (isEdited() && !$scope.forecast) {
                    var modalInstance = $modal.open({
                        templateUrl: 'Content/js/pricecare/modal/yesNoCancelModal.html',
                        controller: 'YesNoCancelModalController',
                        backdrop: 'static',
                        resolve: {
                            infos: function () {
                                return {
                                    title: 'Warning',
                                    content: "You have unsaved modifications. If you change product, you will loose them. Do you want to save?"
                                }
                            }
                        }
                    });
                    modalInstance.result.then(function (save) {
                        if (save) {
                            saveRule(function () { selectProductInternal(productId); });
                        } else {
                            selectProductInternal(productId);
                        }
                    });
                } else {
                    selectProductInternal(productId);
                }
                $scope.forAllProducts = productId == 0;
            }

            var selectProductInternal = function (productId) {
                setStyleForItems($scope.products, productId);
                $scope.selectedProductId = productId;
                if (forecast) {
                    getCacheSimulation();
                }
                else {
                    getVersions();
                }
                
            };
            var isEdited = function() {
                return $scope.definition.edited ||
                    _.any($scope.definition.defaultBasket, function(item) { return item.edited; }) ||
                    _.any($scope.definition.referencedData, function(item) { return item.edited; });
            };

            $scope.addSubRule = function() {
                $scope.showAddSubRule = true;
            };
            $scope.addReferencedCountry = function () {
                var ids = _.pluck($scope.currentlyShownBasket, 'referencedGeographyId');
                ids.push(getSelectedCountryId());

                var result = [];

                regionCountryOptions.properties.items.forEach(function(region) {

                    var newRegion = {
                        id: region.id,
                        selected: region.selected,
                        sname: region.sname,
                        text: region.text,
                        items:[]
                    };

                    region.items.forEach(function(country) {
                        if (!_.contains(ids, country.id)) {
                            newRegion.items.push({
                                id: country.id, 
                                name: country.name,
                                selected: country.selected,
                                text: country.text
                            });
                        }
                    });

                    if (newRegion.items.length > 0) {
                        result.push(newRegion);
                    }

                });

                var selectedRegion;

                result.forEach(function(region) {
                    if (region.selected)
                        selectedRegion = region;
                });

                if (!selectedRegion) {
                    selectedRegion = result[0];
                    selectedRegion.selected = true;
                }
                var selectedCountry;
                selectedRegion.items.forEach(function(country) {
                    if (country.selected)
                        selectedCountry = country;
                });
                if (!selectedCountry) {
                    selectedCountry = selectedRegion.items[0];
                    selectedCountry.selected = true;
                }

                $scope.countryOptions.properties.items = result;



                $scope.showAddReferencedCountry = true;
            };
            $scope.confirmNewSubRule = function () {                
                var math = getSelectedMathOption();
                var index = getSubRuleIndex(math.id);

                var newSubRule = {
                    subRuleIndex: index,
                    gprmMathId: math.id,
                    argument: index,
                    weightTypeId: 48,
                    default: false,
                    gprmMath: math.text,
                    edited:true,
                    active:true,
                    basket:[]
                };
                $scope.canSave = true;
                var defaultRule = _.find($scope.definition.referencedData, function(rd) { return rd.default; });
                if (defaultRule) {
                    defaultRule.basket.forEach(function(basketItem) {
                        newSubRule.basket.push(helperService.clone(basketItem));
                    });
                }

                $scope.definition.referencedData.push(newSubRule);
                $scope.showAddSubRule = false;

                $scope.canRemoveRules = _.filter($scope.definition.referencedData, function (ru) { return ru.active; }).length > 1;
            };
            $scope.cancelNewSubRule = function () {
                $scope.showAddSubRule = false;
            };
            $scope.confirmReferencedCountry = function () {
                var country = getNewCountry();
                var priceType = getNewPriceType();

                rulesService.getRulePriceTypes({
                    productId: getSelectedProductId(),
                    countryId: country.id,
                    applicableFrom: getApplicableFrom()
                }).then(function(result) {
                    $scope.currentlyShownBasket.push({
                        referencedGeography: country.text,
                        referencedPriceType: priceType.text,
                        referencedGeographyId: country.id,
                        referencedPriceTypeId: priceType.id,
                        referencedPriceAdjustment: 0,
                        active: true,
                        edited:true
                    });
                    $scope.currentlyShownPriceTypeOptions.push({
                        type: helperService.FieldTypes.Select,
                        name: 'defaultPriceType' + $scope.currentlyShownPriceTypeOptions.length,
                        properties: {
                            class: 't-custom-select-boxed',
                            items: getPriceTypesForSelect(result, priceType.id)
                        }
                    });
                    $scope.showAddReferencedCountry = false;
                    $scope.canSave = true;
                });
            };
            $scope.cancelReferencedCountry = function() {
                $scope.showAddReferencedCountry = false;
            };
            $scope.removeCountryFromBasket = function(index, item) {
                item.edited = true;
                item.active = false;
                $scope.canSave = true;
            };           
            $scope.removeSubRule = function (subRule, event) {
                subRule.edited = true;
                subRule.active = false;
                $scope.canRemoveRules = _.filter($scope.definition.referencedData, function (ru) { return ru.active; }).length > 1;
                event.stopPropagation();
                $scope.canSave = true;
            };
            $scope.selectPriceType = function (priceTypeId) {
                $scope.canSave = true;
                $scope.definition.edited = true;
                $scope.definition.selectedReviewedPriceTypeId = priceTypeId;
                $scope.definition.reviewedPriceTypeOptions.forEach(function(priceType) {                    
                    priceType.selected = priceType.id == priceTypeId;                    
                });                
            };
            $scope.selectRuleMath = function(ruleMathId) {
                $scope.definition.gprmMathId = ruleMathId;
                $scope.ruleMath.forEach(function(math) {
                    math.selected = math.id == ruleMathId;
                });

                $scope.definition.edited = true;
                $scope.canSave = true;
            };
            $scope.onChangeParameter = function() {
                $scope.definition.edited = true;
                $scope.canSave = true;
            }

            $scope.selectSubRule = function (subRuleIndex, subrule) {
                $scope.selectedSubRuleIndex = subRuleIndex;
                $scope.definition.referencedData.forEach(function(subRule, index) {
                    subRule.selected = subRuleIndex == index;                    
                });

                $scope.definition.edited = true;
                $scope.canSave = true;

                $scope.selectedSubRuleText = subrule.gprmMath + (subrule.argument > 0 ? (' ' + subrule.argument) : '');

                var promiseArray = [];

                subrule.basket.forEach(function (item) {
                    var promise = rulesService.getRulePriceTypes({
                        productId: getSelectedProductId(),
                        countryId: item.referencedGeographyId,
                        applicableFrom: getApplicableFrom()
                    });

                    promiseArray.push(promise);
                });

                $q.all(promiseArray).then(function (data) {
                    var priceTypeOptions = [];
                    subrule.basket.forEach(function(item, i) {
                        priceTypeOptions.push({
                            type: helperService.FieldTypes.Select,
                            name: 'defaultPriceType' + i,
                            properties: {
                                class: 't-custom-select-boxed',
                                items: getPriceTypesForSelect(data[i], item.referencedPriceTypeId)
                            }
                        });
                    });
                    $scope.currentlyShownBasket = subrule.basket;
                    $scope.currentlyShownPriceTypeOptions = priceTypeOptions;

                }); 
            };
            $scope.showDefaultBasket = function() {
                $scope.selectedSubRuleIndex = undefined;

                $scope.definition.referencedData.forEach(function (subRule, index) {
                    subRule.selected = false;
                });
                $scope.currentlyShownBasket = $scope.definition.defaultBasket;
                $scope.currentlyShownPriceTypeOptions = $scope.defaultReferencedPriceTypeOptions;
            }

            var getSelectedCountryId = function() {
                var id = 0;
                regionCountryOptions.properties.items.forEach(function(region) {
                    if (region.selected) {
                        region.items.forEach(function(country) {
                            if (country.selected)
                                id = country.id;
                        });
                    }
                });
                return id;
            };
            var getSelectedRuleTypeId = function() {
                var id = 0;
                ruleTypeOptions.properties.items.forEach(function (item) {
                    if (item.selected)
                        id = item.id;
                });
                return id;
            };
            var getSelectedVersionId = function() {
                var id = 0;
                versionOptions.properties.items.forEach(function (item) {
                    if (item.selected)
                        id = item.versionId;
                });
                return id;
            };
            var getSelectedSimulationOption = function () {
                var simulationOption = _.find(simulationOptions.properties.items, function (s) {
                    return s.selected == true;
                });

                return simulationOption;
            }
            var getSelectedSimulationOptionItem = function () {
                var simulation = getSelectedSimulationOption();
                if (simulation == null) {
                    return null;
                }
                var item = _.find(simulation.items, function (i) {
                    return i.selected == true;
                });

                return item;
            }
            var changeSelectedSaveId = function (saveId) {
                var parentSimulation = simulationOptions.properties.items;
                parentSimulation.forEach(function (parent) {
                    var childFound = false;
                    for (var i = 0; i < parent.items.length; i++) {
                        parent.items[i].selected = parent.items[i].id == saveId;
                        if (!childFound) childFound = parent.items[i].id == saveId;
                    }
                    parent.selected = childFound;
                });
            };
            var getApplicableFrom = function() {
                var result;

                if (applicableFromFilter.properties.items) {
                    applicableFromFilter.properties.items.forEach(function (date) {
                        if (date.selected) {
                            result = date.text;
                        }
                    });
                }
                if(result)
                    result = result + '-01 00:00:00Z'; //BAD but necessary...
                return result;
            }
            var getSelectedMathOption = function() {
                var result = {};
                $scope.mathOptions.properties.items.forEach(function(math) {
                    if (math.selected)
                        result = math;
                });
                return result;
            };

            $scope.onPriceTypeChanged = function (item, index) {
                item.referencedPriceTypeId = _.find($scope.currentlyShownPriceTypeOptions[index].properties.items, function (pt) { return pt.selected; }).id;
                item.edited = true;
                $scope.canSave = true;
            };

            $scope.onEditCountryDiscount = function(item) {
                item.edited = true;
                $scope.canSave = true;
            }

            var getPriceTypesForSelect = function (data, selected) {
                data.forEach(function(item) {
                    item.text = item.shortName;
                    item.selected = item.id == selected;
                });
                return data;
            };
            var setRuleMath = function () {
                $scope.ruleMath.forEach(function(ruleMath) {
                    if (ruleMath.id == $scope.definition.gprmMathId)
                        $scope.definition.gprmMath = ruleMath.name;
                });

                if ($scope.definition.referencedData) {
                    $scope.definition.referencedData.forEach(function(data) {
                        $scope.ruleMath.forEach(function (ruleMath) {
                            if (data.gprmMathId == ruleMath.id)
                                data.gprmMath = ruleMath.name;
                        });
                    });                  
                }
            };
            var getRegionsForFilter = function(data) {
                var result = [];

                data.forEach(function(region, i) {
                    var newRegion = {
                        id: region.region.id,
                        text: region.region.name,
                        sname: region.region.name.replace(/\s+/g, ''),
                        items: [],
                        selected: (($scope.lastSettingsChosen.defaultRegionId && $scope.lastSettingsChosen.defaultRegionId == region.region.id)
                            || (!$scope.lastSettingsChosen.defaultRegionId && i == 0)) ? true : false
                    };
                    var countrySelected = false;
                    region.countries.forEach(function(country) {
                        var newCountry = {
                            id: country.id,
                            text: country.name,
                            name: country.name.replace(/\s+/g, '')
                        };

                        if ($scope.lastSettingsChosen.defaultCountryId && $scope.lastSettingsChosen.defaultCountryId == newCountry.id) {
                            newCountry.selected = true;
                            countrySelected = true;
                        }

                        newRegion.items.push(newCountry);
                    });

                    if (!countrySelected && newRegion.selected) {
                        newRegion.items[0].selected = true;
                    }

                    result.push(newRegion);
                });

                return result;
            }
            var getPriceTypeFilterItems = function(priceTypes, priceTypeIdToSelect) {
                var items = [];
                if (priceTypeIdToSelect) {
                    priceTypes.forEach(function(priceType) {
                        var item = { id: priceType.id, text: priceType.shortName };
                        item.selected = item.id == priceTypeIdToSelect;
                        items.push(item);
                    });
                } else {
                    priceTypes.forEach(function (priceType, i) {
                        var item = { id: priceType.id, text: priceType.shortName };
                        item.selected = i ==0;
                        items.push(item);
                    });
                }                
                return items;
            };      
            var getCountryFilterItems = function() {
                var items = [];
                $scope.countries.forEach(function(country, index) {
                    items.push({
                        id: country.id,
                        text: country.name,
                        selected: index == 0
                    });
                });
                return items;
            };
            var getFilterItems = function(list) {
                var items = [];
                list.forEach(function(element, i) {
                    items.push({
                        id: element.id,
                        text: element.name,
                        selected: i == 0
                    });
                });
                return items;
            }
            var userMapping;
            var getBaseScreenInfo = function () {
                var userMappingPromise = userService.getUserMapping();
                var userPromise = userService.getLastSettingsChosen();
                var regionCountriesPromise = countryService.getRegionsAndCountries();
                var productPromise = productService.getAllProducts();
                var ruleMathPromise = rulesService.getRuleMath();
                var userInfoPromise = userService.getUserInfo();
                var promiseArray = [regionCountriesPromise, ruleMathPromise, userPromise, userMappingPromise];
                if (validate) {
                    var next = loadService.getNext($routeParams.loadId, helperService.LoadItemNames.Rule);
                    promiseArray.push(next);
                } else {
                    promiseArray.push(productPromise);
                }
                promiseArray.push(userInfoPromise);
                $q.all(promiseArray).then(function(data) {
                    if (validate) {
                        $scope.products = [];
                        usableProductsAndGeographyIds = data[4];
                    } else {
                        $scope.products = data[4];
                    }
                    $scope.userService = userService;
                    $scope.lastSettingsChosen = data[2];
                    var regionCountries = getRegionsForFilter(data[0]);
                    $scope.ruleMath = data[1];
                    userMapping = data[3];

                    var mathOptions = getFilterItems($scope.ruleMath);

                    $scope.mathOptions.properties.items = mathOptions;
                        regionCountryOptions.properties.items = regionCountries;
                    if (validate) {
                        filterGeographyForProduct();
                    }
                    if (forecast) {
                        $scope.products.forEach(function (p, i) { p.selected = p.id == $scope.lastSettingsChosen.defaultProductId; });
                        simulationOptions.hide = false;
                        versionOptions.hide = true;
                        getSimulations();
                    }
                    else {
                        //JH Here
                        $scope.products.splice(0, 0, { id: 0, name: 'All products', selected: true });
                        simulationOptions.hide = true;
                        versionOptions.hide = false;
                        getVersions();
                    }
                    
                });
            };
            var filterGeographyForProduct = function () {
                var selectedProductId = getSelectedProductId();
                var usableGeographiesForProduct = _.find(usableProductsAndGeographyIds, function (p) { return p.productId == selectedProductId == 0 ? 15 : selectedProductId; });

                var regionsToRemove = [];
                regionCountryOptions.properties.items.forEach(function (region) {
                    var countriesToRemove = [];
                    region.items.forEach(function (country) {
                        if (usableGeographiesForProduct == null || !_.any(usableGeographiesForProduct.geographyIds, function (geo) {
                            return country.id == geo;
                        })) {
                            countriesToRemove.push(country);
                        }
                    });
                    countriesToRemove.forEach(function (toRemove) {
                        region.items.splice(region.items.indexOf(toRemove), 1);
                    });
                    if (region.items.length == 0) {
                        regionsToRemove.push(region);
                    }
                });

                regionsToRemove.forEach(function (toRemove) {
                    regionCountryOptions.properties.items.splice(regionCountryOptions.properties.items.indexOf(toRemove), 1);
                });

                if (!getSelectedCountryId()) {
                    var selectedRegion = _.find(regionCountryOptions.properties.items, function(item) { return item.selected; });
                    if (selectedRegion) {
                        selectedRegion.items[0].selected = true;
                    } else {
                        if (regionCountryOptions.properties.items.length > 0) {
                            regionCountryOptions.properties.items[0].selected = true;
                            regionCountryOptions.properties.items[0].items[0].selected = true;
                        }                        
                    }
                }
               
            };
            var getSimulations = function ()
            {
                simulationService.getSimulations().then(function (data) {
                    simulationOptions.properties.items = simulationService.prepareSimulationForFilter(data);
                    
                    getCacheSimulation();
                });
            }
            var getVersions = function () {
                var selectedProductId = getSelectedProductId();
                var selectedCountryId = getSelectedCountryId();
                var selectedRuleTypeId = getSelectedRuleTypeId();

                versionService.getRulesVersions({
                    productId: selectedProductId,
                    geographyId: selectedCountryId,
                    ruleTypeId: selectedRuleTypeId
                }).then(function(data) {
                    $scope.versions = data;
                    var versionItems = versionService.getVersionsForFilter($scope.versions, userMapping);
                    versionOptions.properties.items = versionItems;

                    getApplicableFroms();
                });
            };
            var getApplicableFroms = function () {

                var geographyId = getSelectedCountryId();
                var productId = getSelectedProductId();
                var ruleTypeId = getSelectedRuleTypeId();
                var versionId = forecast ? 1 : getSelectedVersionId();
                //TODO: get correct id (JH)
                var request = {
                    geographyId: geographyId,
                    gprmRuleTypeId: ruleTypeId,
                    productId: productId,
                    versionId: versionId,
                    forecast: forecast,
                    simulationId: $scope.simulationId
                };


                rulesService.getApplicableFromList(request).then(function(data) {
                    applicableFromFilter.properties.items = data;
                    applicableFromFilter.properties.items.forEach(function (appFrom, index) {
                        appFrom.originalText = appFrom.text;
                        appFrom.text = appFrom.text.substring(0, 7);
                        appFrom.selected = index == 0;
                    });

                    getData();                    

                });
                
            };
            var getCacheSimulation = function ()
            {
                simulationService.getFirstSimulation().then(function (result) {
                    if (result != "null") {
                        $scope.simulationId = result.id;
                        $scope.isCurrentUser = result.isCurrentUser;
                        changeSelectedSaveId(result.saveId);
                        if (result.isLaunch)
                            changeSelectedProductId(15);
                        getApplicableFroms();
                    }
                    else
                    {
                        loadCacheAndReloadData();
                    }
                    
                });
            }
            var loadCacheAndReloadData = function () {
                var saveId = getSelectedSimulationOptionItem().id;
                if (saveId && saveId != 0) {
                    simulationService.loadSimulation(saveId, getSelectedProductId()).then(function (data) {
                        $scope.simulationId = data.id;
                        $scope.isCurrentUser = data.isCurrentUser;
                        getApplicableFroms();
                    });
                }
            };         
            var getData = function () {
                $scope.showAddReferencedCountry = false;
                $scope.showAddSubRule = false;
                $scope.defaultReferencedPriceTypeOptions = [];

                var geographyId = getSelectedCountryId();
                var productId = getSelectedProductId();
                var ruleTypeId = getSelectedRuleTypeId();
                var versionId = forecast ? 1 : getSelectedVersionId();
                var applicableFrom = getApplicableFrom();
                var simulationId = $scope.simulationId;

                var request = {
                    geographyId: geographyId,
                    gprmRuleTypeId: ruleTypeId,
                    productId: productId,
                    versionId: versionId,
                    applicableFrom: applicableFrom,
                    forecast: forecast,
                    simulationId: simulationId,
                    validate: validate
                };

                rulesService.getRules(request).then(function (result) {
                    $scope.priceTypeOptions = [];
                    $scope.definition = result;
                    $scope.selectedSubRuleIndex = undefined;

                    var priceTypePromise = rulesService.getRulePriceTypes({
                        productId: getSelectedProductId(),
                        countryId: getSelectedCountryId(),
                        applicableFrom: getApplicableFrom()
                    });

                    if (!$scope.definition.defaultBasket)
                        $scope.definition.defaultBasket = [];

                    var promiseArray = [priceTypePromise];

                    $scope.definition.defaultBasket.forEach(function (item) {
                        var promise = rulesService.getRulePriceTypes({
                            productId: getSelectedProductId(),
                            countryId: item.referencedGeographyId,
                            applicableFrom: getApplicableFrom()
                        });

                        promiseArray.push(promise);
                    });

                    $q.all(promiseArray).then(function (dataArray) {
                        $scope.priceTypes = dataArray[0];
                        var priceTypeItems = getPriceTypeFilterItems($scope.priceTypes);
                        $scope.allPriceTypeOptions.properties.items = priceTypeItems;

                        $scope.definition.defaultBasket.forEach(function (item, i) {
                            $scope.defaultReferencedPriceTypeOptions.push({
                                type: helperService.FieldTypes.Select,
                                name: 'defaultPriceType' + i,
                                properties: {
                                    class: 't-custom-select-boxed',
                                    items: getPriceTypesForSelect(dataArray[i + 1], item.referencedPriceTypeId)
                                }
                            });
                        });

                        $scope.currentlyShownBasket = $scope.definition.defaultBasket;
                        $scope.currentlyShownPriceTypeOptions = $scope.defaultReferencedPriceTypeOptions;

                        setStyleForItems($scope.definition.reviewedPriceTypeOptions, $scope.definition.selectedReviewedPriceTypeId);
                        setRuleMath();
                        setStyleForItems($scope.ruleMath, $scope.definition.gprmMathId);

                        $scope.canRemoveRules = _.filter($scope.definition.referencedData, function (ru) { return ru.active; }).length > 1;
                    });
                });

            };

            $scope.updateVersion = function () {

                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/saveRuleModal.html',
                    controller: 'SaveRuleModalController',
                    backdrop: 'static',
                    resolve: {
                        infos: function () {
                            return {
                                year: getApplicableFrom().substring(0, 4),
                                month: getApplicableFrom().substring(5, 7)
                            }
                        }
                    }
                });
                modalInstance.result.then(function (save) {
                    var data = {
                        ruleDefinition: $scope.definition,
                        applicableFrom: save,
                        geographyId: getSelectedCountryId(),
                        productId: getSelectedProductId(),
                        gprmRuleTypeId: getSelectedRuleTypeId(),
                        simulationId: $scope.simulationId
                    };
                    rulesService.cacheRule(data).then(function (result) {
                        if (result) {
                            getSimulations();
                        }
                    });
                });

                $scope.edited = false;
            };
            $scope.createSimulation = function () {

                var modalInstance = $modal.open({
                    templateUrl: 'Content/js/pricecare/modal/createSimulationModal.html',
                    controller: 'CreateSimulationModalController',
                    backdrop: 'static',
                    resolve: {
                        infos: function () {
                            return {
                                productId: getSelectedProductId(),
                            };
                        }
                    }
                });

                modalInstance.result.then(function (data) {
                    simulationService.create(data).then(function (data) {
                        $scope.simulationId = data.id;
                        $scope.isCurrentUser = data.isCurrentUser;
                        changeSelectedSaveId(data.saveId);
                        getData();
                    });
                }, function () {
                });
            };

            getBaseScreenInfo();
        }
    ]);
});
