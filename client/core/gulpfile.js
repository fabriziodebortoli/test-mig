/* eslint-disable */
var gulp = require('gulp'),
    path = require('path'),
    ngc = require('@angular/compiler-cli/src/main').main,
    rollup = require('gulp-rollup'),
    rename = require('gulp-rename'),
    del = require('del'),
    runSequence = require('run-sequence'),
    inlineResources = require('./tools/gulp/inline-resources');

const rootFolder = path.join(__dirname);
const srcFolder = path.join(rootFolder, 'src');
const tmpFolder = path.join(rootFolder, '.tmp');
const buildFolder = path.join(rootFolder, 'build');
const distFolder = path.join(rootFolder, 'dist');

/**
 * 1. Delete /dist folder
 */
gulp.task('clean:dist', function() {

    // Delete contents but not dist folder to avoid broken npm links
    // when dist directory is removed while npm link references it.
    return deleteFolders([distFolder + '/**', '!' + distFolder]);
});

/**
 * 2. Clone the /src folder into /.tmp. If an npm link inside /src has been made,
 *    then it's likely that a node_modules folder exists. Ignore this folder
 *    when copying to /.tmp.
 */
gulp.task('copy:source', function() {
    return gulp.src([`${srcFolder}/**/*`, `!${srcFolder}/node_modules`])
        .pipe(gulp.dest(tmpFolder));
});

/**
 * 3. Inline template (.html) and style (.css) files into the the component .ts files.
 *    We do this on the /.tmp folder to avoid editing the original /src files
 */
gulp.task('inline-resources', function() {
    return Promise.resolve()
        .then(() => inlineResources(tmpFolder));
});

/**
 * 4. Run the Angular compiler, ngc, on the /.tmp folder. This will output all
 *    compiled modules to the /build folder.
 *
 *    As of Angular 5, ngc accepts an array and no longer returns a promise.
 */
gulp.task('ngc', function() {
    ngc(['--project', `${tmpFolder}/tsconfig.es5.json`]);
    return Promise.resolve()
});

/**
 * 5. Run rollup inside the /build folder to generate our Flat ES module and place the
 *    generated file into the /dist folder
 */
gulp.task('rollup:fesm', function() {
    return gulp.src(`${buildFolder}/**/*.js`)
        // transform the files here.
        .pipe(rollup({

            // Bundle's entry point
            // See "input" in https://rollupjs.org/#core-functionality
            input: `${buildFolder}/index.js`,

            // Allow mixing of hypothetical and actual files. "Actual" files can be files
            // accessed by Rollup or produced by plugins further down the chain.
            // This prevents errors like: 'path/file' does not exist in the hypothetical file system
            // when subdirectories are used in the `src` directory.
            allowRealFiles: true,

            // A list of IDs of modules that should remain external to the bundle
            // See "external" in https://rollupjs.org/#core-functionality
            external: [
                '@angular/animation', '@angular/core', '@angular/common', '@angular/http', '@angular/material', '@angular/router', '@angular/forms', '@angular/animations', '@angular/material/index', '@angular/forms',
                'rxjs/Rx', 'rxjs/Observable', 'rxjs/add/observable/of', 'rxjs/add/operator/map', 'rxjs/add/operator/toPromise', 'rxjs/BehaviorSubject', 'rxjs/Subject',
                'rxjs/add/operator/catch', 'rxjs/add/observable/throw', 'rxjs/add/operator/distinctUntilChanged', 'rxjs/add/operator/filter', 'rxjs/add/operator/first', 'rxjs/add/observable/interval',
                'rxjs/add/observable/timer', 'rxjs/add/operator/take', 'rxjs/add/operator/takeUntil', 'rxjs/add/operator/toArray', 'rxjs/add/operator/do', 'rxjs/add/operator/repeat',
                'rxjs/add/operator/timeout', 'rxjs/add/operator/share', 'rxjs/operator/map', 'rxjs/operator/pluck', 'rxjs/operator/distinctUntilChanged',
                'rxjs/Subscription', 'rxjs/Observer', 'rxjs/observable/ErrorObservable', 'rxjs/util/TimeoutError', 'rxjs/add/operator/reduce',
                'rxjs/add/observable/combineLatest', 'rxjs/operator/reduce', 'rxjs/Subscriber', 'rxjs/util/isNumeric',
                '@telerik/kendo-intl', '@progress/kendo-data-query', '@progress/kendo-angular-dialog', '@progress/kendo-angular-layout', '@progress/kendo-angular-popup',
                '@progress/kendo-angular-buttons', '@progress/kendo-angular-inputs', '@progress/kendo-angular-dateinputs', '@progress/kendo-angular-dropdowns', '@progress/kendo-angular-grid',
                '@progress/kendo-angular-charts', 'hammerjs', '@taskbuilder/icons', 'angular-tree-component', 'json8-patch', 'lodash'
            ],

            // Format of generated bundle
            // See "format" in https://rollupjs.org/#core-functionality
            format: 'es',

            // Skip THIS_IS_UNDEFINED warnings 
            onwarn: function(warning) {
                if (warning.code === 'THIS_IS_UNDEFINED') {
                    return;
                } else {
                    console.warn(warning.message);
                }
            }
        }))
        .pipe(gulp.dest(distFolder));
});

