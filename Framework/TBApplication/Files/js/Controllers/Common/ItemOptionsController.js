function ItemOptionsController($scope, $log, $http, menuService, localizationService, easyStudioService) {

    $scope.easyStudioService = easyStudioService;

    //---------------------------------------------------------------------------------------------
    $scope.init = function (object)
    {
        $scope.easyStudioService.getEasyStudioCustomizations(object);
    }

    //---------------------------------------------------------------------------------------------
    $scope.canShowMenu = function (object)
    {
        return $scope.easyStudioService.canShowEasyStudioButton(object);
    }
};