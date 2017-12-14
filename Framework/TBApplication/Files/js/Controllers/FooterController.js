function FooterController($scope, $http, $rootScope, menuService, imageService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.generalFunctionsService = generalFunctionsService;
    $scope.loggingService = loggingService;
    $scope.showFavorites = false;
    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
    });

    //---------------------------------------------------------------------------------------------
    $scope.getListIconLabel = function () {
        if (settingsService.showListIcons) {
            return localizationService.getLocalizedElement('ShowAsTile');
        }
        else
            return localizationService.getLocalizedElement('ShowAsTree');
    }
    //---------------------------------------------------------------------------------------------
    $scope.requestLoadFavorites = function () {
        $scope.showFavorites = true;
    }
    //---------------------------------------------------------------------------------------------
    $scope.setListIcons = function () {
        settingsService.showListIcons = !settingsService.showListIcons;

        settingsService.setPreference('ShowListIcons', settingsService.showListIcons == true ? 1 : 0);
    }
}