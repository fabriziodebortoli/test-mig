function NewNavBarController($scope, $log, $location, $rootScope, $http, $route, $uibModal, imageService, menuService, settingsService, localizationService, generalFunctionsService, loggingService) {

    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.loggingService = loggingService;
    
    $scope.changePasswordVisible = false;
    $scope.appMenu = undefined;
    $scope.menu = undefined;
    $scope.hideAll = true;

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
            $scope.hideAll = false;
        });

        settingsService.setRelogin(true);

            
        //evento bootstrap dropdown hidden per gestire il ritorno alla visione iniziale quando si chiude la dropdown ( sempliecemente riporto le variabili che gestiscono ng-show delle dropdown a false)
            $('#userDropdown').on('hidden.bs.dropdown', function () {
                settingsService.changePasswordChangeVisibility1(false); settingsService.changeLoginChangeVisibility1(false);
               
            });

            $('#productDropdown').on('hidden.bs.dropdown', function () {
                $scope.changeViewConnInfo1(false); $scope.changeViewprodInfo1(false);

            });
         
            //$('#myDropdown').on('shown.bs.dropdown', function () {
            //    alert("Dropdown opened..");
            //});

            //$('#myDropdown').on('hide.bs.dropdown', function () {
            //    alert("Hiding dropdown..");
            //});

            //$('#myDropdown').on('hidden.bs.dropdown', function () {
            //    alert("Dropdown hidden..");
            //});
   
    });

    //---------------------------------------------------------------------------------------------
    $scope.$watch(function () { return $scope.smallSearchActivated; },
    function (newValue, oldValue) { }
    );

    //---------------------------------------------------------------------------------------------
    $scope.help = function (callback) {
        $http.post( 'showProductInfo/')
		.success(function (data, status, headers, config) {
		    callback();
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

    //---------------------------------------------------------------------------------------------
    $(window).resize(function () {
        var e = null;
        var icon = null;
        if ($(window).width() < 767) {
            e = document.getElementById('smallDimSearchBox');
            icon = document.getElementById('smallDimSearchIcon');
            if (e.style.display == 'inline-block' && icon.style.display == 'inline-block')
                return;
            else {
                e.style.display = 'none';
                icon.style.display = 'inline-block'
            }
        }
        else {
            e = document.getElementById('smallDimSearchBox');
            e.style.display = 'inline-block';
            e = document.getElementById('smallDimSearchIcon');
            e.style.display = 'none';
            e = document.getElementById('calendarIcon');
            e.style.display = 'inline-block';
            e = document.getElementById('userIcon');
            e.style.display = 'inline-block';
            e = document.getElementById('infoIcon');
            e.style.display = 'inline-block';
        }

        var c = null;
        if ($(window).width() < 1000) {
            c = document.getElementById('custContextBox');
            if (c != undefined)
                c.style.display = 'none';
        }
        else
        {
            c = document.getElementById('custContextBox');
            if (c != undefined)
                c.style.display = 'inline-block';
        }
    });

    //---------------------------------------------------------------------------------------------
    $('#smallDimSearchIcon').click(function (event) {

        e = document.getElementById('calendarIcon');
        if (e.style.display == 'inline-block' || e.style.display == '')
            e.style.display = 'none';
        else
            e.style.display = 'inline-block';

        e = document.getElementById('userIcon');
        if (e.style.display == 'inline-block' || e.style.display == '')
            e.style.display = 'none';
        else
            e.style.display = 'inline-block';

        e = document.getElementById('infoIcon');
        if (e.style.display == 'inline-block' || e.style.display == '')
            e.style.display = 'none';
        else
            e.style.display = 'inline-block';

        e = document.getElementById('smallDimSearchBox');
        if (e.style.display == 'inline-block')
            e.style.display = 'none';
        else
            e.style.display = 'inline-block';

    });

    //---------------------------------------------------------------------------------------------
    $scope.showSearch = function () {
        var e = null;
        if ($(window).width() < 767) {
            e = document.getElementById('smallDimSearchBox');
            e.style.display = 'none';
            e = document.getElementById('smallDimSearchIcon');
            e.style.display = 'inline-block';
        }
        else {
            e = document.getElementById('smallDimSearchBox');
            e.style.display = 'inline-block';
            e = document.getElementById('smallDimSearchIcon');
            e.style.display = 'none';
        }
    }

    //---------------------------------------------------------------------------------------------
    $scope.setDropdownWidth = function (id, count) {
        if (count == 0)
            return;

        if (id == ".moduleSelector" || id == "#infoIcon") {
            if (count < 4)
                $(id + " .dropdown-menu").css("width", count * 130 + 3 + "px");
            else
                $(id + " .dropdown-menu").css("width", 4 * 130 + 3 + "px");

            return;
        }

        if (id == ".applicationSelector") {
            if (count == 1)
                $(id + " .dropdown-menu").css("width", count * 130 + 3 + "px");
            else
                $(id + " .dropdown-menu").css("width", 2 * 130 + 3 + "px");

            return;
        }

    }

    //---------------------------------------------------------------------------------------------
    $scope.getApplications = function () {
        if ($scope.appMenu == undefined || $scope.appMenu == null)
            return;
        var apps = [];
        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            if (tempAppArray[i].name.toLowerCase() != menuService.tbsName.toLowerCase())
                apps.push(tempAppArray[i]);
        }
        return apps;
    }

    //---------------------------------------------------------------------------------------------
    $scope.showAppSelector = function () {
        if ($scope.appMenu == undefined || $scope.appMenu == null || menuService.selectedApplication == undefined)
            return false;
        var count = 0;

        if (menuService.selectedApplication.name == menuService.tbsName)
            return true;

        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < $scope.appMenu.Application.length; i++) {
            if (tempAppArray[i].name != menuService.tbsName)
                count++;
        }

        return count > 1;
    }

    //---------------------------------------------------------------------------------------------
    $scope.getApplicationsNumber = function () {
        if ($scope.appMenu == undefined || $scope.appMenu == null)
            return false;
        var count = 0;
        var tempAppArray = generalFunctionsService.ToArray($scope.appMenu.Application);
        for (var i = 0; i < tempAppArray.length; i++) {
            if (tempAppArray[i].name.toLowerCase() != menuService.tbsName.toLowerCase())
                count++;
        }

        return count;
    }

    $scope.options = undefined;
    //---------------------------------------------------------------------------------------------
    $scope.getOptionCount = function () {
        if ($scope.options == undefined) return 0;
        return $scope.options.length;
    }
    //---------------------------------------------------------------------------------------------
    $scope.getOptions = function () {
        if ($scope.options == undefined) {
           $scope.options = [];
           var option5 = { "name": "ViewProductInfo", "code": "4" };
           $scope.options.push(option5);

           var option7 = { "name": "ConnectionInfo", "code": "6" };
           $scope.options.push(option7);

           var option4 = { "name": "GotoProducerSite", "code": "3" };
           $scope.options.push(option4);

           var option3 = { "name": "ClearCachedData", "code": "2" };
           $scope.options.push(option3);

           var option1 = { "name": "ActivateViaSMS", "code": "0" };
           $scope.options.push(option1);

           var option2 = { "name": "ActivateViaInternet", "code": "1" };
           $scope.options.push(option2);

           //var option6 = { "name": "viewOldMsg", "code": "5" };
           //$scope.options.push(option6);

        }
        return $scope.options;
    }

    $scope.viewprodInfo = false;
    $scope.viewConnInfo = false;
    //---------------------------------------------------------------------------------------------
    $scope.changeViewConnInfo = function () { $scope.viewConnInfo = !$scope.viewConnInfo; }
    //---------------------------------------------------------------------------------------------
    $scope.changeViewprodInfo = function () { $scope.viewprodInfo = !$scope.viewprodInfo; }
    //---------------------------------------------------------------------------------------------
    $scope.changeViewConnInfo1 = function (val) { $scope.viewConnInfo = val; }
    //---------------------------------------------------------------------------------------------
    $scope.changeViewprodInfo1 = function (val) { $scope.viewprodInfo = val; }
    //---------------------------------------------------------------------------------------------
    $scope.HideInfo = function () { $scope.viewprodInfo = false; $scope.viewConnInfo = false; }
    //---------------------------------------------------------------------------------------------
    $scope.getOptionClick = function (option) {

        if (option == undefined)
            return;

        if (option.code == "0")
            $scope.activateViaSMS();

        if (option.code == "1")
            $scope.activateViaInternet();

        if (option.code == "2")
            menuService.clearCachedData();

        if (option.code == "3")
            $scope.gotosite();

        if (option.code == "4")
            $scope.changeViewprodInfo();

        if (option.code == "5")
            $scope.viewOldMsg();

        if (option.code == "6")
            $scope.changeViewConnInfo();
    }

    //---------------------------------------------------------------------------------------------
    $scope.activateViaSMS = function () {
        $http.post( 'activateViaSMS/')
		.success(function (data, status, headers, config) {
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.gotosite = function () {
        $http.post( 'producerSite/')
		.success(function (data, status, headers, config) {
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.activateViaInternet = function () {
        $http.post( 'activateViaInternet/')
		.success(function (data, status, headers, config) {
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.viewOldMsg = function () {
        $scope.loggingService.showDiagnostic("No messages.");

    }
   
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


$(document).ready(function () { 
     // action on key up
    $(document).keyup(function (e) {
        if (e.which == 17) {
            isCtrl = false;
        }
    });
    // action on key down
    $(document).keydown(function (e) {
        if (e.which == 17) {
            isCtrl = true;
        }
     
        if (e.which == 70 && isCtrl) {
            var el = document.getElementById('searchbox');
            el.focus();
        }
    });

});


