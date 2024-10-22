define(['pricecare/services/module'], function (module) {
    'use strict';
    module.factory('RowFactory', ['helperService', function (helperService) {

        function RowFactory(columns, countries, priceTypes, cellsBehaviour) {
            this.columns = columns;
            this.countries = countries;
            this.priceTypes = priceTypes;
            this.cellsBehaviour = cellsBehaviour;
        }

        RowFactory.prototype.createTitleRow = function () {
            // Create title row
            var row = {
                title: true,
                cells: [{}] // Empty cell for the first column of header
            };
            this.columns.forEach(function (item) {
                row.cells.push({
                    title: true,
                    value: item.label,
                    baseValue: item.value
                });
            });
            return row;
        };

        RowFactory.prototype.createHeaderRow = function (title, span) {
            // Create header row
            var row = {
                pop: true,
                title: true,
                cells: [{value: title, title: true, colspan: span }],

            };
            if (!span) {
                this.columns.forEach(function() {
                    row.cells.push({});
                });
            }

            return row;
        };

        RowFactory.prototype.createPriceRows = function (prices) {
            var countryGroups = _.groupBy(prices, 'geographyId');

            var rows = [];
            var regionRows = [];
            var parentRegionRows = [];
            for (var countryId in this.countries) {
                var country = this.countries[countryId];
                var parentTypeRow = {};

                var countryData = countryGroups[country.id];
                if (countryData) {
                    var priceTypeGroups = _.groupBy(countryData, 'priceTypeId');
                    for (var priceTypeId in priceTypeGroups) {
                        var priceTypeData = priceTypeGroups[priceTypeId];
                        var parentTypeReviewed = _.find(priceTypeData, function (p) { return p.reviewed });

                        if (parentTypeReviewed != null && country.id == parentTypeReviewed.geographyId && this.cellsBehaviour.useOnlyReviewedCells) {
                            parentTypeRow.cells
                            = [
                                {
                                    value:
                                      country.name + " - " + _.find(this.priceTypes, function (pt) { return pt.id == priceTypeId; }).shortName,
                                    country: country.id,
                                    actions: [
                                    {
                                        text: 'Expand',
                                        class: 'table-expandable-row icon-collapsed',
                                        click: function (cell, row, table, action) {
                                            row.expanded = row.expanded ? false : true;
                                            action.text = row.expanded ? "Collapse" : "Expand";
                                            action.class = row.expanded ? "table-expandable-row icon-expanded" : "table-expandable-row icon-collapsed";
                                        }
                                    }
                                    ]
                                }
                            ];
                        }

                        var priceTypeRow = {
                            cells: [
                                {
                                    value: country.name + " - " + _.find(this.priceTypes, function (pt) { return pt.id == priceTypeId; }).shortName,
                                    country: country.id
                                }
                            ]
                        };

                        if (this.cellsBehaviour.useOnlyReviewedCells)
                            priceTypeRow.parentRow = parentTypeRow;

                        for (var i = 0; i < this.columns.length; i++) {
                            var price = priceTypeData[i];
                            if (price) {
                                var newCell = {
                                    originalPrice: helperService.clone(price),
                                    price: price,
                                };

                                newCell.rightMenu = this.cellsBehaviour.getRightMenu(newCell);
                                newCell.leftMenu = this.cellsBehaviour.getLeftMenu(newCell);
                                this.cellsBehaviour.setCellDisplay(newCell);
                                if (parentTypeReviewed != null && country.id == parentTypeReviewed.geographyId && this.cellsBehaviour.useOnlyReviewedCells)
                                    parentTypeRow.cells.push(newCell);
                                else
                                    priceTypeRow.cells.push(newCell);
                            }
                        }
                        if (parentTypeReviewed != null && country.id == parentTypeReviewed.geographyId && this.cellsBehaviour.useOnlyReviewedCells)
                            parentRegionRows.push(parentTypeRow);
                        else
                            regionRows.push(priceTypeRow);
                    }
                }
                rows = rows.concat(parentRegionRows);
                if (regionRows.length >= 1) {
                    rows = rows.concat(regionRows);
                }

                parentRegionRows = [];
                regionRows = [];

            }
            return rows;
        };

        return RowFactory;

    }]);
});