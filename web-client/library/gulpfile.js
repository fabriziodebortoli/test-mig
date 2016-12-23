// gulpfile.js
var gulp = require('gulp');

var formClientPath = '../form-client/node_modules/web-library/dist';
var reportingStudioClientPath = '../reporting-studio-client/node_modules/web-library/dist';

// clean dist folder
var clean = require('gulp-rimraf');
gulp.task('clean', [], function() {
    console.log("Clean all files in external dist folder");

    gulp.src(formClientPath + "/*", { read: false }).pipe(clean({ force: true }));
    gulp.src(reportingStudioClientPath + "/*", { read: false }).pipe(clean({ force: true }));
});

// copy dist folder
gulp.task('copy', [], function() {
    console.log("Coping dist to external projects");

    gulp.src(['dist/**/*']).pipe(gulp.dest(formClientPath));
    gulp.src(['dist/**/*']).pipe(gulp.dest(reportingStudioClientPath));
});

// copy dist folder in public repo
var deployPath = '../../../web-library/dist';
gulp.task('deploy', [], function() {
    console.log("Coping dist to public repo");

    gulp.src(deployPath + "/*", { read: false }).pipe(clean({ force: true }));
    gulp.src(['dist/**/*']).pipe(gulp.dest(deployPath));
});

gulp.task('default', ['clean', 'copy'], function() {

});