import { CookieService } from 'angular2-cookie/services/cookies.service';
import { Injectable } from '@angular/core';
import { UtilsService } from '../../core/services/utils.service';
import { Logger } from '../../core/services/logger.service';
import { EventManagerService } from './event-manager.service';
import { HttpMenuService } from './http-menu.service';
export class SettingsService {
    /**
     * @param {?} cookieService
     * @param {?} httpMenuService
     * @param {?} eventManagerService
     * @param {?} logger
     * @param {?} utilsService
     */
    constructor(cookieService, httpMenuService, eventManagerService, logger, utilsService) {
        this.cookieService = cookieService;
        this.httpMenuService = httpMenuService;
        this.eventManagerService = eventManagerService;
        this.logger = logger;
        this.utilsService = utilsService;
        this.isRelogin = false;
        this.nrMaxItemsSearch = 20;
        this.showSearchBox = true;
        this.showFilterBox = true;
        this.showWorkerImage = false;
        this.canEditDate = true;
        this.showListIcons = false;
        this._lastApplicationName = undefined;
        this._lastGroupName = undefined;
        this._lastMenuName = undefined;
        this.isEasyStudioActivated = false;
        this.logger.debug('SettingsService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    /**
     * @return {?}
     */
    get LastApplicationName() {
        if (this._lastApplicationName == undefined) {
            this._lastApplicationName = this.cookieService.get('_lastApplicationName');
        }
        return this._lastApplicationName;
    }
    /**
     * @param {?} val
     * @return {?}
     */
    set LastGroupName(val) {
        this._lastApplicationName = val;
        this.cookieService.put('_lastApplicationName', this._lastApplicationName);
    }
    /**
     * @return {?}
     */
    get LastGroupName() {
        if (this._lastGroupName == undefined) {
            this._lastGroupName = this.cookieService.get('_lastGroupName');
        }
        return this._lastGroupName;
    }
    /**
     * @param {?} val
     * @return {?}
     */
    set LastApplicationName(val) {
        this._lastGroupName = val;
        this.cookieService.put('_lastGroupName', this._lastGroupName);
    }
    /**
     * @return {?}
     */
    get LastMenuName() {
        if (this._lastMenuName == undefined) {
            this._lastMenuName = this.cookieService.get('_lastMenuName');
        }
        return this._lastMenuName;
    }
    /**
     * @param {?} val
     * @return {?}
     */
    set LastMenuName(val) {
        this._lastMenuName = val;
        this.cookieService.put('_lastMenuName', this._lastMenuName);
    }
    /**
     * @param {?} referenceName
     * @param {?} referenceValue
     * @param {?} callback
     * @return {?}
     */
    setPreference(referenceName, referenceValue, callback) {
        this.cookieService.put(referenceName, referenceValue);
    }
    /**
     * @return {?}
     */
    getThemedSettings() {
        let /** @type {?} */ sub = this.httpMenuService.getThemedSettings().subscribe(data => {
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
            sub.unsubscribe();
        });
    }
    /**
     * @return {?}
     */
    getPreferences() {
        let /** @type {?} */ subs = this.httpMenuService.getPreferences().subscribe(data => {
            var /** @type {?} */ current = this.getPreferenceByName(data.Root.Preference, "NrMaxItemsSearch");
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
            subs.unsubscribe();
        });
    }
    /**
     * @param {?} preferences
     * @param {?} name
     * @return {?}
     */
    getPreferenceByName(preferences, name) {
        var /** @type {?} */ preferencesArray = this.utilsService.toArray(preferences);
        for (var /** @type {?} */ i = 0; i < preferencesArray.length; i++) {
            var /** @type {?} */ temp = preferencesArray[i];
            if (temp.name == name)
                return temp.value;
        }
        return '';
    }
    /**
     * @return {?}
     */
    getSettings() {
        this.getThemedSettings();
        this.getPreferences();
        // $rootScope.$emit('preferencesLoaded');
    }
}
SettingsService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
SettingsService.ctorParameters = () => [
    { type: CookieService, },
    { type: HttpMenuService, },
    { type: EventManagerService, },
    { type: Logger, },
    { type: UtilsService, },
];
function SettingsService_tsickle_Closure_declarations() {
    /** @type {?} */
    SettingsService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    SettingsService.ctorParameters;
    /** @type {?} */
    SettingsService.prototype.isRelogin;
    /** @type {?} */
    SettingsService.prototype.nrMaxItemsSearch;
    /** @type {?} */
    SettingsService.prototype.showSearchBox;
    /** @type {?} */
    SettingsService.prototype.showFilterBox;
    /** @type {?} */
    SettingsService.prototype.showWorkerImage;
    /** @type {?} */
    SettingsService.prototype.canEditDate;
    /** @type {?} */
    SettingsService.prototype.showListIcons;
    /** @type {?} */
    SettingsService.prototype._lastApplicationName;
    /** @type {?} */
    SettingsService.prototype._lastGroupName;
    /** @type {?} */
    SettingsService.prototype._lastMenuName;
    /** @type {?} */
    SettingsService.prototype.isEasyStudioActivated;
    /** @type {?} */
    SettingsService.prototype.cookieService;
    /** @type {?} */
    SettingsService.prototype.httpMenuService;
    /** @type {?} */
    SettingsService.prototype.eventManagerService;
    /** @type {?} */
    SettingsService.prototype.logger;
    /** @type {?} */
    SettingsService.prototype.utilsService;
}
;
