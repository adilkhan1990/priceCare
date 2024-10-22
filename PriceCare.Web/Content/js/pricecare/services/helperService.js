define(['pricecare/services/module'], function (module) {
    'use strict';
    module.service('helperService', ['$http', '$window', '$q', '$filter', function ($http, $window, $q, $filter) {

        var me = this;

        me.FieldTypes = {
            Label: 0,
            Checkbox: 1,
            NumericTextbox: 2,
            Textbox: 3,
            Select: 4,
            QuickSwitch: 5,
            Action: 6,
            SelectMultiLevel: 7,

            DynamicPop: 99
        };

        Object.freeze(me.FieldTypes);

        me.FieldTriggers = {
            OnEnter: 1,
            OnBlur: 2,
            OnChange: 4
        };

        Object.freeze(me.FieldTriggers);

        me.CurrencyTypes = {
            PriceCurrency: 1,
            EurosSpot:2,
            EurosBudget:3,
            DollarsSpot:4,
            DollarsBudget:5
        };

        Object.freeze(me.CurrencyTypes);

        me.LoadTag = {
            Empty: null,
            Loaded: "Loaded",
            Deleted: "Deleted",
            Edited: "Edited"
        };

        Object.freeze(me.LoadTag);

        Object.freeze(me.CurrencyTypes);

        me.RuleTypes = [
            {
                id: 0,
                text: "<",
                check: function(a, b) {
                    return a < b;
                }
            },
            {
                id: 2, text: "=<",
                check: function (a, b) {
                    return a <= b;
                }
            },
            {
                id: 3, text: "=",
                check: function (a, b) {
                    return a == b;
                }
            },
            {
                id: 4, text: ">",
                check: function (a, b) {
                    return a > b;
                }
            },
            {
                id: 5, text: ">=",
                check: function (a, b) {
                    return a >= b;
                }
            }
        ];
        
        Object.freeze(me.RuleTypes);

        me.LoadItemNames = {
            Currency: "Currencies",
            Country: "Country",
            Product: "Product",
            PriceType: "Price Types",
            NetData: "Net Data",
            Sku: "SKU",
            Volume: "Volume",
            Event: "Event",
            Rule: "Rule",
            Price: "Price"
        };

        me.SimulationTypes = {
            Budget: 0,
            Reference: 1,
            Public: 2,
            User: 3
        };

        Object.freeze(me.LoadItemNames);

        me.EqualityThreshold = 0.0001;

        me.itemsPerPage = 1000;
        me.itemsPerPageLimited = 100;

        me.makeid = function() {
            var text = "";
            var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            for (var i = 0; i < 5; i++)
                text += possible.charAt(Math.floor(Math.random() * possible.length));

            return text;
        };

        me.clone = function(source) {
            var target = {};
            for (var prop in source) {
                if (typeof source[prop] === 'object' || typeof source[prop] === 'Array') {
                    target[prop] = me.clone(source[prop]);
                }
                else {
                    target[prop] = source[prop];
                }
            }
            return target;
        };

        me.borderLeft = {
            left: true,
            bottom: true,
            top: true
        };

        me.borderMiddle = {
            bottom: true,
            top: true
        };

        me.borderRight = {
            right: true,
            bottom: true,
            top: true
        };

        me.borderAll = {
            left: true,
            right: true,
            bottom: true,
            top: true
        };

        me.formatDate = function(date) {
            return $filter('date')(date, 'dd-MM-yyyy HH:mm:ss');
        };

        me.formatDateMonthYear = function(date) {
            return $filter('date')(date, "MMM yyyy"); //todo fix quote
        };

        me.formatDateYear = function(date) {
            return $filter('date')(date, "yyyy"); //todo fix quote
        };

        me.formatNumber = function (number) {
            if(number)
                return number.toPrecision(5);
            return number;
        };

        me.formatPercentage = function (number) {
            if(number != undefined)
                return (number * 100).toFixed(2) + "%";
            return number;
        };

        me.formatNumberTo2SignificantNumber = function (number) {
            return number.toFixed(2);
        };

        me.validateEmail = function (email) {
            var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return re.test(email);
        };

        me.getObjectSize = function (obj) {
            var size = 0, key;
            for (key in obj) {
                if (obj.hasOwnProperty(key))  size++; 
            }
            return size;
        };

        me.getCurrencyFilter = function (layoutClass) {

            var layout = 't-custom-select-text';
            if (layoutClass)
                layout = layoutClass;

            return {
                type: me.FieldTypes.Select,
                name: 'currencies',
                properties:
                {
                    class: layout,
                    items: [
                        { text: 'Price currency', id: me.CurrencyTypes.PriceCurrency, selected: true },
                        { text: 'Euros spot', id: me.CurrencyTypes.EurosSpot },
                        { text: 'Euros budget', id: me.CurrencyTypes.EurosBudget },
                        { text: 'Dollars spot', id: me.CurrencyTypes.DollarsSpot },
                        { text: 'Dollars budget', id: me.CurrencyTypes.DollarsBudget }
                    ]
                }
            };
        };
        
    }]);

});