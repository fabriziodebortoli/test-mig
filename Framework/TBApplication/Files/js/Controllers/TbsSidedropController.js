function TbsSidedropController($scope, $log, $location, $rootScope, $http, $route, $uibModal, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;

    $scope.changePasswordVisible = false;
    $scope.appMenu = undefined;
    $scope.menu = undefined;


    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {
        var promise = menuService.getMenuElements();
        promise.then(function (menu) {
            $scope.menu = menu;

            if (menu.ApplicationMenu == undefined)
                return;

            $scope.appMenu = menu.ApplicationMenu.AppMenu;
        });

        $rootScope.$on('preferencesLoaded', function (event, object) {
            menuService.initApplicationAndGroup($scope.appMenu.Application);
        });

        settingsService.setRelogin(true);

    });

    //---------------------------------------------------------------------------------------------
    $(window).resize(function () {

        if ($("#studioSideDrop").css('display') == 'block')
            $scope.setDropdownWidth("#studioSideDrop", $scope.getStudioMenu().length);
    });
  

    //---------------------------------------------------------------------------------------------
    $scope.setDropdownWidth = function (id, count) {
   
        if (id == "#studioSideDrop") {
            //if (count * 130 < $(window).width() * 0.4)
            if (count<=4)
                $(id).css("width", count * 130 + "px");
            else {
                var buttonsNum = 4;// Math.floor($(window).width() * 0.4 / 130);
                $(id).css("width", buttonsNum * 130 + "px");
            }
            return;
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.showStudioSidebar = function () {
        if ($scope.appMenu == undefined || $scope.appMenu == null || menuService.selectedApplication == undefined || menuService.selectedApplication.name.toLowerCase() == menuService.tbsName.toLowerCase())
            return false;

        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            var name = tempAppArray[i].name;
            var title = tempAppArray[i].title;
            if (tempAppArray[i].name.toLowerCase() == menuService.tbsName.toLowerCase()) {
                var tempAppArrayGroup = generalFunctionsService.ToArray(tempAppArray[i].Group);
                for (var y = 0; y < tempAppArrayGroup.length; y++) {
                    if (tempAppArrayGroup[y] != undefined) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //---------------------------------------------------------------------------------------------
    $scope.setStudioMenu = function (group) {
        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            if (tempAppArray[i].name.toLowerCase() == menuService.tbsName.toLowerCase()) {

                settingsService.lastApplicationName = tempAppArray[i].name;
                settingsService.lastGroupName = group.name;

                settingsService.setPreference('LastApplicationName', encodeURIComponent(settingsService.lastApplicationName), function ()
                {
                    settingsService.setPreference('LastGroupName', encodeURIComponent(settingsService.lastGroupName), function ()
                    {
                        window.location.replace("newMenu.html?group=" + group.name);
                    });
                });
                return;
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.getStudioMenu = function () {
        if ($scope.appMenu == undefined || $scope.appMenu == null)
            return;
        var groups = [];
        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            
            if (tempAppArray[i].name.toLowerCase() == menuService.tbsName.toLowerCase()) {
                var tempAppArrayGroup = generalFunctionsService.ToArray(tempAppArray[i].Group);
                for (var y = 0; y < tempAppArrayGroup.length; y++) {                 
                    if (tempAppArrayGroup[y] != undefined)
                        groups.push(tempAppArrayGroup[y]);
                }
                return groups;
            }
        }
        return groups;
    }

    //---------------------------------------------------------------------------------------------
    $scope.hideShowStuido = function () {
        if ($('#studioSideDrop').css('display') == 'none')
            $('#studioSideDrop').css('display', 'block');
        else
            $('#studioSideDrop').css('display', 'none');
    }

    //---------------------------------------------------------------------------------------------
    $("body").click(function (event) {
        if (event.target.id != 'studioSideDrop' && event.target.id != 'studioSideButton' && event.target.id != 'studioSideButton1' && event.target.id != 'studioSideButton2' && $('#studioSideDrop').css('display') == 'block')
            $('#studioSideDrop').css('display', 'none');
    });
}

$(function () {
    /*
     * this swallows backspace keys on any non-input element.
     * stops backspace -> back
     */
    var rx = /INPUT|SELECT|TEXTAREA/i;

    $(document).bind("keydown keypress", function (e) {
        if (e.which == 8) { // 8 == backspace
            if (!rx.test(e.target.tagName) || e.target.disabled || e.target.readOnly) {
                e.preventDefault();
            }
        }
    });
});
