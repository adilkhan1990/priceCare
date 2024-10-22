/**
 * bootstraps angular onto the window.document node
 */
define([
    'require',
    'angular',
    'ngRoute',
    'ngCookies',
    'ui.bootstrap',
    'ui.sortable',
    'ui.utils',
    'pricecare/app',
    'pricecare/routes',
    'fileUpload',
   'angular-md5'
], function (require, ng) {
    'use strict';

    //require(['domReady!'], function (document) {
    ng.bootstrap(document, ['app']);
    //});
});