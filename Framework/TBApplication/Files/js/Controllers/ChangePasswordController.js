//---------------------------------------------------------------------------------------------
function ChangePasswordController($scope, $location, $http, generalFunctionsService, localizationService, settingsService, loggingService) {

	$scope.pwd = "";
	$scope.pwd2 = "";
	$scope.settingsService = settingsService;
	$scope.changeEnabled = true;
	$scope.localizationService = localizationService;
	$scope.loggingService = loggingService;

	/*//---------------------------------------------------------------------------------------------
	$(document).keyup(function (e) {
		if ($scope.changeEnabled) {
			if (e.keyCode === 13) {
				$scope.changepwd();
			}
		}
	});*/
	//---------------------------------------------------------------------------------------------
	$scope.init = function (hideBackButton) {
	    $scope.hideBackButton = hideBackButton;
	    
	}
	//---------------------------------------------------------------------------------------------
	$scope.enableButton = function () {
		$scope.changeEnabled =
            ($scope.pwd != null && $scope.pwd2 != null && $scope.pwd==$scope.pwd2);

	};

    //siccome lo spazio è un carattere che la dropdown bootstrap gestisce in maniera speciale  io eseguo uno stop propagation per fare in modo che si possano inserire spazi nel cambio password e nel cambio login
    //---------------------------------------------------------------------------------------------
	$scope.keydown = function ($event)
	{
	    if ($event.which == 32) $event.stopPropagation();
	}
	
	//---------------------------------------------------------------------------------------------
	$scope.changepwd = function () {

		if (!$scope.changeEnabled) return;

		if ($scope.pwd != $scope.pwd2) {
			loggingService.showDiagnostic(localizationService.getLocalizedElement("PwdMatchError"));
			return;
		}


		var urlToRun = 'changePassword/?password=' + $scope.pwd;

		$http.post(urlToRun)
			.success(function (data, status, headers, config) {
				$scope.pwd == undefined;
				$scope.pwd2 == undefined;

				if (data.success)
				{
					$scope.loggingService.showDiagnostic(localizationService.getLocalizedElement("PwdChanged"));
				}
				else if (data.messages) {
					$scope.loggingService.showDiagnostic(data.messages);
				}
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});


	};


	//---------------------------------------------------------------------------------------------
	$scope.setPwd = function (pwd) {
		$scope.pwd = pwd;

	}

	//---------------------------------------------------------------------------------------------
	$scope.setPwd2 = function (pwd2) {
		$scope.pwd2 = pwd2;
	}

};



