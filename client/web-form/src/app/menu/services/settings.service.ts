import { CookieService } from 'angular2-cookie/services/cookies.service';
import { Injectable } from '@angular/core';

import { UtilsService } from './../../core/utils.service';
import { EventManagerService } from './event-manager.service';
import { HttpMenuService } from './http-menu.service';

import { Logger } from './../../core/logger.service';

@Injectable()
export class SettingsService {

    private isRelogin: boolean = false;

    private nrMaxItemsSearch: number = 20;
    private showSearchBox: boolean = true;
    private showFilterBox: boolean = true;
    private showWorkerImage: boolean = false;
    private canEditDate: boolean = true;
    private showListIcons: boolean = false;

    private _lastApplicationName: string = undefined;
    private _lastGroupName: string = undefined;
    private _lastMenuName: string = undefined;

    public isEasyStudioActivated: boolean = false;

    constructor(
        private cookieService: CookieService,
        private httpMenuService: HttpMenuService,
        private eventManagerService: EventManagerService,
        private logger: Logger,
        private utilsService: UtilsService) {
        this.logger.debug('SettingsService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }


  //---------------------------------------------------------------------------------------------
    get LastApplicationName() {
        if (this._lastApplicationName == undefined) {
            this._lastApplicationName = this.cookieService.get('_lastApplicationName');
        }
        return this._lastApplicationName;
    }

    //---------------------------------------------------------------------------------------------
    set LastGroupName(val) {
        this._lastApplicationName = val;
        this.cookieService.put('_lastApplicationName', this._lastApplicationName);
    }

    //---------------------------------------------------------------------------------------------
    get LastGroupName() {
        if (this._lastGroupName == undefined) {
            this._lastGroupName = this.cookieService.get('_lastGroupName');
        }
        return this._lastGroupName;
    }

    //---------------------------------------------------------------------------------------------
    set LastApplicationName(val) {
        this._lastGroupName = val;
        this.cookieService.put('_lastGroupName', this._lastGroupName);
    }

    //---------------------------------------------------------------------------------------------
    get LastMenuName() {
        if (this._lastMenuName == undefined) {
            this._lastMenuName = this.cookieService.get('_lastMenuName');
        }
        return this._lastMenuName;
    }

    //---------------------------------------------------------------------------------------------
    set LastMenuName(val) {
        this._lastMenuName = val;
        this.cookieService.put('_lastMenuName', this._lastMenuName);
    }


    //---------------------------------------------------------------------------------------------
    setPreference(referenceName, referenceValue, callback) {

        this.cookieService.put(referenceName, referenceValue);
        //this.httpMenuService.setPreference(referenceName, referenceValue).subscribe(result => { if (callback != undefined) callback() });
    }

    getThemedSettings() {
        this.httpMenuService.getThemedSettings().subscribe(data => {

            if (data.ThemedSettings.nrMaxItemsSearch != undefined)
                this.nrMaxItemsSearch = parseInt(data.ThemedSettings.nrMaxItemsSearch);

            if (data.ThemedSettings.canEditDate != undefined)
                this.canEditDate = this.utilsService.parseBool(data.ThemedSettings.canEditDate);

            if (data.ThemedSettings.showSearchBox != undefined) {
                this.showSearchBox = this.utilsService.parseBool(data.ThemedSettings.showSearchBox);
                this.showFilterBox = !this.showSearchBox;
            }

            if (data.ThemedSettings.showWorkerImage != undefined)
                this.showWorkerImage = this.utilsService.parseBool(data.ThemedSettings.showWorkerImage);

            if (data.OtherSettings != undefined && data.OtherSettings.isEasyStudioActivated != undefined)
                this.isEasyStudioActivated = this.utilsService.parseBool(data.OtherSettings.isEasyStudioActivated);
        });
    }

    //---------------------------------------------------------------------------------------------
    getPreferences() {

        this.httpMenuService.getPreferences().subscribe(data => {

            var current = this.getPreferenceByName(data.Root.Preference, "NrMaxItemsSearch");
            if (current != undefined)
                this.nrMaxItemsSearch = parseInt(current);

            current = this.getPreferenceByName(data.Root.Preference, "ShowSearchBox");
            if (current != undefined) {
                this.showSearchBox = this.utilsService.parseBool(current);
                this.showFilterBox = !this.showSearchBox;
            }

            current = this.getPreferenceByName(data.Root.Preference, "ShowListIcons");
            if (current != undefined) {
                this.showListIcons = this.utilsService.parseBool(current);
            }

            this.eventManagerService.emitPreferenceLoaded();
        })
    }

    //---------------------------------------------------------------------------------------------
    getPreferenceByName(preferences, name) {
        var preferencesArray = this.utilsService.toArray(preferences);

        for (var i = 0; i < preferencesArray.length; i++) {

            var temp = preferencesArray[i];

            if (temp.name == name)
                return temp.value;
        }

        return '';
    }
    //---------------------------------------------------------------------------------------------
    getSettings() {

        this.getThemedSettings();
        this.getPreferences();
        // $rootScope.$emit('preferencesLoaded');
    }
};

// //---------------------------------------------------------------------------------------------
// this.setRememberMe = function (checked) {
//     $http.post('setRememberMe/?checked=' + checked)
//         .success(function (data, status, headers, config) {

//         })

// }
// //---------------------------------------------------------------------------------------------
// this.isAutoLoginable = function () {
//     if (rem != undefined)
//         return false;

//     var rem = $q.defer();

//     $http.post('isAutoLoginable/')
//         .success(function (data, status, headers, config) {
//             if (data == "True")
//                 rem.resolve(true);
//             else rem.resolve(false);
//         })
//         .error(function (data, status, headers, config) {
//             rem.reject(false);
//         });

//     return rem.promise;



// }

// //---------------------------------------------------------------------------------------------
// this.getRememberMe = function () {
//     if (rem != undefined)
//         return false;

//     var rem = $q.defer();

//     $http.post('getRememberMe/')
//         .success(function (data, status, headers, config) {
//             if (data == "True")
//                 rem.resolve(true);
//             else rem.resolve(false);
//         })
//         .error(function (data, status, headers, config) {
//             rem.reject(false);
//         });

//     return rem.promise;



// }
// //---------------------------------------------------------------------------------------------
// this.getThemedSettings = function (callback) {
//     var urlToRun = 'needLoginThread/getThemedSettings/';
//     $http.post(urlToRun)
//         .success(function (data, status, headers, config) {
//             callback(data);
//         })
//         .error(function (data, status, headers, config) {

//             this.loggingService.handleError(urlToRun, status);
//         });
// }


// //---------------------------------------------------------------------------------------------
// this.changePasswordChangeVisibility = function () {

//     this.passwordChangeVisibility = !this.passwordChangeVisibility;
// }
// //---------------------------------------------------------------------------------------------
// this.changePasswordChangeVisibility1 = function (val) {

//     this.passwordChangeVisibility = val;
// }
// //---------------------------------------------------------------------------------------------
// this.getRelogin = function () {

//     return this.isRelogin;
// }
// //---------------------------------------------------------------------------------------------
// this.setRelogin = function (val) {
//     this.isRelogin = val;
// }

// //---------------------------------------------------------------------------------------------
// this.changeLoginChangeVisibility = function () {
//     if (this.loginChangeVisibility)
//         this.loginChangeVisibility = false;
//     else this.loginChangeVisibility = true;

// }
// //---------------------------------------------------------------------------------------------
// this.changeLoginChangeVisibility1 = function (val) {

//     this.loginChangeVisibility = val;

// }
