import { Injectable } from '@angular/core';

import { UtilsService } from '@taskbuilder/core';
import { EventManagerService } from './event-manager.service';
import { HttpMenuService } from './http-menu.service';

import { Logger } from '@taskbuilder/core';

@Injectable()
export class SettingsService {

    public nrMaxItemsSearch: number = 20;
    public nrMaxFavorites: number = 10;
    public nrMaxMostUsed: number = 10;
    private showSearchBox: boolean = true;
    private canEditDate: boolean = true;

    private _lastApplicationName: string = undefined;
    private _lastGroupName: string = undefined;
    private _lastMenuName: string = undefined;

    constructor(
        private httpMenuService: HttpMenuService,
        private eventManagerService: EventManagerService,
        private logger: Logger,
        private utilsService: UtilsService
    ) {
        this.logger.debug('SettingsService instantiated - ' + Math.round(new Date().getTime() / 1000));
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

    getThemedSettings() {
        let sub = this.httpMenuService.getThemedSettings().subscribe(data => {

            if (data.ThemedSettings.nrMaxItemsSearch != undefined)
                this.nrMaxItemsSearch = parseInt(data.ThemedSettings.nrMaxItemsSearch);

            if (data.ThemedSettings.canEditDate != undefined)
                this.canEditDate = this.utilsService.parseBool(data.ThemedSettings.canEditDate);

            sub.unsubscribe()
        });
    }

    //---------------------------------------------------------------------------------------------
    getPreferences() {

        let subs = this.httpMenuService.getPreferences().subscribe(data => {
            if (data != undefined && data.Root != undefined) {

                var current = this.getPreferenceByName(data.Root.Preference, "NrMaxItemsSearch");
                if (current != undefined)
                    this.nrMaxItemsSearch = parseInt(current);

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