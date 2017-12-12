function ImageLoginController($scope, $http,generalFunctionsService, imageService, loggingService) {

    $scope.imageService = imageService;
    $scope.loginimagePath = undefined;
    $scope.loginBackgroundPath = undefined;
    $scope.loggingService = loggingService;

    //---------------------------------------------------------------------------------------------
    $scope.getLoginInitImage = function (callback) {
        var urlToRun = 'getLoginInitImage/';
        $http.post(urlToRun)
		.success(function (data, status, headers, config) {
		    callback(data);
		})
		.error(function (data, status, headers, config) {
			$scope.loggingService.handleError(urlToRun, status);
		});
    };

    //---------------------------------------------------------------------------------------------
    $scope.getLoginBackgroundImage = function (callback) {
        var urlToRun = 'getLoginBackgroundImage/';
        $http.post(urlToRun)
		.success(function (data, status, headers, config) {
		    callback(data);

		})
		.error(function (data, status, headers, config) {
		    $scope.loggingService.handleError(urlToRun, status);
		});
    };


    

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        $scope.getLoginInitImage(function (result) {
            $scope.loginimagePath = $scope.imageService.getStaticImage(result);
            
        });

        $scope.getLoginBackgroundImage(function (result) {
            $scope.loginBackgroundPath = $scope.imageService.getStaticImage(result);

        });

    });

};



