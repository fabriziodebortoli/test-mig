function ChangeThemeController($scope, $http, $rootScope, $location, menuService, settingsService, generalFunctionsService, loggingService, localizationService) {
    $scope.menuService = menuService;
    $scope.localizationService = localizationService;
    $scope.settingsService = settingsService;
    $scope.loggingService = loggingService;

    $scope.themes = [];
   //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        $scope.getThemes();
    });


    //---------------------------------------------------------------------------------------------
    $scope.getThemes = function () {

        var urlToRun =  'getThemes/';
        generalFunctionsService.post(urlToRun)
            .success(function (data, status, headers, config) {
                $scope.themes = data.Themes.Theme;

                for (var i = 0; i < $scope.themes.length; i++) {
                    $scope.getThemeName($scope.themes[i]);
                }
            })
            .error(function (data, status, headers, config) {
                $scope.loggingService.handleError(urlToRun, status);
            });
    }

    //---------------------------------------------------------------------------------------------
    $scope.changeTheme = function (theme) {

        var urlToRun =  'changeThemes/?theme=' + theme.path;
        $http.post(urlToRun)
            .success(function (data, status, headers, config) {
                if (data.success) {
                    location.reload();
                }
                else if (data.message) {
                    $scope.loggingService.showDiagnostic(data.message);
                }

            })
            .error(function (data, status, headers, config) {
                $scope.loggingService.handleError(urlToRun, status);
            });
    }

    //---------------------------------------------------------------------------------------------
    $scope.getThemeName = function (theme) {
        var tempName = theme.path.split("\\").pop();
        theme.name = tempName.replace(/.theme/gi, "");
    }

}






