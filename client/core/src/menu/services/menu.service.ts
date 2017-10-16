import { LoadingService } from './../../core/services/loading.service';
import { Injectable, EventEmitter, ComponentFactoryResolver, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { InfoService } from './../../core/services/info.service';
import { HttpService } from './../../core/services/http.service';
import { ComponentService } from './../../core/services/component.service';
import { UtilsService } from './../../core/services/utils.service';
import { WebSocketService } from './../../core/services/websocket.service';
import { Logger } from './../../core/services/logger.service';
import { ImageService } from './image.service';
import { SettingsService } from './settings.service';
import { HttpMenuService } from './http-menu.service';

@Injectable()
export class MenuService {

    public isMenuCacheActive: boolean = true;
    public _selectedApplication: any;
    public _selectedGroup: any;
    public _selectedMenu: any;

    public favoritesCount: number = 0;
    public mostUsedCount: number = 0;

    public allMenus: Array<any> = [];

    public favorites = [];
    public mostUsed = [];

    public searchSources: Array<any> = [];
    public ifMoreAppsExist: boolean;

    public showDescription: boolean = false;
    public clearCachedData = false;
    get selectedMenu(): any {
        return this._selectedMenu;
    }

    set selectedMenu(menu: any) {
        this._selectedMenu = menu;
        if (menu != undefined) {
            this.settingsService.LastMenuName = menu.name;
        }
        this.selectedMenuChanged.emit();
    }

    get selectedGroup(): any {
        return this._selectedGroup;
    }

    set selectedGroup(group: any) {
        this._selectedGroup = group;
        if (group != undefined) {
            this.settingsService.LastGroupName = group.name;
        }

        this.selectedGroupChanged.emit(group.title);
    }

    get selectedApplication(): any {
        return this._selectedApplication;
    }

    set selectedApplication(application: any) {
        this._selectedApplication = application;
        if (application != undefined) {
            this.settingsService.LastApplicationName = application.name;
        }
        this.selectedMenuChanged.next(true);
    }

    selectedMenuChanged: EventEmitter<any> = new EventEmitter(true);
    selectedApplicationChanged: EventEmitter<any> = new EventEmitter(true);
    selectedGroupChanged: EventEmitter<string> = new EventEmitter(true);
    menuActivated: EventEmitter<any> = new EventEmitter();

    constructor(
        public httpService: HttpService,
        public webSocketService: WebSocketService,
        public httpMenuService: HttpMenuService,
        public logger: Logger,
        public utilsService: UtilsService,
        public imageService: ImageService,
        public settingsService: SettingsService,
        public componentService: ComponentService,
        public infoService: InfoService,
        public loadingService: LoadingService

    ) {
        this.logger.debug('MenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    //---------------------------------------------------------------------------------------------
    initApplicationAndGroup(applications) {

        var queryStringLastApplicationName = this.utilsService.getApplicationFromQueryString();
        if (queryStringLastApplicationName != '')
            this.settingsService.LastApplicationName = queryStringLastApplicationName;

        var tempAppArray = this.utilsService.toArray(applications);
        this.ifMoreAppsExist = applications.length > 1;

        if (this.settingsService.LastApplicationName != '' && this.settingsService.LastApplicationName != undefined) {
            for (var i = 0; i < tempAppArray.length; i++) {
                if (applications[i].name.toLowerCase() == this.settingsService.LastApplicationName.toLowerCase()) {
                    //this.setSelectedApplication(tempAppArray[i]);
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
            var tempGroupArray = this.utilsService.toArray(this.selectedApplication.Group);

            for (var i = 0; i < tempGroupArray.length; i++) {
                if (this.selectedApplication.Group[i].name.toLowerCase() == this.settingsService.LastGroupName.toLowerCase()) {

                    this.selectedGroup = tempGroupArray[i];
                    this.selectedGroup.isSelected = true;
                    this.settingsService.LastGroupName = tempGroupArray[i].name;
                    break;
                }
            }
        }

        if (this.selectedGroup == undefined) {
            this.setSelectedGroup(tempGroupArray[0]);
        }

        if (this.selectedGroup == undefined) {
            return;
        }

        let tempMenuArray = this.utilsService.toArray(this.selectedGroup.Menu);
        let found = false;
        for (let i = 0; i < tempMenuArray.length; i++) {
            if (tempMenuArray[i].name.toLowerCase() == this.settingsService.LastMenuName.toLowerCase()) {
                this.setSelectedMenu(tempMenuArray[i]);
                return;
            }
        }

        if (!found) {
            this.setSelectedMenu(tempMenuArray[0]);
        }
    }

    //---------------------------------------------------------------------------------------------
    setSelectedApplication(application) {
        if (this.selectedApplication != undefined && this.selectedApplication.title == application.title)
            return;

        if (this.selectedApplication != undefined)
            this.selectedApplication.isSelected = false;

        this.selectedApplication = application;
        this.selectedApplication.isSelected = true;

        var tempGroupArray = this.utilsService.toArray(this.selectedApplication.Group);

        if (tempGroupArray[0] != undefined)
            this.setSelectedGroup(tempGroupArray[0]);
    }

    //---------------------------------------------------------------------------------------------
    setSelectedGroup(group) {
        if (this.selectedGroup != undefined && this.selectedGroup == group)
            return;

        if (this.selectedGroup != undefined)
            this.selectedGroup.isSelected = false;

        this.selectedGroup = group;
        this.selectedGroup.isSelected = true;

        var tempMenuArray = this.utilsService.toArray(this.selectedGroup.Menu);

        if (tempMenuArray[0] != undefined)
            this.setSelectedMenu(tempMenuArray[0]);
    }

    //---------------------------------------------------------------------------------------------
    setSelectedMenu(menu) {
        if (this.selectedMenu != undefined && this.selectedMenu == menu && this.selectedMenu.active == true && this.selectedMenu.visible == true)
            return;

        if (menu == undefined) {
            this.selectedMenu = undefined;
            this.settingsService.LastMenuName = '';
            return;
        }

        this.selectedMenu = menu;
        this.selectedMenu.active = true;
        this.selectedMenu.visible = true;
    }

    //---------------------------------------------------------------------------------------------
    runFunction = function (object) {
        if (object === undefined)
            return;

        if (this.infoService.isDesktop) {
            this.runObject(object);
        }
        else {
            if (object.objectType.toLowerCase() == 'report') {
                this.componentService.createReportComponent(object.target, true);
            }
            else {
                this.webSocketService.runDocument(object.target, object.args)
                    .catch(() => { object.isLoading = false; });
            }
        }
        this.addToMostUsed(object);
        object.isLoading = true;
        const subs1 = this.componentService.componentInfoCreated.subscribe(arg => {
            object.isLoading = false;
            subs1.unsubscribe();
        });
        const subs2 = this.componentService.componentCreationError.subscribe(reason => {
            object.isLoading = false;
            subs2.unsubscribe();
        });
    }

    runObject(object: any) {
        let urlToRun = "";
        let objType = object.objectType.toLowerCase();
        let ns = object.target.toLowerCase();
        if (objType == 'document') {
            urlToRun = 'runDocument/?ns=' + encodeURIComponent(ns);
            if (object.arguments)
                urlToRun += "&args=" + encodeURIComponent(object.arguments);
        }
        else if (objType == 'batch')
            urlToRun = 'runDocument/?ns=' + encodeURIComponent(ns);
        else if (objType == 'report') {
            urlToRun = 'runReport/?ns=' + encodeURIComponent(ns);
            if (object.arguments)
                urlToRun += "&args=" + encodeURIComponent(object.arguments);
        }
        else if (objType == 'function') {
            var args = object.arguments;
            if (object.isUrl)
                urlToRun = 'runUrl/?url=' + encodeURIComponent(ns) + '&title=' + object.title;
            else
                urlToRun = 'runFunction/?ns=' + encodeURIComponent(ns) + '&args=' + encodeURIComponent(args);
        }
        else if (objType == 'officeitem') {
            var type = object.sub_type;
            var app = object.application;
            urlToRun = 'runOfficeItem/?ns=' + encodeURIComponent(ns) + '&subType=' + type + '&application=' + app;
        }

        let sub = this.httpService.postDataWithAllowOrigin(this.infoService.getMenuBaseUrl() + urlToRun).subscribe((res) => {
            object.isLoading = false;
            sub.unsubscribe();
        })
        // return typeof (window.event) !== 'undefined' && window.event.ctrlKey ? urlToRun + "&notHooked=true" : urlToRun;
    }

    //---------------------------------------------------------------------------------------------
    clearMostUsed() {
        for (let i = this.mostUsed.length - 1; i >= 0; i--) {
            let current = this.mostUsed[i];
            this.removeFromMostUsed(current);
        }
    }

    //---------------------------------------------------------------------------------------------
    clearFavorites() {
        for (let i = this.favorites.length - 1; i >= 0; i--) {
            let current = this.favorites[i];
            this.toggleFavorites(current);
        }
    }

    //---------------------------------------------------------------------------------------------
    loadSearchObjects() {
        this.getSearchObjects();
    }

    //---------------------------------------------------------------------------------------------
    getSearchObjects() {
        if (this.allMenus != undefined) {
            this.findSearchesInApplication(this.allMenus);
        }

        this.searchSources = this.searchSources.sort(this.compareTitle);
    }

    //---------------------------------------------------------------------------------------------
    findSearchesInApplication(application) {

        var tempApplicationArray = this.utilsService.toArray(application);
        for (var a = 0; a < tempApplicationArray.length; a++) {
            var allGroupsArray = this.utilsService.toArray(tempApplicationArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {
                this.getSearchesObjectsFromMenu(allGroupsArray[d], tempApplicationArray[a].title, allGroupsArray[d].title, undefined, undefined);
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    getSearchesObjectsFromMenu(menu, applicationTitle, groupTitle, menuTitle, tileTitle) {

        var allSubObjects = this.utilsService.toArray(menu.Object);
        if (allSubObjects != undefined) {


            for (var i = 0; i < allSubObjects.length; i++) {

                var temp = menu.Object[i];
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

        var allSubMenus = this.utilsService.toArray(menu.Menu);
        if (allSubMenus != undefined) {

            //cerca gli object dentro il menu
            for (var j = 0; j < allSubMenus.length; j++) {

                this.getSearchesObjectsFromMenu(allSubMenus[j], applicationTitle, groupTitle, menu.title, allSubMenus[j].title);
            }
        }
    };

    //---------------------------------------------------------------------------------------------
    getSearchItemTooltip = function (object) {
        // return $sce.trustAsHtml(object.title + "<br/>" + object.applicationTitle + " | " + object.groupTitle + " | " + object.menu + " | " + object.tile);
        return "ciao";
    }


    //---------------------------------------------------------------------------------------------
    containsSameSearch(array, obj) {
        for (var i = 0; i < array.length; i++) {
            var temp = array[i];
            if (temp.target == obj.target && temp.objectType == obj.objectType && temp.title == obj.title) {
                return true;
            }
        }
        return false;
    }

    //---------------------------------------------------------------------------------------------
    toggleFavorites(object) {

        var isFavorite = object.isFavorite;
        if (object.isFavorite == undefined || !object.isFavorite) {
            object.isFavorite = true;
            this.addToFavoritesInternal(object);
            // $rootScope.$emit('favoritesAdded', object);
        }
        else {
            object.isFavorite = false;
            this.removeFromFavoritesInternal(object);
            // $rootScope.$emit('favoritesRemoved', object);
        }
        object.isFavorite = !isFavorite;
    }

    //---------------------------------------------------------------------------------------------
    addToFavoritesInternal(object) {
        object.isFavorite = true;
        object.isJustAdded = true;

        this.favorites.push(object);
        this.favoritesCount++;
        object.position = this.favorites.length;
    }

    //---------------------------------------------------------------------------------------------
    removeFromFavoritesInternal(object) {
        object.isFavorite = false;
        object.isJustAdded = false;
        object.position = undefined;
        for (var i = 0; i < this.favorites.length; i++) {

            if (this.favorites[i].target == object.target && this.favorites[i].objectType == object.objectType &&
                (object.objectName == undefined || (object.objectName != undefined && this.favorites[i].objectName == object.objectName))
            ) {

                this.favorites.splice(i, 1);
                this.favoritesCount--;
                return;
            }
        }
    }

    updateAllFavoritesAndMostUsed() {
        let sub = this.httpMenuService.updateAllFavoritesAndMostUsed(this.favorites, this.mostUsed).subscribe(()=>{
            sub.unsubscribe();
        });
    }

    //---------------------------------------------------------------------------------------------
    setFavoritesIsOpened() {

        // $rootScope.favoritesIsOpened = this.favoritesIsOpened;
    }

    //---------------------------------------------------------------------------------------------
    rearrangePositions() {
        for (var a = 0; a < this.favorites.length; a++) {
            this.favorites[a].position = a;
        }
    }


    //---------------------------------------------------------------------------------------------
    compareFavorites(a, b) {
        if (a.position < b.position)
            return -1;
        if (a.position > b.position)
            return 1;
        return 0;
    }

    //---------------------------------------------------------------------------------------------
    compareMostUsed(a, b) {
        if (a.lastModified < b.lastModified)
            return 1;
        if (a.lastModified > b.lastModified)
            return -1;
        return 0;
    }

    //---------------------------------------------------------------------------------------------
    compareTitle(a, b) {
        if (a.title < b.title)
            return -1;
        if (a.title > b.title)
            return 1;
        return 0;
    }

    getMenuElements() {
        this.resetMenuServices();
        let sub = this.httpMenuService.getMenuElements(this.clearCachedData).subscribe((result) => {
            this.clearCachedData = false;
            this.onAfterGetMenuElements(result.Root);
            sub.unsubscribe();
        });
    }

    invalidateCache() {
        //TODOLUCA , clone del metodo getmenuelements qua sopra, per motivi di async
        this.loadingService.setLoading(true, "reloading menu");
        this.resetMenuServices();
        this.clearCachedData = true;
        let sub = this.httpMenuService.getMenuElements(this.clearCachedData).subscribe((result) => {
            this.clearCachedData = false;
            this.loadingService.setLoading(false);
            this.onAfterGetMenuElements(result.Root);
            sub.unsubscribe();
        });
    }

    //---------------------------------------------------------------------------------------------
    resetMenuServices(){
        this.allMenus = [];
        this.favoritesCount = 0;
        this.mostUsedCount = 0;
        this.favorites = [];
        this.mostUsed = [];
    }

    //---------------------------------------------------------------------------------------------
    onAfterGetMenuElements(root) {
        //creo un unico allmenus che contiene tutte le applicazioni sia di environment che di applications
        let temp = this.utilsService.toArray(root.ApplicationMenu.AppMenu.Application);
        for (var a = 0; a < temp.length; a++) {
            this.allMenus.push(temp[a])
        }

        temp = this.utilsService.toArray(root.EnvironmentMenu.AppMenu.Application);
        for (var a = 0; a < temp.length; a++) {
            this.allMenus.push(temp[a])
        }

        this.initApplicationAndGroup(this.allMenus);
        this.loadFavoritesAndMostUsed();
        this.loadSearchObjects();
    }

    //---------------------------------------------------------------------------------------------
    loadFavoritesAndMostUsed() {
    
        if (this.allMenus != undefined)
            this.findFavoritesAndMostUsedInApplication(this.allMenus);

        this.favorites = this.favorites.sort(this.compareFavorites);
        this.mostUsed = this.mostUsed.sort(this.compareMostUsed);
    }

    //---------------------------------------------------------------------------------------------
    findFavoritesAndMostUsedInApplication(applications) {

        for (var a = 0; a < applications.length; a++) {
            var allGroupsArray = this.utilsService.toArray(applications[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {
                this.getFavoritesAndMostUsedObjectsFromMenu(allGroupsArray[d]);
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    getFavoritesAndMostUsedObjectsFromMenu(menu) {

        var allSubObjects = this.utilsService.toArray(menu.Object);
        for (var i = 0; i < allSubObjects.length; i++) {


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

        var allSubMenus = this.utilsService.toArray(menu.Menu);
        //cerca gli object dentro il menu
        for (var j = 0; j < allSubMenus.length; j++) {
            this.getFavoritesAndMostUsedObjectsFromMenu(allSubMenus[j]);
        }
    }

    //---------------------------------------------------------------------------------------------
    addToMostUsed(object) {
        this.addToMostUsedArray(object);
    }

    //---------------------------------------------------------------------------------------------
    removeFromMostUsed = function (object) {

        this.removeFromMostUsedArray(object);
    };

    //---------------------------------------------------------------------------------------------
    addToMostUsedArray(object) {

        var now = this.utilsService.getCurrentDate();
        for (var i = 0; i < this.mostUsed.length; i++) {

            if (this.mostUsed[i].target == object.target && this.mostUsed[i].objectType == object.objectType &&
                (object.objectName == undefined || (object.objectName != undefined && object.objectName == this.mostUsed[i].objectName))) {
                this.mostUsed[i].lastModified = now;

                this.mostUsed = this.mostUsed.sort(this.compareMostUsed);
                return;
            }
        }

        object.isMostUsed = true;
        object.lastModified = now;
        this.mostUsed.push(object);
        this.mostUsedCount++;

    }

    //---------------------------------------------------------------------------------------------
    removeFromMostUsedArray(object) {
        var index = -1;

        for (var i = 0; i < this.mostUsed.length; i++) {
            if (this.mostUsed[i].target == object.target && this.mostUsed[i].objectType == object.objectType &&
                (object.objectName == undefined || (object.objectName != undefined && this.mostUsed[i].objectName == object.objectName))) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            this.mostUsed.splice(index, 1);
            this.mostUsedCount--;
            this.mostUsed = this.mostUsed.sort(this.compareMostUsed);
        }
    };

    //---------------------------------------------------------------------------------------------
    getFilteredSearch(viewValue, Item, searchInReport, searchInDocument, searchInBatch, startsWith): boolean {
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

        let found: boolean = false;
        if (!startsWith) {
            return title.indexOf(value) >= 0;
        }

        return found = found || this.stringStartsWith(title, value);
    }

    //---------------------------------------------------------------------------------------------
    stringStartsWith(string, prefix): boolean {
        return string.slice(0, prefix.length) == prefix;
    }

    //---------------------------------------------------------------------------------------------
    toggleDecription() {
        this.showDescription = !this.showDescription;
    }

    //---------------------------------------------------------------------------------------------
    activateMenu() {
        this.menuActivated.emit();
    }
}