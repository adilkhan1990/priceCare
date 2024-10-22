define(['pricecare/services/module'], function(module) {
    'use strict';
    module.service('formatHelperService', [
        '$http', '$window', '$q', '$filter', function($http, $window, $q, $filter) {

            var me = this;

            me.formatNumber = function (number) {
                if (number == null)
                    return null;

                var isNegative = number < 0;

                var absNumber = Math.abs(number);
                var result = '';

                //if (absNumber < Math.pow(10, -9)) {
                //} else if (absNumber < Math.pow(10, -6)) {
                //} else if (absNumber < Math.pow(10, -3)) {
                //}

                if (absNumber < 10) {
                    result = (Math.round(absNumber * 1000) / 1000).toString();

                    switch (result.length) {
                    case 1:
                        result = result + '.000';
                        break;
                    case 2:
                        result = result + '000';
                        break;
                    case 3:
                        result = result + '00';
                        break;
                    case 4:
                        result = result + '0';
                        break;
                    }
                } else if (absNumber < 100) {
                    result = (Math.round(absNumber * 100) / 100).toString();
                    switch (result.length) {
                    case 1:
                        result = result + '0000';
                        break;
                    case 2:
                        result = result + '.00';
                        break;
                    case 3:
                        result = result + '00';
                        break;
                    case 4:
                        result = result + '0';
                        break;
                    }
                } else if (absNumber < 1000) {
                    result = (Math.round(absNumber * 10) / 10).toString();
                    switch (result.length) {
                    case 1:
                        result = result + '0000';
                        break;
                    case 2:
                        result = result + '000';
                        break;
                    case 3:
                        result = result + '.0';
                        break;
                    case 4:
                        result = result + '0';
                        break;
                    }
                } else if (absNumber < 1000000) {
                    var divided = absNumber / 1000;
                    if (divided < 10) {
                        result = (Math.round(divided * 1000) / 1000).toString();
                        switch (result.length) {
                            case 1:
                                result = (Math.round(divided * 100) / 100).toString() + '.00K';
                                break;
                            case 2:
                                result = (Math.round(divided * 100) / 100).toString() + '00K';
                                break;
                            case 3:
                                result = (Math.round(divided * 100) / 100).toString() + '0K';
                                break;
                            default:
                                result = (Math.round(divided * 100) / 100).toString() + 'K';
                                break;
                        }
                    } else if (divided < 100) {
                        result = (Math.round(divided * 100) / 100).toString();
                        switch (result.length) {
                            case 1:
                                result = (Math.round(divided * 10) / 10).toString() + '000K';
                                break;
                            case 2:
                                result = (Math.round(divided * 10) / 10).toString() + '.0K';
                                break;
                            case 3:
                                result = (Math.round(divided * 10) / 10).toString() + 'K';
                                break;
                            default:
                                result = (Math.round(divided * 10) / 10).toString() + 'K';
                                break;
                        }
                    } else if (divided < 1000) {
                        result = (Math.round(divided * 10) / 10).toString();
                        switch (result.length) {
                            case 1:
                                result = (Math.round(divided)).toString() + '000K';
                                break;
                            case 2:
                                result = (Math.round(divided)).toString() + '00K';
                                break;
                            case 3:
                                result = (Math.round(divided)).toString() + 'K';
                                break;
                            default:
                                result = (Math.round(divided)).toString() + 'K';
                                break;
                        }
                    }
                } else if (absNumber < 1000000000) {
                    var divided = absNumber / 1000000;
                    if (divided < 10) {
                        result = (Math.round(divided * 1000) / 1000).toString();
                        switch (result.length) {
                            case 1:
                                result = (Math.round(divided * 100) / 100).toString() + '.00M';
                                break;
                            case 2:
                                result = (Math.round(divided * 100) / 100).toString() + '00M';
                                break;
                            case 3:
                                result = (Math.round(divided * 100) / 100).toString() + '0M';
                                break;
                            default:
                                result = (Math.round(divided * 100) / 100).toString() + 'M';
                                break;
                        }
                    } else if (divided < 100) {
                        result = (Math.round(divided * 100) / 100).toString();
                        switch (result.length) {
                            case 1:
                                result = (Math.round(divided * 10) / 10).toString() + '000M';
                                break;
                            case 2:
                                result = (Math.round(divided * 10) / 10).toString() + '.0M';
                                break;
                            case 3:
                                result = (Math.round(divided * 10) / 10).toString() + 'M';
                                break;
                            default:
                                result = (Math.round(divided * 10) / 10).toString() + 'M';
                                break;
                        }
                    } else if (divided < 1000) {
                        result = (Math.round(divided * 10) / 10).toString();
                        switch (result.length) {
                            case 1:
                                result = (Math.round(divided)).toString() + '000M';
                                break;
                            case 2:
                                result = (Math.round(divided)).toString() + '00M';
                                break;
                            case 3:
                                result = (Math.round(divided)).toString() + 'M';
                                break;
                            default:
                                result = (Math.round(divided)).toString() + 'M';
                                break;
                        }
                    }
                } else if (absNumber < 1000000000000) {
                    var divided = absNumber / 1000000000;
                    if (divided < 10) {
                        result = (Math.round(divided * 1000) / 1000).toString();
                        switch (result.length) {
                        case 1:
                            result = (Math.round(divided * 100) / 100).toString() + '.00B';
                            break;
                        case 2:
                            result = (Math.round(divided * 100) / 100).toString() + '00B';
                            break;
                        case 3:
                            result = (Math.round(divided * 100) / 100).toString() + '0B';
                            break;
                        default:
                            result = (Math.round(divided * 100) / 100).toString() + 'B';
                            break;
                        }
                    } else if (divided < 100) {
                        result = (Math.round(divided * 100) / 100).toString();
                        switch (result.length) {
                        case 1:
                            result = (Math.round(divided * 10) / 10).toString() + '000B';
                            break;
                        case 2:
                            result = (Math.round(divided * 10) / 10).toString() + '.0B';
                            break;
                        case 3:
                            result = (Math.round(divided * 10) / 10).toString() + 'B';
                            break;
                        default:
                            result = (Math.round(divided * 10) / 10).toString() + 'B';
                            break;
                        }
                    } else if (divided < 1000) {
                        result = (Math.round(divided * 10) / 10).toString();
                        switch (result.length) {
                        case 1:
                            result = (Math.round(divided)).toString() + '000B';
                            break;
                        case 2:
                            result = (Math.round(divided)).toString() + '00B';
                            break;
                        case 3:
                            result = (Math.round(divided)).toString() + 'B';
                            break;
                        default:
                            result = (Math.round(divided)).toString() + 'B';
                            break;
                        }
                    }
                } else {
                    result = Math.round(absNumber).toString() + '.00';
                }

                if (isNegative) {
                    result = '-' + result;
                }

                return result;

            };
        }
    ]);
});