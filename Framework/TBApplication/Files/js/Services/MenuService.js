var menuService = function ($http, $log, $sce, $rootScope, $q, $location, $timeout, $route, localizationService, settingsService, generalFunctionsService, loggingService, imageService) {

    var deferredMenu = undefined;
    var deferredBrand = undefined;
    var thiz = this;

    this.HiddenTilesCount = 0;
    this.MostUsedCount = 0;
    this.FavoritesCount = 0;
    this.OpenDocumentsCount = 0;
    
    this.ifMoreAppsExist = false;
    this.selectedApplication = undefined;
    this.selectedGroup = undefined;
    this.selectedMenu = undefined;
    this.tbsName = 'TBS';
    this.tbfName = 'Framework';

    //---------------------------------------------------------------------------------------------
    this.emptyContextMenu = [];

    //---------------------------------------------------------------------------------------------
    this.getObjectContextMenu = function (object) {
        return this.objectContextMenu;
    }
 
    //---------------------------------------------------------------------------------------------
    this.getMenuElements = function () {


        if (deferredMenu != undefined)
            return deferredMenu.promise;

        deferredMenu = $q.defer();

        if ($rootScope.menu != undefined) {
            deferredMenu.resolve($rootScope.menu);
            return deferredMenu.promise;
        }

        generalFunctionsService.post( 'getMenuElements/')
		.success(function (data, status, headers, config) {
		    $rootScope.menu = data.Root;
		    thiz.postInitializeMenu();
		    deferredMenu.resolve($rootScope.menu);
		})
		.error(function (data, status, headers, config) {
		    deferredMenu.reject('error getMenuElements' + status);
		});

        return deferredMenu.promise;
    }

    //---------------------------------------------------------------------------------------------
    this.initMBF = function (applications) {
        var tempAppArray = generalFunctionsService.ToArray(applications);
        if (tempAppArray.length > 1)
            this.ifMoreAppsExist = true;

        var queryStringLastGroupName = generalFunctionsService.getGroupFromQueryString();
        if (queryStringLastGroupName != '')
            settingsService.lastGroupName = queryStringLastGroupName;

        var tempAppArray = generalFunctionsService.ToArray(applications);
        this.selectedApplication = tempAppArray[0];
        this.selectedApplication.isSelected = true;

        settingsService.lastApplicationName = tempAppArray[0].name;

        if (settingsService.lastGroupName != '') {
            var tempGroupArray = generalFunctionsService.ToArray(this.selectedApplication.Group);
            for (var i = 0; i < tempGroupArray.length; i++) {

                if (tempGroupArray[i].name.toLowerCase() == settingsService.lastGroupName.toLowerCase()) {

                    this.setSelectedGroup(tempGroupArray[i]);
                    this.selectedGroup.isSelected = true;
                    break;
                }
            }
        }

        if (this.selectedGroup == undefined)
            this.setSelectedGroup(tempGroupArray[0]);
    }

    //---------------------------------------------------------------------------------------------
    this.initApplicationAndGroup = function (applications) {

        var queryStringLastApplicationName = generalFunctionsService.getApplicationFromQueryString();
        if (queryStringLastApplicationName != '')
            settingsService.lastApplicationName = queryStringLastApplicationName;

        var tempAppArray = generalFunctionsService.ToArray(applications);
        this.ifMoreAppsExist = tempAppArray.length > 1;

        if (settingsService.lastApplicationName != '') {
            for (var i = 0; i < tempAppArray.length; i++) {
                if (tempAppArray[i].name.toLowerCase() == settingsService.lastApplicationName.toLowerCase()) {
                    this.selectedApplication = tempAppArray[i];
                    this.selectedApplication.isSelected = true;
                    settingsService.lastApplicationName = tempAppArray[i].name;
                    break;
                }
            }
        }

        if (this.selectedApplication == undefined)
            this.setSelectedApplication(tempAppArray[0]);

        if (settingsService.lastGroupName != '') {
            var tempGroupArray = generalFunctionsService.ToArray(this.selectedApplication.Group);
            for (var i = 0; i < tempGroupArray.length; i++) {
                if (tempGroupArray[i].name.toLowerCase() == settingsService.lastGroupName.toLowerCase()) {
                    this.selectedGroup = tempGroupArray[i];
                    this.selectedGroup.isSelected = true;
                    settingsService.lastGroupName = tempGroupArray[i].name;
                    break;
                }
            }
        }

        if (this.selectedGroup == undefined) {
            this.setSelectedGroup(tempGroupArray[0]);
            return;
        }

        $location.path("/MenuTemplate");
        $route.reload();
        return;
    }

    //---------------------------------------------------------------------------------------------
    this.setSelectedApplication = function (application) {
        if (this.selectedApplication != undefined && this.selectedApplication.title == application.title)
            return;

        if (this.selectedApplication != undefined)
            this.selectedApplication.isSelected = false;

        this.selectedApplication = application;
        this.selectedApplication.isSelected = true;

        settingsService.lastApplicationName = application.name;
        settingsService.setPreference('LastApplicationName', encodeURIComponent(settingsService.lastApplicationName));

        var tempGroupArray = generalFunctionsService.ToArray(this.selectedApplication.Group);
        if (tempGroupArray[0] != undefined)
            this.setSelectedGroup(tempGroupArray[0]);
    }

    //---------------------------------------------------------------------------------------------
    this.noImageOrDef = function (application) {
     
        if (application == undefined)
            return false;

        if (application.showTitleAlways)
            return true;

        if (application.image_file != undefined)
            return false;
        return true;
    }

    //TODOLUCA TODOILARIA, cablature da togliere
    //---------------------------------------------------------------------------------------------
    this.getApplicationIcon = function (application) {

        if (application == undefined) 
            return 'Images/Default.png';
         
        return imageService.getStaticImage(application);
    }

    //---------------------------------------------------------------------------------------------
    this.getOptionIconClass = function (option) {

        if (option.code == "0")
            return 'activateViaSmsImg';

        if (option.code == "1") 
            return 'activateViaInternetImg';

        if (option.code == "2")
            return 'refreshImg';

        if (option.code == "3")
            return 'domainImg';

        if (option.code == "4")
            return 'infoImg';

        if (option.code == "5")
            return 'mailImg';

        if (option.code == "6")
            return 'databaseConnectionImg';

        return ''
    }

    //---------------------------------------------------------------------------------------------
    this.getOptionIcon = function (option) {

        if (option.code == "0")
            return 'Images/ActivateViaSms32.png';

        if (option.code == "1")
            return 'Images/ActivateViaInternet32.png';

        if (option.code == "2")
            return 'Images/Refresh32.png';

        if (option.code == "3")
            return 'Images/Domain32.png';

        if (option.code == "4")
            return 'Images/Info32.png';

        if (option.code == "5")
            return 'Images/Mail32.png';

        if (option.code == "6")
            return 'Images/DatabaseConnection.png';

        return 'Images/default.png'
    }

    //---------------------------------------------------------------------------------------------
    this.getApplicationName = function (application) {
       
        if (application == undefined)
            return '';

        return application.title;
    }

    //---------------------------------------------------------------------------------------------
    this.setSelectedGroup = function (group) {

        if (this.selectedGroup != undefined && this.selectedGroup == group)
            return;

        if (this.selectedGroup != undefined)
            this.selectedGroup.isSelected = false;

        this.selectedGroup = group;
        this.selectedGroup.isSelected = true;
        settingsService.lastGroupName = group.name;
        settingsService.setPreference('LastGroupName', encodeURIComponent(settingsService.lastGroupName));

        var tempMenuArray = generalFunctionsService.ToArray(this.selectedGroup.Menu);
        if (tempMenuArray[0] != undefined)
            this.setSelectedMenu(tempMenuArray[0]);

        $location.path("/MenuTemplate");
        $route.reload();
    }

    //---------------------------------------------------------------------------------------------
    this.setSelectedMenu = function (menu) {

        if (this.selectedMenu != undefined && this.selectedMenu == menu)
            return;

        //deseleziono il vecchio se presente
        if (this.selectedMenu != undefined)
            this.selectedMenu.active = false;

        if (menu == undefined) {
            this.selectedMenu = '';
            settingsService.lastMenuName = '';
            settingsService.setPreference('LastMenuName', encodeURIComponent(settingsService.lastMenuName));
            return;
        }

        this.selectedMenu = menu;
        settingsService.lastMenuName = menu.name;
        settingsService.setPreference('LastMenuName', encodeURIComponent(settingsService.lastMenuName));
        this.selectedMenu.active = true;
        menu.visible = true
    }

    //---------------------------------------------------------------------------------------------
    this.backToSelectedApplication = function (application) {
        window.location.replace("newMenu.html?app=" + application.name);
    }



    //---------------------------------------------------------------------------------------------
    this.backToTBS = function (group) {
        window.location.replace("newMenu.html?group=" + group.name);
    }

    //---------------------------------------------------------------------------------------------
    this.objectContextMenu = [
            [
                function () { return localizationService.getLocalizedElement('Open') },
                function ($itemScope) { thiz.runFunction($itemScope.object); }
            ],
            null,
             [
                function ($itemScope) { return thiz.FavoritesTooltip($itemScope.object); },
                function ($itemScope) { thiz.toggleFavorites($itemScope.object); }
             ]
    ];

   
    //---------------------------------------------------------------------------------------------
    this.openStartingMenu = function () {
        if ($rootScope.startupGroup != undefined) {
            var tempGroup = $rootScope.startupGroup;
            tempGroup.isStartup = true;
            this.openGroupMenu(tempGroup, false);
            return;
        }
        $location.path('/FullMenu');
        $route.reload();
        return
    }

    //---------------------------------------------------------------------------------------------
    this.getFunctionToRun = function (object) {
        var ns = object.target;
        var objType = object.objectType.toLowerCase();
        var urlToRun = undefined;

        if (objType == 'document') {
            urlToRun =  'runDocument/?ns=' + encodeURIComponent(ns);
            if (object.arguments)
                urlToRun += "&args=" + encodeURIComponent(object.arguments);
        }
        else if (objType == 'batch')
            urlToRun =  'runDocument/?ns=' + encodeURIComponent(ns);
        else if (objType == 'report') {
            urlToRun =  'runReport/?ns=' + encodeURIComponent(ns);
            if (object.arguments)
                urlToRun += "&args=" + encodeURIComponent(object.arguments);
        }
        else if (objType == 'function') {
            var args = object.arguments;
            if (object.isUrl)
                urlToRun =  'runUrl/?url=' + encodeURIComponent(ns) + '&title=' + object.title;
            else
                urlToRun =  'runFunction/?ns=' + encodeURIComponent(ns) + '&args=' + encodeURIComponent(args);
        }
        else if (objType == 'officeitem') {
            var type = object.sub_type;
            var app = object.application;
            urlToRun =  'runOfficeItem/?ns=' + encodeURIComponent(ns) + '&subType=' + type + '&application=' + app;
        }
        return typeof (window.event) !== 'undefined' && window.event.ctrlKey ? urlToRun + "&notHooked=true" : urlToRun;

    }

    //---------------------------------------------------------------------------------------------
    this.runFunction = function (object) {

        if (object.isLoading == true || $rootScope.objectCurrentlyLoading != undefined)
            return;

        var urlToRun = this.getFunctionToRun(object);

        if ($rootScope.objectCurrentlyLoading != undefined)
            $rootScope.objectCurrentlyLoading.isLoading = false;

        $rootScope.objectCurrentlyLoading = object;
        object.isLoading = true;

        document.body.style.cursor = 'wait';


        generalFunctionsService.post(urlToRun)
        .success(function (data, status, headers, config) {
            if (data && data.windowUrl)
                window.open(data.windowUrl);
            $rootScope.$emit('runFunctionCompleted', object);
            
            var objType = object.objectType.toLowerCase();
            //if (objType == 'function' || objType == 'officeitem') {
                object.isLoading = false;
                $rootScope.objectCurrentlyLoading = undefined;
                document.body.style.cursor = 'default';

            //}
        })
        .error(function (data, status, headers, config) {
            loggingService.handleError(urlToRun, status);

        });

    };

    //---------------------------------------------------------------------------------------------
    this.openGroupMenu = function (currentGroup, placeBackButton) {
        $location.search('groupToShow', currentGroup);
        $location.search('placeBackButton', placeBackButton);
        $location.path("/SingleGroup");
        $route.reload();
    };


    //---------------------------------------------------------------------------------------------
    this.getTileTooltip = function (object) {
        return object.title;
    }

    //---------------------------------------------------------------------------------------------
    this.postInitializeMenu = function () {
        localizationService.loadLocalizedElements(true);

        settingsService.getSettings();

        $rootScope.objectCurrentlyLoading = undefined;

        this.listenXSocket();

    }

    //---------------------------------------------------------------------------------------------
    $rootScope.setStartupGroup = function (group) {
        $rootScope.startupGroup = group;
        $rootScope.$emit('startupGroupCompleted', group);
    }

    //---------------------------------------------------------------------------------------------
    this.getBrandInfo = function () {

        if (deferredBrand != undefined)
            return deferredBrand.promise;

        var deferredBrand = $q.defer();

        if ($rootScope.brandInfos != undefined) {
            deferredBrand.resolve($rootScope.brandInfos);
            return deferredBrand.promise;
        }

        $http.post( 'getBrandInfo/')
        .success(function (data, status, headers, config) {
            thiz.brandInfos = data.BrandInfos;
            deferredBrand.resolve($rootScope.brandInfos);
        })
        .error(function (data, status, headers, config) {
            deferredBrand.reject('error getBrandInfo' + status);
        });

        return deferredBrand.promise;
    }

    //---------------------------------------------------------------------------------------------
    this.openUrlLink = function (link) {

        var urlToRun =  'openUrlLink/?link=' + link;
        $http.post(urlToRun)
            .success(function (data, status, headers, config) {
            })
            .error(function (data, status, headers, config) {
                loggingService.handleError(urlToRun, status);
            });
    }

    //---------------------------------------------------------------------------------------------
    this.favoriteObject = function (object) {
        var urlToRun = 'favoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
        if (object.objectName != undefined)
            urlToRun += '&objectName=' + object.objectName;

        $http.post(urlToRun)
        .success(function (data, status, headers, config) {
        })
        .error(function (data, status, headers, config) {
            loggingService.handleError(urlToRun, status);
        });

    }

    //---------------------------------------------------------------------------------------------
    this.unFavoriteObject = function (object) {
        var urlToRun = 'unFavoriteObject/?target=' + object.target + '&objectType=' + object.objectType;
        if (object.objectName != undefined)
            urlToRun += '&objectName=' + object.objectName;

        $http.post(urlToRun)
        .success(function (data, status, headers, config) {
        })
        .error(function (data, status, headers, config) {
            loggingService.handleError(urlToRun, status);
        });
    }

    //---------------------------------------------------------------------------------------------
    this.toggleFavorites = function (object) {

        var isFavorite = object.isFavorite;
        if (object.isFavorite == undefined || !object.isFavorite) {
            this.favoriteObject(object);
            $rootScope.$emit('favoritesAdded', object);
        }
        else {
            this.unFavoriteObject(object);
            $rootScope.$emit('favoritesRemoved', object);
        }

        object.isFavorite = !isFavorite;
    }

    //---------------------------------------------------------------------------------------------
    this.getSearchItemTooltip = function (object) {
        return $sce.trustAsHtml(object.title + "<br/>" + object.applicationTitle + " | " + object.groupTitle + " | " + object.menu + " | " + object.tile);
    }

    //---------------------------------------------------------------------------------------------
    this.FavoritesTooltip = function (object) {
        if (object.isFavorite) {
            return localizationService.getLocalizedElement('RemoveFromFavorites');
        }
        else {
            return localizationService.getLocalizedElement('AddToFavorites');
        }
    }

    //---------------------------------------------------------------------------------------------
    this.hasMoreThanOneApplication = function (appMenu) {

        var appMenuArray = generalFunctionsService.ToArray(appMenu.Application);
        return appMenuArray.length > 1;
    }

    //---------------------------------------------------------------------------------------------
    this.getLimitedTitle = function (object) {

        var limit = 25;
        if (object.title.length > limit)
            return object.title.substring(0, limit) + '...'

        return object.title;
    }

    //---------------------------------------------------------------------------------------------
    this.hideTile = function (tile) {
        $rootScope.$emit('hiddenTileAdded', this.selectedMenu, tile);
    }

    //---------------------------------------------------------------------------------------------
    this.clearCachedData = function () {
        var urlToRun =  'clearCachedData/';
        $http.post(urlToRun)
      .success(function (data, status, headers, config) {
          location.reload();
      })
      .error(function (data, status, headers, config) {
          loggingService.handleError(urlToRun, status);
      });
    }

    //---------------------------------------------------------------------------------------------
    this.canChangeLogin = function () {
        $http.post( 'canChangeLogin/')
      .success(function (data, status, headers, config) {
      	if (data.success)
      		settingsService.changeLoginChangeVisibility();
      	else if (data.message)
      		loggingService.showDiagnostic(data.message.text);
      })
      .error(function (data, status, headers, config) {

      });
    }

    //---------------------------------------------------------------------------------------------
    this.toggleSelected = function (source, object) {

        var found = false;
        for (var i = 0; i < source.length; i++) {

            var current = source[i];

            if (object.record != undefined) {
                if (object.target == current.target && object.objectType == current.objectType && object.record == current.record) {
                    source.splice(i, 1);
                    found = true;
                    break;
                }
            }

            else {
                if (object.target == current.target && object.objectType == current.objectType) {

                    source.splice(i, 1);
                    found = true;
                    break;
                }
            }
        }

        if (!found)
            source.push(object);
    }

    //---------------------------------------------------------------------------------------------
    this.getFilteredSearch = function (viewValue, Item, searchInReport, searchInDocument, searchInBatch, startsWith) {
        var target = Item['target'].toLowerCase();
        var title = Item['title'].toLowerCase();
        var objectType = Item['objectType'].toLowerCase();
        var value = viewValue.toLowerCase();

        if (!searchInReport && objectType == "report")
            return false;

        if (!searchInDocument && objectType == "document")
            return false;

        if (!searchInBatch && objectType == "batch")
            return false;

        var found = false;
        if (!startsWith) {
            return title.indexOf(value) >= 0;
        }

        return found |= this.stringStartsWith(title, value);
    }

    //---------------------------------------------------------------------------------------------
    this.stringStartsWith = function (string, prefix) {
        return string.slice(0, prefix.length) == prefix;
    }

    //---------------------------------------------------------------------------------------------
    this.listenXSocket = function () {
        connection = new TBWebSocket(generalFunctionsService.getCookieByName('authtoken'));
        connection.on('DocumentListUpdated', function (response) {
            try {
                $rootScope.$apply(function () {
                    //var oData = d.data;
                    if ($rootScope.objectCurrentlyLoading == undefined)
                        return;

                    $rootScope.objectCurrentlyLoading.isLoading = false;
                    document.body.style.cursor = 'default';

                    $rootScope.objectCurrentlyLoading = undefined;
                });
            }
            catch (ex) {
                loggingService.showDiagnostic('error listenXSocket');
            }
        });
    }

};
