function SchedulerDashboardController(
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
    $scope.tasks = {};
    $scope.taskProperties = {};

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {

        // load tasks
        $scope.tasks = $scope.getTasks();

        // ui tasks
        $('.lBar').css('height', $(window).height());
    });

    $scope.$on('schedulerStarted', function () {
        $scope.getTasks();
    });

    $scope.loadTaskProperties = function (code) {
        $http.post('getSchedulerTaskProperties/?taskCode=' + code)
        .success(function (data, status, header, config) {
            $scope.taskProperties = data;
        })
        .error(function (data, statues, header, config) {
            
        });
    };

    $scope.setTaskStatus = function (code, status) {
        $http.post('setTaskStatus/?taskCode=' + code + '&taskStatus=' + status)
       .success(function (data, status, headers, config) {
           
       })
       .error(function (data, status, headers, config) {
           deferredBrand.reject('error setting task status ' + status);
       });
    };

    $scope.getTasks = function () {
        $http.post('getTaskList/')
       .success(function (data, status, headers, config) {
           $scope.tasks = data;
       })
       .error(function (data, status, headers, config) {
           deferredBrand.reject('error reading tasks' + status);
       });
    };

    $scope.close = function () {
        window.close();
    };
}