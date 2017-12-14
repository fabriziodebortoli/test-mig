function FavoritesController($scope, $log, $http, $rootScope, menuService, imageService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.menu = undefined;
    $scope.favorites = undefined;
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;
    $scope.favoritesIsOpened = true;
  
    var thiz = this;

    $scope.lastPosition = -1;

    //---------------------------------------------------------------------------------------------
    $scope.dropSuccessHandler = function ($event, originalPosition) {
        if ($scope.lastPosition == originalPosition) {
            originalPosition = -1;
            $scope.lastPosition = -1;
            return;
        }

        if (originalPosition < 0 || $scope.lastPosition < 0 || $scope.favorites[originalPosition] == undefined || $scope.favorites[$scope.lastPosition] == undefined)
            return;

        $scope.updateFavoritesPosition($scope.favorites[originalPosition], $scope.favorites[$scope.lastPosition]);
        $scope.swapArrayElements($scope.favorites, originalPosition, $scope.lastPosition);

        originalPosition = -1;
        $scope.lastPosition = -1;
    };


    //---------------------------------------------------------------------------------------------
    $scope.updateFavoritesPosition = function (object1, object2) {
        var urlToRun = 'updateFavoritesPosition/?target1=' + object1.target + '&objectType1=' + object1.objectType;
        if (object1.objectName != undefined)
            urlToRun += '&objectName1=' + object1.objectName;
        urlToRun += '&target2=' + object2.target + '&objectType2=' + object2.objectType;
        if (object1.objectName != undefined)
            urlToRun += '&objectName2=' + object2.objectName; $http.post(urlToRun)
        .success(function (data, status, headers, config) {
        })
        .error(function (data, status, headers, config) {
        	$scope.loggingService.handleError(urlToRun, status);
        });
    }


    //---------------------------------------------------------------------------------------------
    function compare(a, b) {
        if (a.position < b.position)
            return -1;
        if (a.position > b.position)
            return 1;
        return 0;
    }

    //---------------------------------------------------------------------------------------------
    $scope.onDrop = function ($event, $data, $index, object) {

        $scope.lastPosition = $index;
    };

    //---------------------------------------------------------------------------------------------
    $scope.swapArrayElements = function (a, from, to) {
        if (a.length === 1)
            return;

        a.splice(to, 0, a.splice(from, 1)[0]);
    };

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {

        $rootScope.$on('favoritesAdded', function (event, object) { $scope.addToFavoritesInternal(object); });
        $rootScope.$on('favoritesRemoved', function (event, object) { $scope.removeFromFavoritesInternal(object); });

        var promise = menuService.getMenuElements();

        promise.then(function (menu) {
            $scope.menu = menu;
            $scope.favorites = $scope.getFavoriteObjectsInternal(menu).sort(compare);

            if ($rootScope.favoritesIsOpened != undefined) {
                $scope.favoritesIsOpened = $rootScope.favoritesIsOpened;
            }
            else {
                $scope.favoritesIsOpened = $rootScope.favoritesIsOpened = true;
            }
        });
    });

    //---------------------------------------------------------------------------------------------
    $scope.addToFavoritesInternal = function (object)
    {
        object.isFavorite = true;
        object.isJustAdded = true;

        $scope.favorites.push(object);
        menuService.FavoritesCount++;
        object.position = $scope.favorites.length;
    }

    //---------------------------------------------------------------------------------------------
    $scope.removeFromFavoritesInternal = function (object)
    {
        object.isFavorite = false;
        object.isJustAdded = false;
        object.position = undefined;
        for (var i = 0; i < $scope.favorites.length; i++) {

            if ($scope.favorites[i].target == object.target && $scope.favorites[i].objectType == object.objectType &&
              (object.objectName == undefined || (object.objectName != undefined && $scope.favorites[i].objectName == object.objectName))
              ) {
                $scope.favorites.splice(i, 1);
                menuService.FavoritesCount--;
                return;
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.setFavoritesIsOpened = function () {

        $rootScope.favoritesIsOpened = $scope.favoritesIsOpened;
    }

    //---------------------------------------------------------------------------------------------
    $scope.rearrangePositions = function () {
        for (var a = 0; a < $scope.favorites.length; a++) {
            $scope.favorites[a].position = a;
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.getFavoriteObjects = function () {
        if ($rootScope.menu == undefined || $scope.isDragging)
            return $scope.favorites;

        $scope.favorites = $scope.getFavoriteObjectsInternal($rootScope.menu).sort(compare);
        return $scope.favorites;
    }

    //---------------------------------------------------------------------------------------------
    $scope.getFavoriteObjectsInternal = function (root) {
        var filtered = [];

        if (root.ApplicationMenu != undefined)
            this.findFavoritesInApplication(root.ApplicationMenu.AppMenu.Application, filtered);
        if (root.EnvironmentMenu != undefined)
            this.findFavoritesInApplication(root.EnvironmentMenu.AppMenu.Application, filtered);

        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    $scope.findFavoritesInApplication = function (application, filtered) {

        var tempMenuArray = generalFunctionsService.ToArray(application);
        for (var a = 0; a < tempMenuArray.length; a++) {
            var allGroupsArray = generalFunctionsService.ToArray(tempMenuArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {
                this.getFavoritesObjectsFromMenu(allGroupsArray[d], filtered);
            }
        }
        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    $scope.getFavoritesObjectsFromMenu = function (menu, filtered) {

        var allSubObjects = generalFunctionsService.ToArray(menu.Object);
        for (var i = 0; i < allSubObjects.length; i++) {

            if (allSubObjects[i].isFavorite) {
                allSubObjects[i].position = parseInt(allSubObjects[i].position);
                {
                    menuService.FavoritesCount++;
                    filtered.push(allSubObjects[i]);
                }
            }
        }

        var allSubMenus = generalFunctionsService.ToArray(menu.Menu);
        //cerca gli object dentro il menu
        for (var j = 0; j < allSubMenus.length; j++) {

            this.getFavoritesObjectsFromMenu(allSubMenus[j], filtered);
        }

        return filtered;
    };
};




