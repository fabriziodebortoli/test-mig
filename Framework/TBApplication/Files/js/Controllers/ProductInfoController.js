//---------------------------------------------------------------------------------------------
function ProductInfoController($rootScope, $scope, $location, $http, generalFunctionsService, menuService, localizationService, settingsService, loggingService) {

	$scope.settingsService = settingsService;
	$scope.localizationService = localizationService;
	$scope.loggingService = loggingService;
	$scope.menuService = menuService;

	$scope.productInfo = undefined;
	$scope.connectionInfo = undefined;
	$scope.showdbsize = false;

	
	//---------------------------------------------------------------------------------------------
	angular.element(document).ready(function () {
		$scope.getProductInfo(function (data) {
		    $scope.productInfo = data;
		});
		$scope.getConnectionInfo(function (data2) {
		    $scope.connectionInfo = data2;
		});
	});




	//---------------------------------------------------------------------------------------------
	$scope.getProductInfo = function (callback) {
		var urlToRun =  'getProductInfo/';
		$http.post(urlToRun)
			.success(function (data, status, headers, config) {
				callback(data.ProductInfos);
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});
	}
	//---------------------------------------------------------------------------------------------
	$scope.getConnectionInfo = function (callback) {
		var urlToRun =  'getConnectionInfo/';
		generalFunctionsService.post(urlToRun)
			.success(function (data, status, headers, config) {
				$scope.showdbsize = data.showdbsizecontrols == 'Yes';

				if (data.messages)
					$scope.loggingService.showDiagnostic(
						data.messages,
						{ onOk: function () { callback(data); } }
						);
				else
					callback(data);
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});

	}
};



