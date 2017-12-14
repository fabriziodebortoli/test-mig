function ESCloneDocumentController($scope, $log, $http, $rootScope, $uibModalInstance, easyStudioService, localizationService, settingsService) {

	$scope.localizationService = localizationService;
	$scope.easyStudioService = easyStudioService;
	$scope.settingsService = settingsService;
	$scope.object = $rootScope.objectCurrentlyLoading;
	$scope.title = $scope.localizationService.getLocalizedElement("Clone Document");
	$scope.subTitle = $scope.localizationService.getLocalizedElement("Create a new option from: ") + $scope.object.target;
	$scope.docName = "New ";
	$scope.docTitle = "New Title";
	
    //---------------------------------------------------------------------------------------------
	$scope.ok = function () {
	    $uibModalInstance.close();
	    easyStudioService.executeCloneDocument($scope.object, $scope.docName, $scope.docTitle);
	};

    //---------------------------------------------------------------------------------------------
	$scope.cancel = function () {
	    easyStudioService.cloneDocumentClear($scope.object);
	    $uibModalInstance.close();
	};
}
