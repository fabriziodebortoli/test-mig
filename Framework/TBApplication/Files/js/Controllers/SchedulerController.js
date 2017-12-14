function SchedulerController($scope, $log, $location, $rootScope, $http, $route, $uibModal, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {


    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.generalFunctionsService = generalFunctionsService;
}