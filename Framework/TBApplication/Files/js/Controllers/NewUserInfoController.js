function NewUserInfoController($scope, $http, $modal, imageService, menuService, settingsService, localizationService, generalFunctionsService) {

    $scope.userInfos = undefined;
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.generalFunctionsService = generalFunctionsService;

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        $scope.getUserInfo();
    });

    //---------------------------------------------------------------------------------------------
    $scope.getUserInfo = function () {
        if ($scope.userInfos != undefined && $scope.userInfos.length > 0)
            return;

        $http.post('getUserInfo/')
		.success(function (data, status, headers, config) {
		    $scope.userInfos = data.UserInfos;
		})
		.error(function (data, status, headers, config) {
		    alert('error getUserInfo' + headers);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.getCompany = function () {

        if ($scope.userInfos == undefined)
            return;

        return $scope.userInfos.company;
    }

    //---------------------------------------------------------------------------------------------
    $scope.getWorkerDetails = function () {
        if ($scope.userInfos == undefined)
            return;

        return $scope.userInfos.workerName == undefined ? $scope.userInfos.workerLastName : $scope.userInfos.workerName + ' ' + $scope.userInfos.workerLastName;
    }

    //---------------------------------------------------------------------------------------------
    $scope.showOptions = function () {
        var modalInstance = $modal.open(
		{
		    templateUrl: 'templates/OldMenu/userInfosShowOptions.html',
		    controller: ModalUserInfosShowOptionsCtrl,
		    resolve:
			 {
			     scope: function () {
			         return $scope;
			     },
			     menuService: function () {
			         return menuService;
			     },
			     settingsService: function () {
			         return settingsService;
			     },
			     localizationService: function () {
			         return localizationService;
			     }
			 }
		})
    };

    //---------------------------------------------------------------------------------------------
    $scope.changeLogin = function () {
        $http.post('changeLogin/')
		.success(function (data, status, headers, config) {
		})
		.error(function (data, status, headers, config) {
		    alert('changeLogin' + status);
		});
    }
}

