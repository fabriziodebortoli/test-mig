function GenericDocumentSidebarController($scope, $log, $location, $rootScope, $http, $route, $uibModal, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;

    $scope.menu = [
        {
            link: '#Dashboard',
            title: 'Dashboard',
            icon: 'dashboard'
        },
        {
            link: '#NewTask',
            title: 'New Task',
            icon: 'group'
        },
        {
            link: '',
            title: 'Log',
            icon: 'message'
        }
    ];

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        // UI hack
        $('.lBar').css('height', $(window).height());
    });

    //---------------------------------------------------------------------------------------------
    $scope.$watch(function () { return $scope.smallSearchActivated; },
    function (newValue, oldValue) {}
    );
}