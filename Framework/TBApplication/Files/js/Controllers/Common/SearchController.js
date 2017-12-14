function SearchController($scope, $log, $sce, $rootScope, $http, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.searchSources = [];
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;

    $scope.searchInDocument = true;
    $scope.searchInReport = true;
    $scope.searchInBatch = true;
    $scope.startsWith = false;

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        var promise = menuService.getMenuElements();

        promise.then(function (menu) {
            $scope.searchSources = $scope.getSearchObjects(menu);
        });

        var brandPromise = menuService.getBrandInfo();

        brandPromise.then(function (brand) {
            $scope.brandInfos = brand;
        });
    });

    //---------------------------------------------------------------------------------------------
    $scope.setSearchInDocument = function () {
        $scope.searchInDocument = !$scope.searchInDocument;
    }

    //---------------------------------------------------------------------------------------------
    $scope.setSearchInReport = function () {
        $scope.searchInReport = !$scope.searchInReport;
    }

    //---------------------------------------------------------------------------------------------
    $scope.setSearchInBatch = function () {
        $scope.searchInBatch = !$scope.searchInBatch;
    }

    //---------------------------------------------------------------------------------------------
    $scope.setStartsWith = function () {
        $scope.startsWith = !$scope.startsWith;
    }

    //---------------------------------------------------------------------------------------------
    $scope.getSearchObjects = function (root) {
        var filtered = [];
        if (root.ApplicationMenu != undefined) {
            var appMenu = root.ApplicationMenu.AppMenu;
            $scope.findSearchesInApplication(appMenu.Application, filtered);
        }

        if (root.EnvironmentMenu == undefined)
            return filtered;

        var envMenu = root.EnvironmentMenu.AppMenu;
        $scope.findSearchesInApplication(envMenu.Application, filtered);

        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    $scope.findSearchesInApplication = function (application, filtered) {

        var tempApplicationArray = generalFunctionsService.ToArray(application);
        for (var a = 0; a < tempApplicationArray.length; a++) {
            var allGroupsArray = generalFunctionsService.ToArray(tempApplicationArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {
                $scope.getSearchesObjectsFromMenu(allGroupsArray[d], filtered, tempApplicationArray[a].title, allGroupsArray[d].title);
            }
        }
        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    $scope.getSearchesObjectsFromMenu = function (menu, filtered, applicationTitle, groupTitle, menuTitle, tileTitle) {

        var allSubObjects = generalFunctionsService.ToArray(menu.Object);
        if (allSubObjects != undefined) {

            for (var i = 0; i < allSubObjects.length; i++) {

                var temp = allSubObjects[i];
                if (containsSameSearch(filtered, temp)) {
                    continue;
                }

                if (tileTitle != undefined)
                    allSubObjects[i].tile = tileTitle;
                if (menuTitle != undefined)
                    allSubObjects[i].menu = menuTitle;

                allSubObjects[i].groupTitle = groupTitle;
                allSubObjects[i].applicationTitle = applicationTitle;
                
                allSubObjects[i].itemTooltip = $scope.menuService.getSearchItemTooltip(allSubObjects[i]);
                filtered.push(allSubObjects[i]);
            }
        }

        var allSubMenus = generalFunctionsService.ToArray(menu.Menu);
        if (allSubMenus != undefined) {

            //cerca gli object dentro il menu
            for (var j = 0; j < allSubMenus.length; j++) {

                $scope.getSearchesObjectsFromMenu(allSubMenus[j], filtered, applicationTitle, groupTitle, menu.title, allSubMenus[j].title);
            }
        }

        return filtered;
    };

    //---------------------------------------------------------------------------------------------
    function containsSameSearch(array, obj) {
        for (var i = 0; i < array.length; i++) {
            var temp = array[i];
            if (temp.target == obj.target && temp.objectType == obj.objectType && temp.title == obj.title) {
                return true;
            }
        }
        return false;
    }

    //---------------------------------------------------------------------------------------------
    $scope.onSelect = function ($item, $model, $label) {
        menuService.runFunction($item);
        $scope.nameFilter = undefined;
    }

    //---------------------------------------------------------------------------------------------
    $scope.searchFilter = function (viewValue) {
        return function (Item) {
            return menuService.getFilteredSearch(viewValue, Item, $scope.searchInReport, $scope.searchInDocument, $scope.searchInBatch, $scope.startsWith);
        }
    }
}




