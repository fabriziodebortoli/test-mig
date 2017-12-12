function SchedulerNewTaskController(
    $scope, $log, $location, $rootScope, $http, $route, $uibModal, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;
    $scope.changePasswordVisible = false;
    $scope.appMenu = undefined;
    $scope.menu = undefined;
    $scope.hideAll = true;

    $scope.taskTypes = [
        { code: "1", description: "Batch" },
        { code: "2", description: "Report" }
    ];

    $scope.itemSelectCommands = [
        { action: "Menu" },
        { action: "Command" }
    ];

    $scope.originatorEv = undefined;

    $scope.openMenu = function ($mdOpenMenu, ev) {
        originatorEv = ev;
        $mdOpenMenu(ev);
    };

    $scope.announceClick = function (item) {
        alert(item);
        originatorEv = null;
    };

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {

        // UI hack
        $('.lBar').css('height', $(window).height());
        $('.hTab').css('min-height', $(window).height() - ($(window).height() / 3));
    });
}