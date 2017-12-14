function GenericDocumentToolbarController($scope, $log, $location, $rootScope, $http, $route, $uibModal, $mdSidenav, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;
    $scope.changePasswordVisible = false;
    $scope.appMenu = undefined;
    $scope.menu = undefined;
    $scope.hideAll = true;
    //$scope.toggleLeft = buildToggler('left');

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () { });

    $scope.close = function () {
        window.close();
    }

    $scope.openMenu = function () {
        $mdSidenav('left')
            .toggle();
    }

    //---------------------------------------------------------------------------------------------
    $scope.$watch(function () { return $scope.smallSearchActivated; },
    function (newValue, oldValue) {}
    );
}