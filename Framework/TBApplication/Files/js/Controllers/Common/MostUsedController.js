function MostUsedController($scope, $http, $uibModal, $rootScope, menuService, imageService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.mostUsed = [];
    $scope.maxElementToShow = 8;
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        var promise = menuService.getMenuElements();

        promise.then(function (menu) {
            $scope.mostUsed = $scope.getMostUsedObjects(menu);
            $scope.listenToMostUsed();
        });

        $scope.getMostUsedShowNr(
		function (nrElements) {
		    if (nrElements != undefined && nrElements != '')
		        $scope.maxElementToShow = parseInt(nrElements);
		});
    });

    //---------------------------------------------------------------------------------------------
    $scope.getMostUsedObjects = function (root) {

        var filtered = [];
        if (root.ApplicationMenu != undefined)
        {
            var appMenu = root.ApplicationMenu.AppMenu;
            this.findMostUsedInApplication(appMenu.Application, filtered);
        }
        
        if (root.EnvironmentMenu != undefined) {
            var envMenu = root.EnvironmentMenu.AppMenu;
            this.findMostUsedInApplication(envMenu.Application, filtered);
        }

        return filtered;
    };


    //---------------------------------------------------------------------------------------------
    $scope.findMostUsedInApplication = function (application, filtered) {

        var tempMenuArray = generalFunctionsService.ToArray(application);
        for (var a = 0; a < tempMenuArray.length; a++) {
            var allGroupsArray = generalFunctionsService.ToArray(tempMenuArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {
                this.getMostUsedObjectsFromMenu(allGroupsArray[d], filtered);
            }
        }
        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    $scope.clearall = function () {

        var urlToRun =  'clearAllMostUsed/';
        $http.post(urlToRun)
			.success(function (data, status, headers, config) {
			    $scope.mostUsed.splice(0, $scope.mostUsed.length);
			    menuService.MostUsedCount = 0;
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});
    };

    //---------------------------------------------------------------------------------------------
    $scope.getMostUsedObjectsFromMenu = function (menu, filtered) {

        var allSubObjects = generalFunctionsService.ToArray(menu.Object);
        for (var i = 0; i < allSubObjects.length; i++) {

            if (allSubObjects[i].isMostUsed) {
                allSubObjects[i].lastModified = parseInt(allSubObjects[i].lastModified);
                {
                    filtered.push(allSubObjects[i]);
                    menuService.MostUsedCount++;
                }
            }
        }

        var allSubMenus = generalFunctionsService.ToArray(menu.Menu);
        //cerca gli object dentro il menu
        for (var j = 0; j < allSubMenus.length; j++) {

            $scope.getMostUsedObjectsFromMenu(allSubMenus[j], filtered);
        }

        return filtered;
    };

    //---------------------------------------------------------------------------------------------
    $scope.getMostUsedShowNr = function (callback) {

        var urlToRun =  'getMostUsedShowNr/';
        $http.post(urlToRun)
			.success(function (data, status, headers, config) {
			    callback(data);
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});
    }

    //---------------------------------------------------------------------------------------------
    $scope.listenToMostUsed = function () {
        $rootScope.$on('runFunctionCompleted', function (event, object) { $scope.addToMostUsed(object); });
    };

    //---------------------------------------------------------------------------------------------
    $scope.addToMostUsed = function (object) {
        var urlToRun = 'addToMostUsed/?target=' + object.target + '&objectType=' + object.objectType;
        if (object.objectName != undefined)
            urlToRun += '&objectName=' + object.objectName;

        $http.post(urlToRun)
			.success(function (data, status, headers, config) {
			    $scope.addToMostUsedArray(object);
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});

    };

    //---------------------------------------------------------------------------------------------
    $scope.removeFromMostUsed = function (object) {
        var urlToRun = 'removeFromMostUsed/?target=' + object.target + '&objectType=' + object.objectType;
        if (object.objectName != undefined)
            urlToRun += '&objectName=' + object.objectName;
        $http.post(urlToRun)
			.success(function (data, status, headers, config) {
			    $scope.removeFromMostUsedArray(object);
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});
    };

    //---------------------------------------------------------------------------------------------
    $scope.addToMostUsedArray = function (object) {

        var now = generalFunctionsService.getCurrentDate();
        for (var i = 0; i < $scope.mostUsed.length; i++) {

            if ($scope.mostUsed[i].target == object.target && $scope.mostUsed[i].objectType == object.objectType &&
               (object.objectName == undefined || (object.objectName != undefined && object.objectName == $scope.mostUsed[i].objectName))) {
                $scope.mostUsed[i].lastModified = now;
                return;
            }
        }

        object.isMostUsed = true;
        object.lastModified = now;
        $scope.mostUsed.push(object);
        menuService.MostUsedCount++;

    }

    //---------------------------------------------------------------------------------------------
    $scope.removeFromMostUsedArray = function (object) {
        var index = -1;

        for (var i = 0; i < $scope.mostUsed.length; i++) {
            if ($scope.mostUsed[i].target == object.target && $scope.mostUsed[i].objectType == object.objectType &&
                 (object.objectName == undefined || (object.objectName != undefined && $scope.mostUsed[i].objectName == object.objectName))) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            $scope.mostUsed.splice(index, 1);
            menuService.MostUsedCount--;
        }
    };


    //---------------------------------------------------------------------------------------------
    $scope.openOptions = function (size) {

        var modalInstance = $uibModal.open({
            templateUrl: 'templates/OldMenu/mostUsedShowOptions.html',
            controller: ModalMostUsedOptionsCtrl,
            size: size,
            resolve: {
                maxElementToShow: function () {
                    return $scope.maxElementToShow;
                },
                menuService: function () {
                    return menuService;
                },
            }
        });

        modalInstance.result.then(function (maxElementToShow) {
            if ($scope.maxElementToShow == maxElementToShow)
                return;

            $scope.maxElementToShow = maxElementToShow;
            var urlToRun =  'updateMostUsedShowNr/?nr=' + maxElementToShow;

            $http.post(urlToRun)
			.success(function (data, status, headers, config) {
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});
        });
    };

    //---------------------------------------------------------------------------------------------
    $scope.showAll = function () {
        var modalInstance = $uibModal.open(
		{
		    templateUrl: 'templates/OldMenu/mostUsedShowAll.html',
		    controller: ModalMostUsedShowAllCtrl,
		    resolve:
			 {
			     mostUsed: function () {
			         return $scope.mostUsed;
			     },
			     menuService: function () {
			         return menuService;
			     },
			     scope: function () {
			         return $scope;
			     },
			     imageService: function () {
			         return imageService;
			     }
			 }
		})
    };
}
