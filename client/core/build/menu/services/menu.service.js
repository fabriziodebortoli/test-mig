import { Injectable, EventEmitter, ComponentFactoryResolver, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ComponentService } from '../../core/services/component.service';
import { HttpService } from '../../core/services/http.service';
import { UtilsService } from '../../core/services/utils.service';
import { Logger } from '../../core/services/logger.service';
import { HttpMenuService } from './http-menu.service';
import { ImageService } from './image.service';
import { SettingsService } from './settings.service';
export class MenuService {
    /**
     * @param {?} httpService
     * @param {?} httpMenuService
     * @param {?} logger
     * @param {?} utilsService
     * @param {?} imageService
     * @param {?} settingsService
     * @param {?} router
     * @param {?} componentService
     * @param {?} resolver
     */
    constructor(httpService, httpMenuService, logger, utilsService, imageService, settingsService, router, componentService, resolver) {
        this.httpService = httpService;
        this.httpMenuService = httpMenuService;
        this.logger = logger;
        this.utilsService = utilsService;
        this.imageService = imageService;
        this.settingsService = settingsService;
        this.router = router;
        this.componentService = componentService;
        this.resolver = resolver;
        this.isMenuActivated = true;
        this.favoritesCount = 0;
        this.mostUsedCount = 0;
        this.favorites = [];
        this.mostUsed = [];
        this.searchSources = [];
        this.showDescription = false;
        this.selectedMenuChanged = new EventEmitter(true);
        this.selectedApplicationChanged = new EventEmitter(true);
        this.selectedGroupChanged = new EventEmitter(true);
        this.menuActivated = new EventEmitter();
        //---------------------------------------------------------------------------------------------
        this.runFunction = function (object) {
            if (object === undefined)
                return;
            if (object.objectType.toLowerCase() == 'report') {
                let /** @type {?} */ obs = this.httpService.runReport(object.target).subscribe((jsonObj) => {
                    if (!jsonObj.desktop) {
                        this.componentService.createReportComponent(object.target, true);
                    }
                    obs.unsubscribe();
                });
            }
            else {
                this.httpService.runDocument(object.target, object.args);
            }
            this.addToMostUsed(object);
            object.isLoading = true;
            const /** @type {?} */ subs1 = this.componentService.componentInfoCreated.subscribe(arg => {
                object.isLoading = false;
                subs1.unsubscribe();
            });
            const /** @type {?} */ subs2 = this.componentService.componentCreationError.subscribe(reason => {
                object.isLoading = false;
                subs2.unsubscribe();
            });
        };
        //---------------------------------------------------------------------------------------------
        this.getSearchItemTooltip = function (object) {
            // return $sce.trustAsHtml(object.title + "<br/>" + object.applicationTitle + " | " + object.groupTitle + " | " + object.menu + " | " + object.tile);
            return "ciao";
        };
        //---------------------------------------------------------------------------------------------
        this.removeFromMostUsed = function (object) {
            this.httpMenuService.removeFromMostUsed(object).subscribe(result => {
                this.removeFromMostUsedArray(object);
            });
        };
        this.logger.debug('MenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    /**
     * @return {?}
     */
    get selectedMenu() {
        return this._selectedMenu;
    }
    /**
     * @param {?} menu
     * @return {?}
     */
    set selectedMenu(menu) {
        this._selectedMenu = menu;
        if (menu != undefined) {
            this.settingsService.LastMenuName = menu.name;
        }
        this.selectedMenuChanged.emit();
    }
    /**
     * @return {?}
     */
    get selectedGroup() {
        return this._selectedGroup;
    }
    /**
     * @param {?} group
     * @return {?}
     */
    set selectedGroup(group) {
        this._selectedGroup = group;
        if (group != undefined) {
            this.settingsService.LastGroupName = group.name;
        }
        this.selectedGroupChanged.emit(group.title);
    }
    /**
     * @return {?}
     */
    get selectedApplication() {
        return this._selectedApplication;
    }
    /**
     * @param {?} application
     * @return {?}
     */
    set selectedApplication(application) {
        this._selectedApplication = application;
        if (application != undefined) {
            this.settingsService.LastApplicationName = application.name;
        }
        this.selectedApplicationChanged.emit();
    }
    /**
     * @param {?} applications
     * @return {?}
     */
    initApplicationAndGroup(applications) {
        var /** @type {?} */ queryStringLastApplicationName = this.utilsService.getApplicationFromQueryString();
        if (queryStringLastApplicationName != '')
            this.settingsService.LastApplicationName = queryStringLastApplicationName;
        var /** @type {?} */ tempAppArray = this.utilsService.toArray(applications);
        this.ifMoreAppsExist = tempAppArray.length > 1;
        if (this.settingsService.LastApplicationName != '' && this.settingsService.LastApplicationName != undefined) {
            for (var /** @type {?} */ i = 0; i < tempAppArray.length; i++) {
                if (tempAppArray[i].name.toLowerCase() == this.settingsService.LastApplicationName.toLowerCase()) {
                    this.selectedApplication = tempAppArray[i];
                    this.selectedApplication.isSelected = true;
                    this.settingsService.LastApplicationName = tempAppArray[i].name;
                    break;
                }
            }
        }
        if (this.selectedApplication == undefined)
            this.setSelectedApplication(tempAppArray[0]);
        if (this.settingsService.LastGroupName != '' && this.settingsService.LastGroupName != undefined) {
            var /** @type {?} */ tempGroupArray = this.utilsService.toArray(this.selectedApplication.Group);
            for (var /** @type {?} */ i = 0; i < tempGroupArray.length; i++) {
                if (tempGroupArray[i].name.toLowerCase() == this.settingsService.LastGroupName.toLowerCase()) {
                    this.selectedGroup = tempGroupArray[i];
                    this.selectedGroup.isSelected = true;
                    this.settingsService.LastGroupName = tempGroupArray[i].name;
                    break;
                }
            }
        }
        if (this.selectedGroup == undefined) {
            this.setSelectedGroup(tempGroupArray[0]);
            return;
        }
        // $location.path("/MenuTemplate");
        // $route.reload();
        return;
    }
    /**
     * @param {?} application
     * @return {?}
     */
    setSelectedApplication(application) {
        if (this.selectedApplication != undefined && this.selectedApplication.title == application.title)
            return;
        if (this.selectedApplication != undefined)
            this.selectedApplication.isSelected = false;
        this.selectedApplication = application;
        this.selectedApplication.isSelected = true;
        var /** @type {?} */ tempGroupArray = this.utilsService.toArray(this.selectedApplication.Group);
        if (tempGroupArray[0] != undefined)
            this.setSelectedGroup(tempGroupArray[0]);
    }
    /**
     * @param {?} group
     * @return {?}
     */
    setSelectedGroup(group) {
        if (this.selectedGroup != undefined && this.selectedGroup == group)
            return;
        if (this.selectedGroup != undefined)
            this.selectedGroup.isSelected = false;
        this.selectedGroup = group;
        this.selectedGroup.isSelected = true;
        var /** @type {?} */ tempMenuArray = this.utilsService.toArray(this.selectedGroup.Menu);
        if (tempMenuArray[0] != undefined)
            this.setSelectedMenu(tempMenuArray[0]);
        this.setSelectedMenu(undefined);
        // $location.path("/MenuTemplate");
        // $route.reload();
    }
    /**
     * @param {?} menu
     * @return {?}
     */
    setSelectedMenu(menu) {
        if (this.selectedMenu != undefined && this.selectedMenu == menu)
            return;
        if (menu == undefined) {
            this.selectedMenu = undefined;
            this.settingsService.LastMenuName = '';
            return;
        }
        this.selectedMenu = menu;
        this.selectedMenu.active = true;
        menu.visible = true;
        // this.eventData.model.Title.value = "Menu > " + menu.name;
    }
    /**
     * @param {?} application
     * @return {?}
     */
    getApplicationIcon(application) {
        return this.imageService.getStaticImage(application);
    }
    /**
     * @return {?}
     */
    clearMostUsed() {
        this.mostUsed.splice(0, this.mostUsed.length);
        this.mostUsedCount = 0;
    }
    /**
     * @return {?}
     */
    loadSearchObjects() {
        this.getSearchObjects();
    }
    /**
     * @return {?}
     */
    getSearchObjects() {
        if (this.applicationMenu != undefined) {
            this.findSearchesInApplication(this.applicationMenu.Application);
        }
        if (this.environmentMenu != undefined)
            this.findSearchesInApplication(this.environmentMenu.Application);
        this.searchSources = this.searchSources.sort(this.compareTitle);
    }
    /**
     * @param {?} application
     * @return {?}
     */
    findSearchesInApplication(application) {
        var /** @type {?} */ tempApplicationArray = this.utilsService.toArray(application);
        for (var /** @type {?} */ a = 0; a < tempApplicationArray.length; a++) {
            var /** @type {?} */ allGroupsArray = this.utilsService.toArray(tempApplicationArray[a].Group);
            for (var /** @type {?} */ d = 0; d < allGroupsArray.length; d++) {
                this.getSearchesObjectsFromMenu(allGroupsArray[d], tempApplicationArray[a].title, allGroupsArray[d].title, undefined, undefined);
            }
        }
    }
    /**
     * @param {?} menu
     * @param {?} applicationTitle
     * @param {?} groupTitle
     * @param {?} menuTitle
     * @param {?} tileTitle
     * @return {?}
     */
    getSearchesObjectsFromMenu(menu, applicationTitle, groupTitle, menuTitle, tileTitle) {
        var /** @type {?} */ allSubObjects = this.utilsService.toArray(menu.Object);
        if (allSubObjects != undefined) {
            for (var /** @type {?} */ i = 0; i < allSubObjects.length; i++) {
                var /** @type {?} */ temp = allSubObjects[i];
                if (this.containsSameSearch(this.searchSources, temp)) {
                    continue;
                }
                if (tileTitle != undefined)
                    allSubObjects[i].tile = tileTitle;
                if (menuTitle != undefined)
                    allSubObjects[i].menu = menuTitle;
                allSubObjects[i].groupTitle = groupTitle;
                allSubObjects[i].applicationTitle = applicationTitle;
                allSubObjects[i].itemTooltip = this.getSearchItemTooltip(allSubObjects[i]);
                this.searchSources.push(allSubObjects[i]);
            }
        }
        var /** @type {?} */ allSubMenus = this.utilsService.toArray(menu.Menu);
        if (allSubMenus != undefined) {
            //cerca gli object dentro il menu
            for (var /** @type {?} */ j = 0; j < allSubMenus.length; j++) {
                this.getSearchesObjectsFromMenu(allSubMenus[j], applicationTitle, groupTitle, menu.title, allSubMenus[j].title);
            }
        }
    }
    ;
    /**
     * @param {?} array
     * @param {?} obj
     * @return {?}
     */
    containsSameSearch(array, obj) {
        for (var /** @type {?} */ i = 0; i < array.length; i++) {
            var /** @type {?} */ temp = array[i];
            if (temp.target == obj.target && temp.objectType == obj.objectType && temp.title == obj.title) {
                return true;
            }
        }
        return false;
    }
    /**
     * @param {?} object
     * @return {?}
     */
    toggleFavorites(object) {
        var /** @type {?} */ isFavorite = object.isFavorite;
        if (object.isFavorite == undefined || !object.isFavorite) {
            object.isFavorite = true;
            this.httpMenuService.favoriteObject(object);
            this.addToFavoritesInternal(object);
            // $rootScope.$emit('favoritesAdded', object);
        }
        else {
            object.isFavorite = false;
            this.httpMenuService.unFavoriteObject(object);
            this.removeFromFavoritesInternal(object);
            // $rootScope.$emit('favoritesRemoved', object);
        }
        object.isFavorite = !isFavorite;
    }
    /**
     * @param {?} object
     * @return {?}
     */
    addToFavoritesInternal(object) {
        object.isFavorite = true;
        object.isJustAdded = true;
        this.favorites.push(object);
        this.favoritesCount++;
        object.position = this.favorites.length;
    }
    /**
     * @param {?} object
     * @return {?}
     */
    removeFromFavoritesInternal(object) {
        object.isFavorite = false;
        object.isJustAdded = false;
        object.position = undefined;
        for (var /** @type {?} */ i = 0; i < this.favorites.length; i++) {
            if (this.favorites[i].target == object.target && this.favorites[i].objectType == object.objectType &&
                (object.objectName == undefined || (object.objectName != undefined && this.favorites[i].objectName == object.objectName))) {
                this.favorites.splice(i, 1);
                this.favoritesCount--;
                return;
            }
        }
    }
    /**
     * @return {?}
     */
    setFavoritesIsOpened() {
        // $rootScope.favoritesIsOpened = this.favoritesIsOpened;
    }
    /**
     * @return {?}
     */
    rearrangePositions() {
        for (var /** @type {?} */ a = 0; a < this.favorites.length; a++) {
            this.favorites[a].position = a;
        }
    }
    /**
     * @param {?} a
     * @param {?} b
     * @return {?}
     */
    compareFavorites(a, b) {
        if (a.position < b.position)
            return -1;
        if (a.position > b.position)
            return 1;
        return 0;
    }
    /**
     * @param {?} a
     * @param {?} b
     * @return {?}
     */
    compareMostUsed(a, b) {
        if (a.lastModified < b.lastModified)
            return -1;
        if (a.lastModified > b.lastModified)
            return 1;
        return 0;
    }
    /**
     * @param {?} a
     * @param {?} b
     * @return {?}
     */
    compareTitle(a, b) {
        if (a.title < b.title)
            return -1;
        if (a.title > b.title)
            return 1;
        return 0;
    }
    /**
     * @param {?} root
     * @return {?}
     */
    onAfterGetMenuElements(root) {
        this.applicationMenu = root.ApplicationMenu.AppMenu;
        this.environmentMenu = root.EnvironmentMenu.AppMenu;
        this.loadFavoritesAndMostUsed();
        this.loadSearchObjects();
    }
    /**
     * @return {?}
     */
    loadFavoritesAndMostUsed() {
        if (this.applicationMenu != undefined)
            this.findFavoritesAndMostUsedInApplication(this.applicationMenu.Application);
        if (this.environmentMenu != undefined)
            this.findFavoritesAndMostUsedInApplication(this.environmentMenu.Application);
        this.favorites = this.favorites.sort(this.compareFavorites);
        this.mostUsed = this.mostUsed.sort(this.compareMostUsed);
    }
    /**
     * @param {?} application
     * @return {?}
     */
    findFavoritesAndMostUsedInApplication(application) {
        var /** @type {?} */ tempMenuArray = this.utilsService.toArray(application);
        for (var /** @type {?} */ a = 0; a < tempMenuArray.length; a++) {
            var /** @type {?} */ allGroupsArray = this.utilsService.toArray(tempMenuArray[a].Group);
            for (var /** @type {?} */ d = 0; d < allGroupsArray.length; d++) {
                this.getFavoritesAndMostUsedObjectsFromMenu(allGroupsArray[d]);
            }
        }
    }
    /**
     * @param {?} menu
     * @return {?}
     */
    getFavoritesAndMostUsedObjectsFromMenu(menu) {
        var /** @type {?} */ allSubObjects = this.utilsService.toArray(menu.Object);
        for (var /** @type {?} */ i = 0; i < allSubObjects.length; i++) {
            if (allSubObjects[i].isFavorite) {
                allSubObjects[i].position = parseInt(allSubObjects[i].position);
                this.favoritesCount++;
                this.favorites.push(allSubObjects[i]);
            }
            if (allSubObjects[i].isMostUsed) {
                allSubObjects[i].lastModified = parseInt(allSubObjects[i].lastModified);
                this.mostUsed.push(allSubObjects[i]);
                this.mostUsedCount++;
            }
        }
        var /** @type {?} */ allSubMenus = this.utilsService.toArray(menu.Menu);
        //cerca gli object dentro il menu
        for (var /** @type {?} */ j = 0; j < allSubMenus.length; j++) {
            this.getFavoritesAndMostUsedObjectsFromMenu(allSubMenus[j]);
        }
    }
    ;
    /**
     * @return {?}
     */
    getFavorites() {
        return this.favorites;
    }
    /**
     * @param {?} object
     * @return {?}
     */
    addToMostUsed(object) {
        this.httpMenuService.addToMostUsed(object).subscribe(result => {
            this.addToMostUsedArray(object);
        });
    }
    /**
     * @param {?} object
     * @return {?}
     */
    addToMostUsedArray(object) {
        var /** @type {?} */ now = this.utilsService.getCurrentDate();
        for (var /** @type {?} */ i = 0; i < this.mostUsed.length; i++) {
            if (this.mostUsed[i].target == object.target && this.mostUsed[i].objectType == object.objectType &&
                (object.objectName == undefined || (object.objectName != undefined && object.objectName == this.mostUsed[i].objectName))) {
                this.mostUsed[i].lastModified = now;
                return;
            }
        }
        object.isMostUsed = true;
        object.lastModified = now;
        this.mostUsed.push(object);
        this.mostUsedCount++;
    }
    /**
     * @param {?} object
     * @return {?}
     */
    removeFromMostUsedArray(object) {
        var /** @type {?} */ index = -1;
        for (var /** @type {?} */ i = 0; i < this.mostUsed.length; i++) {
            if (this.mostUsed[i].target == object.target && this.mostUsed[i].objectType == object.objectType &&
                (object.objectName == undefined || (object.objectName != undefined && this.mostUsed[i].objectName == object.objectName))) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            this.mostUsed.splice(index, 1);
            this.mostUsedCount--;
        }
    }
    ;
    /**
     * @param {?} viewValue
     * @param {?} Item
     * @param {?} searchInReport
     * @param {?} searchInDocument
     * @param {?} searchInBatch
     * @param {?} startsWith
     * @return {?}
     */
    getFilteredSearch(viewValue, Item, searchInReport, searchInDocument, searchInBatch, startsWith) {
        var /** @type {?} */ target = Item['target'].toLowerCase();
        var /** @type {?} */ title = Item['title'].toLowerCase();
        var /** @type {?} */ objectType = Item['objectType'].toLowerCase();
        var /** @type {?} */ value = viewValue.toLowerCase();
        if (!searchInReport && objectType == "report")
            return false;
        if (!searchInDocument && objectType == "document")
            return false;
        if (!searchInBatch && objectType == "batch")
            return false;
        let /** @type {?} */ found = false;
        if (!startsWith) {
            return title.indexOf(value) >= 0;
        }
        return found = found || this.stringStartsWith(title, value);
    }
    /**
     * @param {?} string
     * @param {?} prefix
     * @return {?}
     */
    stringStartsWith(string, prefix) {
        return string.slice(0, prefix.length) == prefix;
    }
    /**
     * @return {?}
     */
    toggleDecription() {
        this.showDescription = !this.showDescription;
    }
    /**
     * @return {?}
     */
    activateMenu() {
        this.isMenuActivated = true;
        this.menuActivated.emit();
    }
}
MenuService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
MenuService.ctorParameters = () => [
    { type: HttpService, },
    { type: HttpMenuService, },
    { type: Logger, },
    { type: UtilsService, },
    { type: ImageService, },
    { type: SettingsService, },
    { type: Router, },
    { type: ComponentService, },
    { type: ComponentFactoryResolver, },
];
MenuService.propDecorators = {
    'selectedMenu': [{ type: Input },],
    'selectedGroup': [{ type: Input },],
    'selectedApplication': [{ type: Input },],
};
function MenuService_tsickle_Closure_declarations() {
    /** @type {?} */
    MenuService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MenuService.ctorParameters;
    /** @type {?} */
    MenuService.propDecorators;
    /** @type {?} */
    MenuService.prototype._selectedApplication;
    /** @type {?} */
    MenuService.prototype._selectedGroup;
    /** @type {?} */
    MenuService.prototype._selectedMenu;
    /** @type {?} */
    MenuService.prototype.isMenuActivated;
    /** @type {?} */
    MenuService.prototype.applicationMenu;
    /** @type {?} */
    MenuService.prototype.environmentMenu;
    /** @type {?} */
    MenuService.prototype.favoritesCount;
    /** @type {?} */
    MenuService.prototype.mostUsedCount;
    /** @type {?} */
    MenuService.prototype.favorites;
    /** @type {?} */
    MenuService.prototype.mostUsed;
    /** @type {?} */
    MenuService.prototype.searchSources;
    /** @type {?} */
    MenuService.prototype.ifMoreAppsExist;
    /** @type {?} */
    MenuService.prototype.showDescription;
    /** @type {?} */
    MenuService.prototype.selectedMenuChanged;
    /** @type {?} */
    MenuService.prototype.selectedApplicationChanged;
    /** @type {?} */
    MenuService.prototype.selectedGroupChanged;
    /** @type {?} */
    MenuService.prototype.menuActivated;
    /** @type {?} */
    MenuService.prototype.runFunction;
    /** @type {?} */
    MenuService.prototype.getSearchItemTooltip;
    /** @type {?} */
    MenuService.prototype.removeFromMostUsed;
    /** @type {?} */
    MenuService.prototype.httpService;
    /** @type {?} */
    MenuService.prototype.httpMenuService;
    /** @type {?} */
    MenuService.prototype.logger;
    /** @type {?} */
    MenuService.prototype.utilsService;
    /** @type {?} */
    MenuService.prototype.imageService;
    /** @type {?} */
    MenuService.prototype.settingsService;
    /** @type {?} */
    MenuService.prototype.router;
    /** @type {?} */
    MenuService.prototype.componentService;
    /** @type {?} */
    MenuService.prototype.resolver;
}
