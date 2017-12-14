function HiddenTilesController($scope, $http, $sce, $uibModal, $rootScope, menuService, imageService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.hiddenTiles = [];
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;
    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        var promise = menuService.getMenuElements();
     
        promise.then(function (menu) {
            $scope.listenToHiddenTiles();
            $scope.hiddenTiles = $scope.getHiddenTiles(menu);
         });
    });

    //---------------------------------------------------------------------------------------------
    $scope.listenToHiddenTiles = function () {
        $rootScope.$on('hiddenTileAdded', function (event, menu, tile) { $scope.addToHiddenTiles(menu, tile); });
    };

    //---------------------------------------------------------------------------------------------
    $scope.getTileTooltip = function (tile) {
        tile.tileTooltip= $sce.trustAsHtml(
            $scope.localizationService.getLocalizedElement('ApplicationLabel') + ": " + tile.currentAppTitle + "<br/>" +
            $scope.localizationService.getLocalizedElement('ModuleLabel') + ": " + tile.currentGroupTitle + "<br/>" +
            $scope.localizationService.getLocalizedElement('MenuLabel') + ": " + tile.currentMenuTitle);
    };

    //---------------------------------------------------------------------------------------------
    $scope.getHiddenTiles = function (root) {

        var filtered = [];

        if (root.ApplicationMenu != undefined) {
            var appMenu = root.ApplicationMenu.AppMenu;
            this.findHiddenTilesInApplication(appMenu.Application, filtered);
        }

        if (root.EnvironmentMenu != undefined) {
            var envMenu = root.EnvironmentMenu.AppMenu;
            this.findHiddenTilesInApplication(envMenu.Application, filtered);
        }
       
        return filtered;
    };

    //---------------------------------------------------------------------------------------------
    $scope.findHiddenTilesInApplication = function (application, filtered) {

        var tempAppArray = generalFunctionsService.ToArray(application);
        for (var a = 0; a < tempAppArray.length; a++) {
            var allGroupsArray = generalFunctionsService.ToArray(tempAppArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {

                var allMenusArray = generalFunctionsService.ToArray(allGroupsArray[d].Menu);
                for (var m = 0; m < allMenusArray.length; m++) {

                    var allTiles = generalFunctionsService.ToArray(allMenusArray[m].Menu);
                    for (var t = 0; t < allTiles.length; t++) {
                        if (generalFunctionsService.parseBool(allTiles[t].hiddenTile) == true) {
                            allTiles[t].currentApp = tempAppArray[a].name;
                            allTiles[t].currentGroup = allGroupsArray[d].name;
                            allTiles[t].currentMenu = allMenusArray[m].name;

                            allTiles[t].currentAppTitle = tempAppArray[a].title;
                            allTiles[t].currentGroupTitle = allGroupsArray[d].title;
                            allTiles[t].currentMenuTitle = allMenusArray[m].title;

                            filtered.push(allTiles[t]);
                            menuService.HiddenTilesCount++;
                        }
                    }
                }
            }
        }
        return filtered;
    };

    //---------------------------------------------------------------------------------------------
    $scope.addToHiddenTiles = function (menu, tile) {

        var urlToRun =  'addToHiddenTiles/?application=' + menuService.selectedApplication.name + '&group=' + menuService.selectedGroup.name + '&menu=' + menu.name + '&tile=' + tile.name;

        $http.post(urlToRun)
			.success(function (data, status, headers, config) {

			    tile.currentApp = menuService.selectedApplication.name;
			    tile.currentGroup = menuService.selectedGroup.name;
			    tile.currentMenu = menu.name;

			    tile.currentAppTitle = menuService.selectedApplication.title;
			    tile.currentGroupTitle = menuService.selectedGroup.title;
			    tile.currentMenuTitle = menu.title;

			    $scope.addToHiddenTilesArray(tile);
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});
    };

    //---------------------------------------------------------------------------------------------
    $scope.removeFromHiddenTiles = function (tile) {
        var urlToRun =  'removeFromHiddenTiles/?application=' + tile.currentApp + '&group=' + tile.currentGroup + '&menu=' + tile.currentMenu + '&tile=' + tile.name;
        $http.post(urlToRun)
			.success(function (data, status, headers, config) {
			    $scope.removeFromHiddenTilesArray(tile);
			})
			.error(function (data, status, headers, config) {
				$scope.loggingService.handleError(urlToRun, status);
			});

    };

    //---------------------------------------------------------------------------------------------
    $scope.addToHiddenTilesArray = function (tile) {

        tile.hiddenTile = true;

        for (var i = 0; i < $scope.hiddenTiles.length; i++) {
            if ($scope.hiddenTiles[i] == tile) {
                return;
            }
        }

        $scope.hiddenTiles.push(tile);
        menuService.HiddenTilesCount++;
        
        $scope.showOthers();
    };

    //---------------------------------------------------------------------------------------------
    $scope.removeFromHiddenTilesArray = function (tile) {
        var index = -1;

        for (var i = 0; i < $scope.hiddenTiles.length; i++) {
            if ($scope.hiddenTiles[i] == tile) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            tile.hiddenTile = false;
            $scope.hiddenTiles.splice(index, 1);
            menuService.HiddenTilesCount--;
        }
    };

    /*controlla se ci sono dei tile nascosti nel menu corrente*/
    //---------------------------------------------------------------------------------------------
    $scope.ifMenuExistInHiddenTiles = function () {

        if (menuService.selectedMenu == undefined || menuService.selectedApplication == undefined)
            return false;

        for (var i = 0; i < $scope.hiddenTiles.length; i++) {
            if (($scope.hiddenTiles[i].currentMenuTitle == menuService.selectedMenu.title) && ($scope.hiddenTiles[i].currentAppTitle == menuService.selectedApplication.title))
                return true;
        }
        return false;
    }

    //---------------------------------------------------------------------------------------------
    $scope.ifOtherTilesAreHidden = function () {
        if (menuService.selectedMenu == undefined)
            return true;

        for (var i = 0; i < $scope.hiddenTiles.length; i++) {
            if ($scope.hiddenTiles[i].currentMenuTitle != menuService.selectedMenu.title)
                return true;
        }
        return false;
    }

    //---------------------------------------------------------------------------------------------

    $scope.showOthers = function () {
        var display = $(".othersHiddenContainer").css("display");
        if (display == 'none')
            $(".othersHiddenContainer").css("display", "block");

    }
    

    //---------------------------------------------------------------------------------------------

    $scope.hideOthers = function () {
        var display = $(".othersHiddenContainer").css("display");
        if (display == 'none')
            $(".othersHiddenContainer").css("display", "block");
        else
            $(".othersHiddenContainer").css("display", "none");

    }
}
