define(['pricecare/home/module'], function(module) {
    'use strict';
    module.controller('HomeController', [
        '$scope', '$rootScope', '$controller', '$http', 'helperService',
        function($scope, $rootScope, $controller, $http, helperService) {

            $scope.dynOptions = {
                directive: 'test-directive'
            };

            var getRegionForFilter = function(data) {
                var result = [];

                data.forEach(function (region) {
                    var newRegion = {
                        id: region.region.id,
                        text: region.region.name,
                        name: region.region.name.replace(/\s+/g, ''),
                        items: []
                    };

                    region.countries.forEach(function (country) {
                        var newCountry = {
                            id: country.id,
                            text: country.name,
                            name: country.name.replace(/\s+/g, '')
                        };

                        newRegion.items.push(newCountry);
                    });

                    result.push(newRegion);
                });

                return result;
            };

            $http.get("api/values/regioncountries").success(function(result) {
                var regionAndCountries = getRegionForFilter(result);
                
                $scope.filterOptions = {
                    header: {
                        items: [
                            {
                                type: helperService.FieldTypes.Label,
                                properties:
                                {
                                    text: "List prices in"
                                }
                            },
                            {
                                type: helperService.FieldTypes.Select,
                                properties:
                                {
                                    class: 't-custom-select-text',
                                    items: [
                                        { text: 'US Dollars', textShort: 'USD', selected: true, class: 'currency_USD' },
                                        { text: 'British Pounds', textShort: 'GBP', class: 'currency_GBP' },
                                        { text: 'Euros', textShort: 'EUR', class: 'currency_EUR' }
                                    ]
                                }
                            },
                            {
                                type: helperService.FieldTypes.Select,
                                properties: {
                                    class: 't-custom-select-text',
                                    items: [
                                        { text: 'V3.0', selected: true, class: 'small right', displayRight: '2 days old' },
                                        { text: 'V2.0', class: 'small right', displayRight: '10 days old' },
                                        { text: 'V1.0', class: 'small right', displayRight: '2 months old' }
                                    ]
                                }
                            }
                        ],
                        primaryDisplayOptions: [
                            {
                                type: helperService.FieldTypes.Action,
                                properties:
                                {
                                    text: 'Export to Excel',
                                    class: 'color-white icon-before icon-before-excel',
                                    callback: function () {
                                        console.log('EXPORT');
                                    }
                                }
                            },
                            {
                                type: helperService.FieldTypes.QuickSwitch,
                                properties: {
                                    label: 'View as:',
                                    class: 't-custom-select-text',
                                    items: [
                                        { text: 'Graph' },
                                        { text: 'Grid', selected: true }
                                    ]
                                }
                            },
                            {
                                type: helperService.FieldTypes.Select,
                                properties: {
                                    label: 'Order by:',
                                    class: 't-custom-select-text',
                                    items: [
                                        { text: 'A - Z', selected: true },
                                        { text: 'Z - A' },
                                        { text: 'None' }
                                    ]
                                }
                            }
                        ],
                        displayOptions: [
                            { type: helperService.FieldTypes.Checkbox, text: 'Display Only Changing Data', value: false, name: 'onlyChanging' },
                            {
                                type: helperService.FieldTypes.NumericTextbox,
                                text: 'Highlight when more than',
                                value: 15,
                                name: 'highlightWhenMore',
                                properties: {
                                    trigger: helperService.FieldTriggers.OnEnter,
                                    min: 0,
                                    max: 200
                                }
                            },
                            {
                                type: helperService.FieldTypes.Select,
                                text: 'Compare to',
                                name: 'compareTo',
                                properties:
                                {
                                    class: 't-custom-select-text',
                                    items: [
                                        { text: 'No comparison', selected: true },
                                        { text: 'Last year' },
                                        { text: '2 years ago' }
                                    ]
                                }
                            }
                        ],
                        showDisplayOptions: false
                    },
                    filters: [
                        {
                            type: helperService.FieldTypes.DynamicPop,
                            name: 'countries',
                            properties:
                            {
                                class: 't-custom-select-boxed',
                                directive: 'op-selection-tree-popup',
                                items: regionAndCountries,
                                getText: function () {
                                    return 'All countries';
                                }
                            }
                        },
                        {
                            type: helperService.FieldTypes.Select,
                            name: 'priceType',
                            properties:
                            {
                                class: 't-custom-select-boxed',
                                items: [
                                    { text: 'All Price Types', selected: true },
                                    { text: 'EXF - WHO (NN)' },
                                    { text: 'EXF - WHO (OP)' },
                                    { text: 'PEV - PUBLIC' },
                                    { text: 'PIV - PUBLIC' }
                                ]
                            }
                        },
                        {
                            type: helperService.FieldTypes.Textbox,
                            name: 'searchBox',
                            class: 'search',
                            properties: {
                                trigger: helperService.FieldTriggers.OnEnter | helperService.FieldTriggers.OnChange
                            }
                        }
                    ],
                    advancedFilters: [
                        {
                            type: helperService.FieldTypes.QuickSwitch,
                            name: 'priceType',
                            properties:
                            {
                                class: 't-custom-select-boxed',
                                items: [
                                    { text: 'All', selected: true },
                                    { text: 'Issues' }
                                ]
                            }
                        }
                    ],
                    showAdvancedFilters: false,
                    onChanged: function () {
                        console.log('changed');
                    }
                };
            });
            $scope.table = {
                columnFilteringOptions: {
                    offset: 0,
                    count: 5,
                    fixed: 2,
                },
                rows: [
                    {
                        title: true,
                        cells: [
                            {
                                title: true,
                                value: "Europe"
                            },
                            {
                                value: "EXF - WHO (OP)"
                            },
                            {
                                value: 99.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            },
                            {
                                value: 1.7756,
                                rightMenu: {
                                    type: helperService.FieldTypes.NumericTextbox,
                                    properties: {
                                        required: true,
                                        allowDecimal: true,
                                        trigger: helperService.FieldTriggers.OnEnter,
                                    }
                                }
                            }
                        ]
                    },
                    {
                        cells: [
                            {
                                value: "Belgium"
                            },
                            {
                                value: "EXF - WHO (OP)"
                            },
                            {
                                value: 1
                            },
                            {
                                value: 2
                            },
                            {
                                value: 3
                            },
                            {
                                value: 4
                            },
                            {
                                value: 5
                            },
                            {
                                value: 6
                            },
                            {
                                value: 7
                            },
                            {
                                value: 8
                            },
                            {
                                value: 9
                            },
                            {
                                value: 10
                            }
                        ]
                    }
                ]
            };
        }
    ]);
});