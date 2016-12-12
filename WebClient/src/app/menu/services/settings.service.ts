import { EventManagerService } from './event-manager.service';
import { HttpMenuService } from './http-menu.service';
import { UtilsService } from 'tb-core';
import { Logger } from 'libclient';
import { Injectable } from '@angular/core';

@Injectable()
export class SettingsService {

    private startupGroupName: string = undefined;
    private isHistoryVisible: boolean = true;
    private isMostUsedVisible: boolean = true;
    private showThumbnails: boolean = true;
    private isRelogin: boolean = false;

    private nrMaxItemsSearch: number = 20;
    private showFullMenu: boolean = true;
    private showSearchBox: boolean = true;
    private showFilterBox: boolean = true;
    private showWorkerImage: boolean = false;


    private canEditDate: boolean = true;
    private startFromLeftPanelGroups: boolean = false;

    private showHistoryOptions: boolean = false;
    private showMostUsedOptions: boolean = false;
    private leftGroupVisibility: Array<any> = [];

    private passwordChangeVisibility: boolean = false;
    private loginChangeVisibility: boolean = false;

    private showListIcons: boolean = false;

    public lastApplicationName: string = '';
    public lastGroupName: string = '';
    public lastMenuName: string = '';
    public isEasyStudioActivated: boolean = false;

    constructor(
        private httpMenuService: HttpMenuService,
        private eventManagerService: EventManagerService,
        private logger: Logger,
        private utilsService: UtilsService) {
        this.logger.debug('SettingsService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }


    //---------------------------------------------------------------------------------------------
    setPreference(referenceName, referenceValue, callback) {
        this.httpMenuService.setPreference(referenceName, referenceValue).subscribe(result => { if (callback != undefined) callback() });
    }

    getThemedSettings() {
        this.httpMenuService.getThemedSettings().subscribe(data => {

            if (data.ThemedSettings.showHistoryOptions != undefined)
                this.showHistoryOptions = this.utilsService.parseBool(data.ThemedSettings.showHistoryOptions);

            if (data.ThemedSettings.showMostUsedOptions != undefined)
                this.showMostUsedOptions = this.utilsService.parseBool(data.ThemedSettings.showMostUsedOptions);

            if (data.ThemedSettings.isHistoryVisible != undefined)
                this.isHistoryVisible = this.utilsService.parseBool(data.ThemedSettings.isHistoryVisible);

            if (data.ThemedSettings.isMostUsedVisible != undefined)
                this.isMostUsedVisible = this.utilsService.parseBool(data.ThemedSettings.isMostUsedVisible);

            if (data.ThemedSettings.showThumbnails != undefined)
                this.showThumbnails = this.utilsService.parseBool(data.ThemedSettings.showThumbnails);

            if (data.ThemedSettings.nrMaxItemsSearch != undefined)
                this.nrMaxItemsSearch = parseInt(data.ThemedSettings.nrMaxItemsSearch);

            if (data.ThemedSettings.showFullMenu != undefined)
                this.showFullMenu = this.utilsService.parseBool(data.ThemedSettings.showFullMenu);

            if (data.ThemedSettings.canEditDate != undefined)
                this.canEditDate = this.utilsService.parseBool(data.ThemedSettings.canEditDate);

            if (data.ThemedSettings.startFromLeftPanelGroups != undefined)
                this.startFromLeftPanelGroups = this.utilsService.parseBool(data.ThemedSettings.startFromLeftPanelGroups);

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
            this.startupGroupName = this.getPreferenceByName(data.Root.Preference, "StartupGroup");

            var current = this.getPreferenceByName(data.Root.Preference, "IsHistoryVisible");
            if (current != undefined)
                this.isHistoryVisible = this.utilsService.parseBool(current);

            current = this.getPreferenceByName(data.Root.Preference, "IsMostUsedVisible");
            if (current != undefined)
                this.isMostUsedVisible = this.utilsService.parseBool(current);

            current = this.getPreferenceByName(data.Root.Preference, "ShowFullMenu");
            if (current != undefined)
                this.showFullMenu = this.utilsService.parseBool(current);

            current = this.getPreferenceByName(data.Root.Preference, "ShowThumbnails");
            if (current != undefined)
                this.showThumbnails = this.utilsService.parseBool(current);

            current = this.getPreferenceByName(data.Root.Preference, "NrMaxItemsSearch");
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


            this.lastApplicationName = decodeURIComponent(this.getPreferenceByName(data.Root.Preference, "LastApplicationName"));
            this.lastGroupName = decodeURIComponent(this.getPreferenceByName(data.Root.Preference, "LastGroupName"));
            this.lastMenuName = decodeURIComponent(this.getPreferenceByName(data.Root.Preference, "LastMenuName"));
            this.leftGroupVisibility = this.utilsService.toArray(data.Root.LeftGroupVisibility);

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
// this.setLeftGroupVisibility = function (group) {
//     var urlToRun = 'needLoginThread/setLeftGroupVisibility/?name=' + group.name + '&visible=' + group.visible;
//     $http.post(urlToRun)
//         .success(function (data, status, headers, config) {
//         })
//         .error(function (data, status, headers, config) {

//             this.loggingService.handleError(urlToRun, status);
//         });
// }

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