/**
 * 6. Run rollup inside the /build folder to generate our UMD module and place the
 *    generated file into the /dist folder
 */
gulp.task('rollup:umd', function() {
    return gulp.src(`${buildFolder}/**/*.js`)
        // transform the files here.
        .pipe(rollup({

            // Bundle's entry point
            // See "input" in https://rollupjs.org/#core-functionality
            input: `${buildFolder}/index.js`,

            // Allow mixing of hypothetical and actual files. "Actual" files can be files
            // accessed by Rollup or produced by plugins further down the chain.
            // This prevents errors like: 'path/file' does not exist in the hypothetical file system
            // when subdirectories are used in the `src` directory.
            allowRealFiles: true,

            // A list of IDs of modules that should remain external to the bundle
            // See "external" in https://rollupjs.org/#core-functionality
            external: [
                '@angular/animation', '@angular/core', '@angular/common', '@angular/http', '@angular/material', '@angular/router', '@angular/forms', '@angular/animations', '@angular/material/index', '@angular/forms',
                'rxjs/Rx', 'rxjs/Observable', 'rxjs/add/observable/of', 'rxjs/add/operator/map', 'rxjs/add/operator/toPromise', 'rxjs/BehaviorSubject', 'rxjs/Subject',
                'rxjs/add/operator/catch', 'rxjs/add/observable/throw', 'rxjs/add/operator/distinctUntilChanged', 'rxjs/add/operator/filter', 'rxjs/add/operator/first', 'rxjs/add/observable/interval',
                'rxjs/add/observable/timer', 'rxjs/add/operator/take', 'rxjs/add/operator/takeUntil', 'rxjs/add/operator/toArray', 'rxjs/add/operator/do', 'rxjs/add/operator/repeat',
                'rxjs/add/operator/timeout', 'rxjs/add/operator/share', 'rxjs/operator/map', 'rxjs/operator/pluck', 'rxjs/operator/distinctUntilChanged',
                'rxjs/Subscription', 'rxjs/Observer', 'rxjs/observable/ErrorObservable', 'rxjs/util/TimeoutError', 'rxjs/add/operator/reduce',
                'rxjs/add/observable/combineLatest', 'rxjs/operator/reduce', 'rxjs/Subscriber', 'rxjs/util/isNumeric',
                '@telerik/kendo-intl', '@progress/kendo-data-query', '@progress/kendo-angular-dialog', '@progress/kendo-angular-layout', '@progress/kendo-angular-popup',
                '@progress/kendo-angular-buttons', '@progress/kendo-angular-inputs', '@progress/kendo-angular-dateinputs', '@progress/kendo-angular-dropdowns', '@progress/kendo-angular-grid',
                '@progress/kendo-angular-charts', 'hammerjs', '@taskbuilder/icons', 'angular-tree-component', 'json8-patch', 'lodash'
            ],

            // Format of generated bundle
            // See "format" in https://rollupjs.org/#core-functionality
            format: 'umd',

            // Export mode to use
            // See "exports" in https://rollupjs.org/#danger-zone
            exports: 'named',

            // The name to use for the module for UMD/IIFE bundles
            // (required for bundles with exports)
            // See "name" in https://rollupjs.org/#core-functionality
            name: 'taskbuilder-core',

            // See "globals" in https://rollupjs.org/#core-functionality
            globals: {
                typescript: 'ts',
                '@angular/core': 'core',
                '@angular/common': 'common',
                '@angular/material': 'material',
                '@angular/router': 'router',
                '@angular/http': 'http',
                'rxjs/Observable': 'Observable',
                'rxjs/Rx': 'Rx',
                'json8-patch': 'json8Patch',
                'rxjs/BehaviorSubject': 'BehaviorSubject',
                'rxjs/Subject': 'Subject',
                'rxjs/operator/map': 'map$1',
                'rxjs/operator/pluck': 'pluck',
                'rxjs/operator/distinctUntilChanged': 'distinctUntilChanged$1',
                'rxjs/Subscription': 'Subscription',
                'rxjs/Observer': 'Observer',
                'rxjs/observable/ErrorObservable': 'ErrorObservable',
                'rxjs/util/TimeoutError': 'TimeoutError',
                'rxjs/operator/reduce': 'reduce$1',
                'rxjs/Subscriber': 'Subscriber',
                'rxjs/util/isNumeric': 'isNumeric',
                '@angular/forms': 'forms',
                '@angular/material/index': 'index',
                '@taskbuilder/icons': 'icons',
                '@progress/kendo-angular-dialog': 'kendoAngularDialog',
                '@progress/kendo-angular-layout': 'kendoAngularLayout',
                '@progress/kendo-angular-popup': 'kendoAngularPopup',
                '@progress/kendo-angular-buttons': 'kendoAngularButtons',
                '@progress/kendo-angular-inputs': 'kendoAngularInputs',
                '@progress/kendo-angular-dateinputs': 'kendoAngularDateinputs',
                '@progress/kendo-angular-dropdowns': 'kendoAngularDropdowns',
                '@progress/kendo-angular-grid': 'kendoAngularGrid',
                '@progress/kendo-angular-charts': 'kendoAngularCharts',
                'angular-tree-component': 'angularTreeComponent',
                '@progress/kendo-data-query': 'kendoDataQuery',
                '@angular/animations': 'animations',
                '@telerik/kendo-intl': 'kendoIntl',
                'lodash': '_'
            },

            // Skip THIS_IS_UNDEFINED warnings 
            onwarn: function(warning) {
                if (warning.code === 'THIS_IS_UNDEFINED') {
                    return;
                } else {
                    console.warn(warning.message);
                }
            }

        }))
        .pipe(rename('taskbuilder-core.umd.js'))
        .pipe(gulp.dest(distFolder));
});

