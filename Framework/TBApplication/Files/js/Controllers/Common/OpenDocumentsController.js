function OpenDocumentsController($scope, $rootScope, $log, $http, menuService, imageService, settingsService, localizationService, generalFunctionsService, loggingService) {

	$scope.documents = [];
	$scope.menuService = menuService;
	$scope.imageService = imageService;
	$scope.localizationService = localizationService;
	$scope.generalFunctionsService = generalFunctionsService;
	$scope.loggingService = loggingService;

	$scope.documentCachedImages = [];

	//---------------------------------------------------------------------------------------------
	angular.element(document).ready(function () {
     
		$scope.getOpenDocuments(function (openDocuments) {
			$scope.documents = openDocuments.Document;
			menuService.OpenDocumentsCount = $scope.documents.length;
			$scope.applyImages($scope.documents);
			$scope.listenXSocket();
		});
	});

	//---------------------------------------------------------------------------------------------
	$scope.applyImages = function (documents) {
		for (var i = 0; i < documents.length; i++) {

			var object = $scope.findObjectInMenu(documents[i]);
			if (object == undefined)
				continue;

			if (object.image_file != undefined) {
				documents[i].image_file = object.image_file;
				continue;
			}
		}
	}

	//---------------------------------------------------------------------------------------------
	$scope.findObjectInMenu = function (object) {
		if (object == undefined)
			return undefined;

		var filtered = [];

		var cachedImageObject = $scope.findImageInCache(object);

		if (cachedImageObject != undefined)
			return cachedImageObject;

		if ($rootScope.menu.ApplicationMenu != undefined)
			$scope.findObjectInApplication($rootScope.menu.ApplicationMenu.AppMenu.Application, object, filtered);
		if ($rootScope.menu.EnvironmentMenu != undefined)
			$scope.findObjectInApplication($rootScope.menu.EnvironmentMenu.AppMenu.Application, object, filtered);

		return filtered[0];
	}

	//---------------------------------------------------------------------------------------------
	$scope.findImageInCache = function (object) {

		for (var i = 0; i < $scope.documentCachedImages.length; i++) {
			if (($scope.documentCachedImages[i].target == object.target && $scope.documentCachedImages[i].objectType == object.objectType)) {
				return $scope.documentCachedImages[i];
			}
		}

		return undefined;
	}

	//---------------------------------------------------------------------------------------------
	$scope.showThumbnails = function () {
		return settingsService.showThumbnails;
	}

	//---------------------------------------------------------------------------------------------
	$scope.getOpenDocuments = function (callback) {

		var urlToRun = menuService.getNeedLoginThreadUrl() + 'getOpenDocuments/';
		$http.post(urlToRun)
        .success(function (data, status, headers, config) {
        	callback(data.OpenDocuments);
        })
        .error(function (data, status, headers, config) {
        	$scope.loggingService.handleError(urlToRun, status);
        });
	}

	//---------------------------------------------------------------------------------------------
	$scope.activateDocument = function (document) {

		var urlToRun = menuService.getNeedLoginThreadUrl() + 'activateDocument/?handle=' + document.handle;
		$http.post(urlToRun)
        .success(function (data, status, headers, config) {
        })
        .error(function (data, status, headers, config) {
        	$scope.loggingService.handleError(urlToRun, status);
        });
	}

	//---------------------------------------------------------------------------------------------
	$scope.closeDocument = function (document) {

		var urlToRun = menuService.getNeedLoginThreadUrl() + 'closeDocument/?handle=' + document.handle;
		$http.post(urlToRun)
        .success(function (data, status, headers, config) {
        })
        .error(function (data, status, headers, config) {
        	$scope.loggingService.handleError(urlToRun, status);
        });
	}

	//---------------------------------------------------------------------------------------------
	$scope.getOpenDocumentRecord = function (openDoc) {

		if (openDoc['defaultDescription'] != undefined && openDoc['defaultDescription'] != '')
			return openDoc['defaultDescription'];

		if (openDoc['record'] != undefined && openDoc['record'] != '')
			return openDoc['record'];

		return '';
	}

	//---------------------------------------------------------------------------------------------
	$scope.getOpenDocumentTitle = function (openDoc) {
		return openDoc['title'];
	}

	//---------------------------------------------------------------------------------------------
	$scope.getDocuments = function () {
		$scope.getOpenDocuments(function (openDocuments) {
			$scope.documents = openDocuments.Document;
			$scope.applyImages($scope.documents);
		});
	}

	//---------------------------------------------------------------------------------------------
	$scope.removeDocument = function (handle) {
		if (handle == undefined || handle == '')
			return;
		var index = -1;
		for (var i = 0; i < $scope.documents.length; i++) {

			var temp = $scope.documents[i].handle;

			if (temp == handle) {
				index = i;
				break;
			}
		}

		if (index > -1)
			$scope.documents.splice(index, 1);
	}

	//---------------------------------------------------------------------------------------------
	$scope.listenXSocket = function () {
		controllerConnection = new TBWebSocket(generalFunctionsService.getCookieByName('authtoken'));
		controllerConnection.on("DocumentListUpdated", function (response) {
			try {
				//var oData = d.data;
				$scope.$apply(function () {
					if (response.data != undefined && response.data != '')
						$scope.removeDocument(response.data);
					else
						$scope.getDocuments();

					menuService.OpenDocumentsCount = $scope.documents.length;
				});
			}
			catch (ex) {
				$scope.loggingService.showDiagnostic(ex);
			}
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.findObjectInApplication = function (application, object, filtered) {

        var tempMenuArray = generalFunctionsService.ToArray(application);
        for (var a = 0; a < tempMenuArray.length; a++) {
            var allGroupsArray = generalFunctionsService.ToArray(tempMenuArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {
                $scope.findObjectInApplicationInternal(allGroupsArray[d], object, filtered);
            }
        }
        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    $scope.findObjectInApplicationInternal = function (menu, object, filtered) {

        var allSubObjects = generalFunctionsService.ToArray(menu.Object);
        for (var i = 0; i < allSubObjects.length; i++) {

            if (allSubObjects[i].target == object.target && allSubObjects[i].objectType == object.objectType) {
                filtered.push(allSubObjects[i]);
                $scope.documentCachedImages.push(allSubObjects[i]);
                break;
            }

            if ((object.target == undefined || object.target == '') && allSubObjects[i].title == object.title)
            {
                filtered.push(allSubObjects[i]);
                $scope.documentCachedImages.push(allSubObjects[i]);
                break;
            }
        }

        var allSubMenus = generalFunctionsService.ToArray(menu.Menu);
        //cerca gli object dentro il menu
        for (var j = 0; j < allSubMenus.length; j++) {

            $scope.findObjectInApplicationInternal(allSubMenus[j], object, filtered);
        }

        return filtered;
    };
}


