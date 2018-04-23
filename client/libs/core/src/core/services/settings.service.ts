import { EventManagerService } from './event-manager.service';
import { HttpService } from './http.service';
import { Injectable, EventEmitter } from '@angular/core';


import { UtilsService } from './../../core/services/utils.service';
import { Logger } from './../../core/services/logger.service';

@Injectable()
export class SettingsService {
    public _isEasyStudioActivated: boolean = undefined;

    public nrMaxFavorites: number = 20;
    public nrMaxMostUsed: number = 15;
    public showSearchBox: boolean = true;
    public canEditDate: boolean = true;

    public _lastApplicationName: string = undefined;
    public _lastGroupName: string = undefined;
    public _lastMenuName: string = undefined;

    public settingsPageOpenedEvent: EventEmitter<boolean> = new EventEmitter();

    constructor(
        public httpService: HttpService,
        public eventManagerService: EventManagerService,
        public logger: Logger,
        public utilsService: UtilsService
    ) {
        
    }

    //---------------------------------------------------------------------------------------------
    get IsEasyStudioActivated() {
        if (this._isEasyStudioActivated == undefined) {
            return false;
        }
        return this._isEasyStudioActivated;
    }

    //---------------------------------------------------------------------------------------------
    get LastApplicationName() {
        if (this._lastApplicationName == undefined) {
            this._lastApplicationName = localStorage.getItem('_lastApplicationName')
        }
        return this._lastApplicationName;
    }

    //---------------------------------------------------------------------------------------------
    set LastApplicationName(val) {
        this._lastApplicationName = val;
        localStorage.setItem('_lastApplicationName', this._lastApplicationName);
    }

    //---------------------------------------------------------------------------------------------
    get LastGroupName() {
        if (this._lastGroupName == undefined) {
            this._lastGroupName = localStorage.getItem('_lastGroupName');
        }
        return this._lastGroupName;
    }

    //---------------------------------------------------------------------------------------------
    set LastGroupName(val) {
        this._lastGroupName = val;
        localStorage.setItem('_lastGroupName', this._lastGroupName);
    }

    //---------------------------------------------------------------------------------------------
    get LastMenuName() {
        if (this._lastMenuName == undefined) {
            this._lastMenuName = localStorage.getItem('_lastMenuName');
        }
        return this._lastMenuName;
    }

    //---------------------------------------------------------------------------------------------
    set LastMenuName(val) {
        this._lastMenuName = val;
        localStorage.setItem('_lastMenuName', this._lastMenuName);
    }

    //---------------------------------------------------------------------------------------------    
    getThemedSettings() {
        let sub = this.httpService.getThemedSettings().subscribe(data => {
            if (data.ThemedSettings.canEditDate != undefined)
                this.canEditDate = this.utilsService.parseBool(data.ThemedSettings.canEditDate);

            if (data.OtherSettings != undefined && data.OtherSettings.isEasyStudioActivated != undefined)
                this._isEasyStudioActivated = this.utilsService.parseBool(data.OtherSettings.isEasyStudioActivated);

            sub.unsubscribe()
        });
    }

    //---------------------------------------------------------------------------------------------
    getPreferences() {

        let subs = this.httpService.getPreferences().subscribe(data => {
            if (data != undefined && data.Root != undefined) {

                this.eventManagerService.emitPreferenceLoaded();
                subs.unsubscribe();
            }
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