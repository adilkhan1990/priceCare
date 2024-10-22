require.config({

    // alias libraries paths
    paths: {
        'domReady': 'vendors/require/domReady',
        'angular': 'vendors/angular/angular',
        'ngRoute': 'vendors/angular/angular-route',
        'ngCookies': 'vendors/angular/angular-cookies',
        'ui.bootstrap': 'vendors/angular/ui-bootstrap-tpls-0.11.0',
        'underscore': 'vendors/underscore/underscore-min',
        'fileUpload': 'vendors/angular/angular-file-upload',
        'fileUploadShim': 'vendors/angular/angular-file-upload-shim',
        'fileUploadHtml5Shim': 'vendors/angular/angular-file-upload-html5-shim',
        'ui.sortable': 'vendors/angular/angular-ui-sortable',
        'ui.utils': 'vendors/angular/ui-utils',
        'angular-md5': 'vendors/angular/angular-md5'
    },

    // angular does not support AMD out of the box, put it in a shim
    shim: {
        ngRoute: {
            deps: ['angular', 'underscore'],
            exports: 'angular'
        },
        ngCookies: {
            deps: ['angular', 'underscore'],
            exports: 'angular'
        },
        'angular': {
            exports: 'angular'
        },
        'underscore': {
            exports: '_'
        },
        'ui.bootstrap': {
            deps: ['angular']
        },
        'ui.sortable': {
            deps: ['angular']
        },
        'ui.utils': {
            deps: ['angular']
        },
        fileUpload: {
            deps: ['angular', 'fileUploadShim', 'fileUploadHtml5Shim']
        },
        'angular-md5': {
            deps: ['angular', 'underscore'],
            exports: 'angular-md5'
        }
    },

    // kick start application
    deps: ['./bootstrap']
});