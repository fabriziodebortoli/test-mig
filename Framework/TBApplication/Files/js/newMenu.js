// Caricamento applicazione angular
var menuModule = angular.module('menuApp', ['ui.bootstrap', 'ngRoute', 'ang-drag-drop', 'ui.bootstrap.contextMenu']);

menuModule.service('imageService', imageService);
menuModule.service('menuService', menuService);
menuModule.service('localizationService', localizationService);
menuModule.service('settingsService', settingsService);
menuModule.service('generalFunctionsService', generalFunctionsService);
menuModule.service('loggingService', loggingService);
menuModule.service('easyStudioService', easyStudioService);

menuModule.controller('MainMenuController', MainMenuController);
menuModule.controller('CentralPanelController', CentralPanelController);
menuModule.controller('NewMenuTemplateController', NewMenuTemplateController);
menuModule.controller('NewNavBarController', NewNavBarController);
menuModule.controller('FooterController', FooterController);
menuModule.controller('HiddenTilesController', HiddenTilesController);
menuModule.controller('ChangePasswordController', ChangePasswordController);
menuModule.controller('LoginController', LoginController);
menuModule.controller('ChangeThemeController', ChangeThemeController);
menuModule.controller('SearchController', SearchController);
menuModule.controller('FavoritesController', FavoritesController);
menuModule.controller('MostUsedController', MostUsedController);
menuModule.controller('ProductInfoController', ProductInfoController);
menuModule.controller('NotificationController', NotificationController);
menuModule.controller('UserController', UserController);
menuModule.controller('CalendarController', CalendarController);
menuModule.controller('TbsSidedropController', TbsSidedropController);
menuModule.controller('TbfSidedropController', TbfSidedropController);
menuModule.controller('CustomizationContextController', CustomizationContextController);
menuModule.controller('ESCloneDocumentController', ESCloneDocumentController);


menuModule.config(routingProvider);


menuModule.directive('ngMenuitemelement', function () {
    return {
        
        restrict: 'AEC',
        templateUrl: 'templates/Common/menuItemElement.html'
    };
});

menuModule.directive('ngMenuitemimage', function () {
    return {

        restrict: 'AEC',
        templateUrl: 'templates/Common/menuItemImage.html'
    };
});

menuModule.directive('ngItemcustomizationsdropdown', function () {
    return {

        restrict: 'AEC',
        templateUrl: 'templates/Common/itemCustomizationsDropdown.html'
    };
});


menuModule.directive('ngTilegrouprecursive', function () {
    return {

        restrict: 'AEC',
        templateUrl: 'templates/newTileGroupRecursive.html'
    };
});




menuModule.filter('repeatFilter', function (generalFunctionsService) {
    return function (items) {
        return generalFunctionsService.ToArray(items);
	}
});

menuModule.filter('collapseFilter', function ($log) {
    return function (items, filter) {
        var filtered = [];
        if (filter == undefined || filter == '')
            return items;

        for (var i = 0; i < items.length; i++) {
            var current = items[i];

            var stringone = JSON.stringify(current);
            var ind = stringone.toLowerCase().indexOf(filter.toLowerCase());
            current.collapsed = ind < 0;
            if (ind > -1) {
                filtered.push(current);
            }
        }
        return filtered;
    }
});
menuModule.directive('ngEnter', function () {
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


