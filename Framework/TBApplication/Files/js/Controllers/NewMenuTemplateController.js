function NewMenuTemplateController($scope, $rootScope, $location, $log, $sce, imageService, menuService, localizationService, settingsService, generalFunctionsService, easyStudioService) {
    $scope.imageService = imageService;
    $scope.menuService = menuService;
    $scope.localizationService = localizationService;
    $scope.settingsService = settingsService;
    $scope.generalFunctionsService = generalFunctionsService;
    $scope.easyStudioService = easyStudioService;

    $scope.width = 0;
    
    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        $scope.initTab();
    });

    //---------------------------------------------------------------------------------------------
    $scope.initTab = function () {

        if (menuService.selectedGroup == undefined)
             return;

        var tempMenuArray = generalFunctionsService.ToArray(menuService.selectedGroup.Menu);

        var found = false;
        for (var i = 0; i < tempMenuArray.length; i++) {
            if (tempMenuArray[i].name.toLowerCase() == settingsService.lastMenuName.toLowerCase()) {
                menuService.setSelectedMenu(tempMenuArray[i]);
                return;
            }
        }

        if (!found) {
            menuService.setSelectedMenu(tempMenuArray[0]);
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.IfMenuHasObjects= function (menu) {
        if (menu == undefined)
            return false;

        var tempMenuArray = generalFunctionsService.ToArray(menu.Menu);
        for (var i = 0; i < tempMenuArray.length; i++) {
            var tempObjectArray = generalFunctionsService.ToArray(tempMenuArray[i].Object);
            if (tempObjectArray.length > 0)
                return true;
        }

        if (menu.Object == undefined)
            return false;

        tempMenuArray = generalFunctionsService.ToArray(menu.Object);
        return tempMenuArray.length > 0;
    }


    //---------------------------------------------------------------------------------------------
    $scope.ifTileHasObjects = function (tile) {
        if (tile == undefined || tile.Object == undefined)
            return false;
        var array = generalFunctionsService.ToArray(tile.Object);

        return array.length > 0;
    }

    //---------------------------------------------------------------------------------------------
    /*controllo altezza tile*/
    $scope.SetTileDim=function(){
        var tempTileArray = generalFunctionsService.ToArray(menuService.selectedMenu.Menu);
        if (tempTileArray.length == 0) {
            tempTileArray = generalFunctionsService.ToArray(menuService.selectedMenu);
            if (tempTileArray.length == 0)
                return;
        }
        var length = 0;
        for (var i = 0; i < tempTileArray.length; i++) {
            if (!tempTileArray[i].hiddenTile)
                length++;
        }
        if (length == 0)
            return;
       
        if ($(window).width() < 688) {
            $(".tileContent").css("height", "auto");       
            $(".tileContainer").css("width", "97%");
        }
        else if ($(window).width() >= 688 && $(window).width() < 1024){
            length <= 2 ? $(".tileContent").css("height", "auto") : $(".tileContent").css("height", 235);
            if (length == 1) {
                $(window).width() <767? $(".tileContainer").css("width", "97%") : $(".tileContainer").css("width", "70%");
            }
              
            else
                $(".tileContainer").css("width", "48%");
        }
        else if ($(window).width() >= 1024 && $(window).width() < 1368){
            length <= 3 ? $(".tileContent").css("height", "auto") : $(".tileContent").css("height", 235);
            if (length == 1)
                $(".tileContainer").css("width", "60%");
            else if (length == 2)
                $(".tileContainer").css("width", "48%");
            else
                $(".tileContainer").css("width", "32%");       
        }
        else if ($(window).width() >= 1368){
            length <= 4 ? $(".tileContent").css("height", "auto") : $(".tileContent").css("height", 235);
            if (length <= 3)
                $(".tileContainer").css("width", "32%");
            //else if (length <= 2)
            //    $(".tileContainer").css("width", "48%");
            else
                $(".tileContainer").css("width", "23%");
        }
    }

    $(window).resize(function () {
       $scope.SetTileDim();
    });

    $scope.firstWord = function (title) {
        var text = title.trim().split(" ");
        var first = text.shift();
        var ret = $sce.trustAsHtml( "<span class=\"firstWord\">" + first + "</span> "  + text.join(" "))
        
        return ret;
    };
}
