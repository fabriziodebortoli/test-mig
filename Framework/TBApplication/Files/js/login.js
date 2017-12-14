// Caricamento applicazione angular
var loginAppModule = angular.module('loginApp', ['ui.bootstrap', 'ui.bootstrap.contextMenu']);
loginAppModule.service('imageService', imageService);
loginAppModule.service('localizationService', localizationService);
loginAppModule.service('generalFunctionsService', generalFunctionsService);
loginAppModule.service('settingsService', settingsService);
loginAppModule.service('loggingService', loggingService);

loginAppModule.controller('LoginController', LoginController);
loginAppModule.controller('LoginHostController', LoginHostController);
loginAppModule.controller('ImageLoginController', ImageLoginController);
loginAppModule.controller('ChangePasswordController', ChangePasswordController);
loginAppModule.directive('ngEnter', function () {
	return function (scope, element, attrs) {
		element.bind("keydown keypress", function (event) {
			if (event.which === 13) {
				scope.$apply(function () {
					scope.$eval(attrs.ngEnter);
				});

				event.preventDefault();
			}
		});
	};
});