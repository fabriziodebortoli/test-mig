function TbfSidedropController($scope, $log, $location, $rootScope, $http, $route, $uibModal, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {

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

            if (menu.EnvironmentMenu == undefined)
                return;

            $scope.appMenu = menu.EnvironmentMenu.AppMenu;
        });

        $rootScope.$on('preferencesLoaded', function (event, object) {
            if ($scope.appMenu != undefined)
                menuService.initApplicationAndGroup($scope.appMenu.Application);
        });

        settingsService.setRelogin(true);
        $(".frameworkSideDropContainer").css('top', $(window).height() * 0.3 + 130 + 2 + 'px');

    });

    //---------------------------------------------------------------------------------------------
    $(window).resize(function () {

        if ($("#frameworkSideDrop").css('display') == 'block')
            $scope.setDropdownWidth("#frameworkSideDrop", $scope.getFrameworkMenu().length);
        $(".frameworkSideDropContainer").css('top',$(window).height() * 0.3 + 130 + 2 + 'px');
    });
  

    //---------------------------------------------------------------------------------------------
    $scope.setDropdownWidth = function (id, count) {
   
        if (id == "#frameworkSideDrop") {
            //if (count * 130 < $(window).width() * 0.4)
            if (count<=4)
                $(id).css("width", count * 130 + "px");
            else {
                var buttonsNum = 4; //Math.floor($(window).width() * 0.4 / 130);
                $(id).css("width", buttonsNum * 130 + "px");
            }
            return;
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.showFrameworkSidebar = function () {
        if ($scope.appMenu == undefined || $scope.appMenu == null || menuService.selectedApplication == undefined || menuService.selectedApplication.name.toLowerCase() == menuService.tbfName.toLowerCase())
            return false;

        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            var name = tempAppArray[i].name;
            var title = tempAppArray[i].title;
            if (tempAppArray[i].name.toLowerCase() == menuService.tbfName.toLowerCase()) {
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
    $scope.setFrameworkMenu = function (group) {
        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            if (tempAppArray[i].name.toLowerCase() == menuService.tbfName.toLowerCase()) {

                settingsService.lastApplicationName = tempAppArray[i].name;
                settingsService.lastGroupName = group.name;

                settingsService.setPreference('LastApplicationName', encodeURIComponent(settingsService.lastApplicationName), function ()
                {
                    settingsService.setPreference('LastGroupName', encodeURIComponent(settingsService.lastGroupName), function ()
                    {
                        window.location.replace("tbfMenu.html?group=" + group.name);
                    });
                });
                return;
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.getFrameworkMenu = function () {
        if ($scope.appMenu == undefined || $scope.appMenu == null)
            return;
        var groups = [];
        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            
            if (tempAppArray[i].name.toLowerCase() == menuService.tbfName.toLowerCase()) {
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
    $scope.hideShowFramework = function () {
        if ($('#frameworkSideDrop').css('display') == 'none')
            $('#frameworkSideDrop').css('display', 'block');
        else
            $('#frameworkSideDrop').css('display', 'none');
    }

    //---------------------------------------------------------------------------------------------
    $("body").click(function (event) {
        if (event.target.id != 'frameworkSideDrop' && event.target.id != 'frameworkSideButton' && event.target.id != 'frameworkSideButton2' && event.target.id != 'frameworkSideButton1' && $('#frameworkSideDrop').css('display') == 'block')
            $('#frameworkSideDrop').css('display', 'none');
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
