// Caricamento applicazione angular
var menuModule = angular.module('menuApp', ['ui.bootstrap', 'ngRoute', 'ang-drag-drop', 'ui.bootstrap.contextMenu']);

menuModule.service('imageService', imageService);
menuModule.service('menuService', menuService);
menuModule.service('localizationService', localizationService);
menuModule.service('settingsService', settingsService);
menuModule.service('generalFunctionsService', generalFunctionsService);
menuModule.service('loggingService', loggingService);

menuModule.controller('BrandController', BrandController);
menuModule.controller('CenterPanelController', CenterPanelController);
menuModule.controller('NavBarController', NavBarController);
menuModule.controller('EnvironmentMenuController', EnvironmentMenuController);
menuModule.controller('MenuController', MenuController);
menuModule.controller('ListViewMenuTemplateController', ListViewMenuTemplateController);
menuModule.controller('ListViewMenuController', ListViewMenuController);
menuModule.controller('DatePickerController', DatePickerController);
menuModule.controller('UserInfoController', UserInfoController);
menuModule.controller('HistoryController', HistoryController);
menuModule.controller('LeftPanelGroupsController', LeftPanelGroupsController);
menuModule.controller('AllMenuController', AllMenuController);
menuModule.controller('SearchController', SearchController);

menuModule.controller('ModalMostUsedOptionsCtrl', ModalMostUsedOptionsCtrl);
menuModule.controller('ModalMostUsedShowAllCtrl', ModalMostUsedShowAllCtrl);
menuModule.controller('ModalHistoryOptionsCtrl', ModalHistoryOptionsCtrl);
menuModule.controller('ModalLeftPanelGroupsOptionsCtrl', ModalLeftPanelGroupsOptionsCtrl);
menuModule.controller('ModalHistoryShowAllCtrl', ModalHistoryShowAllCtrl);
menuModule.controller('ModalUserInfosShowOptionsCtrl', ModalUserInfosShowOptionsCtrl);
menuModule.controller('ModalShowNotificationsCtrl', ModalShowNotificationsCtrl);


menuModule.controller('NotificationsController', NotificationsController);
menuModule.controller('OpenDocumentsController', OpenDocumentsController);
menuModule.controller('MostUsedController', MostUsedController);
menuModule.controller('FavoritesController', FavoritesController);


menuModule.config(routingProvider);

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