/**
 * Copia la cartella fonts e il file style.css nella /dist
 */
gulp.task('copy:styles', function() {
    gulp.src(`${srcFolder}/style/*`).pipe(gulp.dest(`${distFolder}/style/`));
});

/**
 * 7. Copy all the files from /build to /dist, except .js files. We ignore all .js from /build
 *    because with don't need individual modules anymore, just the Flat ES module generated
 *    on step 5.
 */
gulp.task('copy:build', function() {
    return gulp.src([`${buildFolder}/**/*`, `!${buildFolder}/**/*.js`])
        .pipe(gulp.dest(distFolder));
});

/**
 * 8. Copy package.json from /src to /dist
 */
gulp.task('copy:manifest', function() {
    return gulp.src([`${srcFolder}/package.json`])
        .pipe(gulp.dest(distFolder));
});

/**
 * 9. Copy README.md from / to /dist
 */
gulp.task('copy:readme', function() {
    return gulp.src([path.join(rootFolder, 'README.MD')])
        .pipe(gulp.dest(distFolder));
});

/**
 * 10. Delete /.tmp folder
 */
gulp.task('clean:tmp', function() {
    return deleteFolders([tmpFolder]);
});

/**
 * 11. Delete /build folder
 */
gulp.task('clean:build', function() {
    return deleteFolders([buildFolder]);
});

gulp.task('compile', function() {
    runSequence(
        'clean:dist',
        'copy:source',
        'inline-resources',
        'ngc',
        'rollup:fesm',
        'rollup:umd',
        'copy:styles',
        'copy:build',
        'copy:manifest',
        'copy:readme',
        'clean:build',
        'clean:tmp',
        function(err) {
            if (err) {
                console.log('ERROR:', err.message);
                deleteFolders([distFolder, tmpFolder, buildFolder]);
            } else {
                console.log('Compilation finished succesfully');
            }
        });
});

/**
 * Watch for any change in the /src folder and compile files
 */
gulp.task('watch', function() {
    gulp.watch(`${srcFolder}/**/*`, ['compile']);
});

gulp.task('clean', ['clean:dist', 'clean:tmp', 'clean:build']);

gulp.task('build', ['clean', 'compile']);
gulp.task('build:watch', ['build', 'watch']);
gulp.task('default', ['build:watch']);

/**
 * Deletes the specified folder
 */
function deleteFolders(folders) {
    return del(folders);
}