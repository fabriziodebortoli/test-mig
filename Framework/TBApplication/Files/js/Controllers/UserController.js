//---------------------------------------------------------------------------------------------
function UserController($scope, $http, $sce, $log, imageService, menuService, localizationService, settingsService, loggingService) {

    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.autoLoginVisible = false;
    $scope.rememberMeChecked = false;
    $scope.usertooltip = undefined;
    $scope.cachedWorkerDetails = '';

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {

        var promise = menuService.getMenuElements();
        promise.then(function (menu) {
            $scope.getUserInfo();
        });

        var MYpromise = settingsService.getRememberMe();

        MYpromise.then(function (rem) {
            $scope.rememberMeChecked = rem;
        });

      
        var MYpromise2 = settingsService.isAutoLoginable();

        MYpromise2.then(function (rem2) {
            $scope.autoLoginVisible = rem2;
        });

    });

    //---------------------------------------------------------------------------------------------
    $scope.getUserInfo = function () {

        if ($scope.userInfos != undefined && $scope.userInfos.length > 0)
            return;

        $http.post( 'getUserInfo/')
		.success(function (data, status, headers, config) {
		    $scope.userInfos = data.UserInfos;
		    $scope.getWorkerDetails();
		    $scope.getUsertooltip();
		})
		.error(function (data, status, headers, config) {
		    $log.warn('error getUserInfo' + headers);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.getUsertooltip = function () {
        var a = localizationService.getLocalizedElement('WorkerLabel');
        var b = $scope.getWorkerDetails();
        if ($scope.userInfos != undefined) {
            var c = localizationService.getLocalizedElement('CompanyLabel');
            var d = $scope.userInfos.company;
        }

        $scope.usertooltip = $sce.trustAsHtml('<div>' + a + ": " + b + "</br>" + c + ": " + d + '</div>');
    }
 
    //---------------------------------------------------------------------------------------------
    $scope.getWorkerDetails = function () {
        if ($scope.userInfos == undefined)
            return;

        if ($scope.cachedWorkerDetails != '')
            return $scope.cachedWorkerDetails;

        var workerFullName = '';
        if ($scope.userInfos.workerName != undefined)
            workerFullName += $scope.userInfos.workerName + " ";

        if ($scope.userInfos.workerLastName != undefined)
            workerFullName += $scope.userInfos.workerLastName;

        return $scope.cachedWorkerDetails = workerFullName == undefined ? $scope.userInfos.userName : workerFullName;
    }

    //---------------------------------------------------------------------------------------------
    $scope.changeRememberMe = function () {

        $scope.rememberMeChecked = !$scope.rememberMeChecked;

        settingsService.setRememberMe($scope.rememberMeChecked);
    }

    //---------------------------------------------------------------------------------------------
    $scope.newLogin = function () {
        $http.post( 'newLogin/')
		.success(function (data, status, headers, config) {
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

   


};



