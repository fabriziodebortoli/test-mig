var settingsService = function ($http, $log, $q, $rootScope, generalFunctionsService, loggingService) {

    this.isRelogin = false;

    this.nrMaxItemsSearch = 20;
    this.showSearchBox = true;
    this.showFilterBox = true;
    this.showWorkerImage = false;

    this.passwordChangeVisibility = false;
    this.loginChangeVisibility = false;

    this.showListIcons = false;

    this.lastApplicationName = '';
    this.lastGroupName = '';
    this.lastMenuName = '';
    this.isEasyStudioActivated = false;

    var thiz = this;

    //---------------------------------------------------------------------------------------------
    this.getPreferences = function (callback) {
        var urlToRun = 'getPreferences/';
        $http.post(urlToRun)
		.success(function (data, status, headers, config) {
		    callback(data.Root);
		})
		.error(function (data, status, headers, config) {
		    this.loggingService.handleError(urlToRun, status);
		});
    }

    //---------------------------------------------------------------------------------------------
    this.setPreference = function (preferenceName, preferenceValue, callback) {
        var urlToRun = 'setPreference/?name=' + preferenceName + '&value=' + preferenceValue;
        $http.post(urlToRun)
       .success(function (data, status, headers, config) {
           if (callback != undefined)
               callback();
       })
       .error(function (data, status, headers, config) {
           this.loggingService.handleError(urlToRun, status);
       });
    }

    //---------------------------------------------------------------------------------------------
    this.setRememberMe = function (checked) {
        $http.post('setRememberMe/?checked=' + checked)
        .success(function (data, status, headers, config) {

        })

    }
    //---------------------------------------------------------------------------------------------
    this.isAutoLoginable = function () {
        if (rem != undefined)
            return false;

        var rem = $q.defer();

        $http.post('isAutoLoginable/')
        .success(function (data, status, headers, config) {
            if (data == "True")
                rem.resolve(true);
            else rem.resolve(false);
        })
        .error(function (data, status, headers, config) {
            rem.reject(false);
        });

        return rem.promise;
    }

    //---------------------------------------------------------------------------------------------
    this.getRememberMe = function () {
        if (rem != undefined)
            return false;

        var rem = $q.defer();

        $http.post('getRememberMe/')
        .success(function (data, status, headers, config) {
            if (data == "True")
                rem.resolve(true);
            else rem.resolve(false);
        })
        .error(function (data, status, headers, config) {
            rem.reject(false);
        });

        return rem.promise;
    }

    //---------------------------------------------------------------------------------------------
    this.getThemedSettings = function (callback) {
        var urlToRun = 'getThemedSettings/';
        $http.post(urlToRun)
        .success(function (data, status, headers, config) {
            callback(data);
        })
        .error(function (data, status, headers, config) {

            this.loggingService.handleError(urlToRun, status);
        });
    }

    //---------------------------------------------------------------------------------------------
    this.getPreferenceByName = function (preferences, name) {
        var preferencesArray = generalFunctionsService.ToArray(preferences);

        for (var i = 0; i < preferencesArray.length; i++) {

            var temp = preferencesArray[i];

            if (temp.name == name)
                return temp.value;
        }

        return undefined;
    }

    //---------------------------------------------------------------------------------------------
    this.changePasswordChangeVisibility = function () {

        thiz.passwordChangeVisibility = !thiz.passwordChangeVisibility;
    }
    //---------------------------------------------------------------------------------------------
    this.changePasswordChangeVisibility1 = function (val) {

        thiz.passwordChangeVisibility = val;
    }
    //---------------------------------------------------------------------------------------------
    this.getRelogin = function () {

        return thiz.isRelogin;
    }
    //---------------------------------------------------------------------------------------------
    this.setRelogin = function (val) {
        thiz.isRelogin = val;
    }

    //---------------------------------------------------------------------------------------------
    this.changeLoginChangeVisibility = function () {
        if (thiz.loginChangeVisibility)
            thiz.loginChangeVisibility = false;
        else thiz.loginChangeVisibility = true;

    }
    //---------------------------------------------------------------------------------------------
    this.changeLoginChangeVisibility1 = function (val) {

        thiz.loginChangeVisibility = val;

    }
    //---------------------------------------------------------------------------------------------
    this.getSettings = function () {
        this.getThemedSettings(
         function (data) {

             if (data.ThemedSettings.nrMaxItemsSearch != undefined)
                 thiz.nrMaxItemsSearch = parseInt(data.ThemedSettings.nrMaxItemsSearch);

             if (data.ThemedSettings.showSearchBox != undefined) {
                 thiz.showSearchBox = generalFunctionsService.parseBool(data.ThemedSettings.showSearchBox);
                 thiz.showFilterBox = !thiz.showSearchBox;
             }

             if (data.ThemedSettings.showWorkerImage != undefined)
                 thiz.showWorkerImage = generalFunctionsService.parseBool(data.ThemedSettings.showWorkerImage);

             if (data.OtherSettings != undefined && data.OtherSettings.isEasyStudioActivated != undefined)
                 thiz.isEasyStudioActivated = generalFunctionsService.parseBool(data.OtherSettings.isEasyStudioActivated);
         });

        //la get preference è inserita dentro la getSettings perchè alcuni dei settings di tema possono essere overraidati dalle preferenze dell'utente
        this.getPreferences(
             function (data) {
                 if (data != undefined) {
           
                     current = thiz.getPreferenceByName(data.Preference, "NrMaxItemsSearch");
                     if (current != undefined)
                         thiz.nrMaxItemsSearch = parseInt(current);

                     current = thiz.getPreferenceByName(data.Preference, "ShowSearchBox");
                     if (current != undefined) {
                         thiz.showSearchBox = generalFunctionsService.parseBool(current);
                         thiz.showFilterBox = !thiz.showSearchBox;
                     }

                     current = thiz.getPreferenceByName(data.Preference, "ShowListIcons");
                     if (current != undefined) {
                         thiz.showListIcons = generalFunctionsService.parseBool(current);
                     }

                     thiz.lastApplicationName = decodeURIComponent(thiz.getPreferenceByName(data.Preference, "LastApplicationName"));
                     thiz.lastGroupName = decodeURIComponent(thiz.getPreferenceByName(data.Preference, "LastGroupName"));
                     thiz.lastMenuName = decodeURIComponent(thiz.getPreferenceByName(data.Preference, "LastMenuName"));
                     thiz.leftGroupVisibility = generalFunctionsService.ToArray(data.LeftGroupVisibility);
                 }

                 $rootScope.$emit('preferencesLoaded');

             });
    };
};
