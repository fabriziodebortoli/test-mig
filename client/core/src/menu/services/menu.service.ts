import { DiagnosticService } from './../../core/services/diagnostic.service';
import { SettingsService } from './../../core/services/settings.service';
import { LoadingService } from './../../core/services/loading.service';
import { Injectable, EventEmitter, ComponentFactoryResolver, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Response } from '@angular/http';
import { Observable, BehaviorSubject } from '../../rxjs.imports';

import { InfoService } from './../../core/services/info.service';
import { HttpService } from './../../core/services/http.service';
import { ComponentService } from './../../core/services/component.service';
import { UtilsService } from './../../core/services/utils.service';
import { WebSocketService } from './../../core/services/websocket.service';
import { Logger } from './../../core/services/logger.service';
import { ImageService } from './image.service';
import { HttpMenuService } from './http-menu.service';

@Injectable()
export class MenuService {

    public isMenuCacheActive: boolean = true;
    public _selectedApplication: any;
    public _selectedGroup: any;
    public _selectedMenu: any;

    public favoritesCount: number = 0;
    public mostUsedCount: number = 0;
    public hiddenTilesCount: number = 0;
    public allMenus: Array<any> = [];

    public hiddenTiles = [];
    public favorites = [];
    public mostUsed = [];

    public searchSources: Array<any> = [];
    public ifMoreAppsExist: boolean;

    public showDescription: boolean = false;
    public clearCachedData = false;
    runFunctionStarted = new EventEmitter<any>();
    runFunctionCompleted = new EventEmitter<any>();

    public isLoading = false;   //concetto generico di isLoading, per evitare loading multiple

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
        public diagnosticService: DiagnosticService,
        public loadingService: LoadingService        
    ) {
        this.logger.debug('MenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    //---------------------------------------------------------------------------------------------
    initApplicationAndGroup() {

        let applications = this.allMenus;

        var queryStringLastApplicationName = this.utilsService.getApplicationFromQueryString();
        if (queryStringLastApplicationName != '')
            this.settingsService.LastApplicationName = queryStringLastApplicationName;

        var tempAppArray = applications;
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
            var tempGroupArray = this.selectedApplication.Group;

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


        let found = false;
        if (this.selectedGroup.Menu) {
            for (let i = 0; i < this.selectedGroup.Menu.length; i++) {
                if (this.selectedGroup.Menu[i].name.toLowerCase() == this.settingsService.LastMenuName.toLowerCase()) {
                    this.setSelectedMenu(this.selectedGroup.Menu[i]);
                    return;
                }
            }
        }

        if (!found) {
            this.setSelectedMenu(this.selectedGroup.Menu[0]);
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

        var tempGroupArray = this.selectedApplication.Group;

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

        var tempMenuArray = this.selectedGroup.Menu;

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
    runFunction(object) {
        if (object === undefined)
            return;

        if (this.isLoading)
            return;

        this.runFunctionStarted.emit();

        if (this.infoService.isDesktop) {
            this.runObject(object);
        }
        else {
            if (object.objectType.toLowerCase() == 'report') {
                this.componentService.createReportComponent(object.target, true);
            }
            else {
                this.webSocketService.runDocument(object.target, object.args)
                    .catch((e) => {
                        object.isLoading = this.isLoading = false;
                        this.diagnosticService.showError(e);
                    });
            }
        }
        this.addToMostUsed(object);
        object.isLoading = this.isLoading = true;
        let subs1 = this.componentService.componentInfoCreated.subscribe(arg => {
            object.isLoading = this.isLoading = false;
            subs1.unsubscribe();
        });

        //TODOLUCA, leak, se lancio mille documenti con successo, mi rimangono mille componentCreationError senza unsubscribe
        let subs2 = this.componentService.componentCreationError.subscribe(reason => {
            object.isLoading = this.isLoading = false;
            subs2.unsubscribe();
        });
    }

    runObject(object: any) {
        let urlToRun = "";
        let objType = object.objectType.toLowerCase();
        let ns = encodeURIComponent(object.target.toLowerCase());
        let type = object.sub_type ? object.sub_type : '';
        let app = object.application ? object.application : '';
        let args = object.arguments ? encodeURIComponent(object.arguments) : '';

        if (objType == 'document')
            urlToRun = 'runDocument/';
        else if (objType == 'batch')
            urlToRun = 'runDocument/';
        else if (objType == 'report') {
            urlToRun = 'runReport/';
        }
        else if (objType == 'function') {
            if (object.isUrl)
                urlToRun = 'runUrl/';
            else
                urlToRun = 'runFunction/';
        }
        else if (objType == 'officeitem') {
            urlToRun = 'runOfficeItem/';
        }
        let obj = {
            authtoken: sessionStorage.getItem('authtoken'),
            ns: ns,
            args: args,
            title: object.title,
            subType: type,
            application: app
        }
        let sub = this.httpService.postData(this.infoService.getMenuBaseUrl() + urlToRun, obj).subscribe((res) => {
            object.isLoading = this.isLoading = false;
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

        for (var a = 0; a < application.length; a++) {
            var allGroupsArray = application[a].Group;
            for (var d = 0; d < allGroupsArray.length; d++) {
                this.getSearchesObjectsFromMenu(allGroupsArray[d], application[a].title, allGroupsArray[d].title, undefined, undefined);
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    getSearchesObjectsFromMenu(menu, applicationTitle, groupTitle, menuTitle, tileTitle) {

        if (menu.Object) {
            for (var i = 0; i < menu.Object.length; i++) {

                var temp = menu.Object[i];
                if (this.containsSameSearch(this.searchSources, temp)) {
                    continue;
                }

                if (tileTitle != undefined)
                    menu.Object[i].tile = tileTitle;
                if (menuTitle != undefined)
                    menu.Object[i].menu = menuTitle;

                menu.Object[i].groupTitle = groupTitle;
                menu.Object[i].applicationTitle = applicationTitle;

                menu.Object[i].itemTooltip = this.getSearchItemTooltip(menu.Object[i]);
                this.searchSources.push(menu.Object[i]);
            }
        }

        if (menu.Menu) {

            //cerca gli object dentro il menu
            for (var j = 0; j < menu.Menu.length; j++) {

                this.getSearchesObjectsFromMenu(menu.Menu[j], applicationTitle, groupTitle, menu.title, menu.Menu[j].title);
            }
        }
    };

    //---------------------------------------------------------------------------------------------
    getSearchItemTooltip(object) {
        return object.title + ' | ' + object.applicationTitle + " | " + object.groupTitle + " | " + object.menu + " | " + object.tile;
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

        if (object.isFavorite == undefined || !object.isFavorite) {
            object.isFavorite = true;
            this.addToFavoritesInternal(object);
        }
        else {
            object.isFavorite = false;
            this.removeFromFavoritesInternal(object);
        }

        let subs = this.httpMenuService.updateFavorites(this.favorites).subscribe(() => {
            subs.unsubscribe()
        });
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
    resetMenuServices() {
        this._selectedApplication = undefined;
        this._selectedGroup = undefined;
        this._selectedMenu = undefined;
        this.searchSources = [];
        this.allMenus = [];
        this.favoritesCount = 0;
        this.mostUsedCount = 0;
        this.hiddenTilesCount = 0;
        this.favorites = [];
        this.mostUsed = [];
        this.hiddenTiles = [];
    }

    //---------------------------------------------------------------------------------------------
    onAfterGetMenuElements(root) {
        let tempMenus = [];
        let orderedMenus = [];
        //creo un unico allmenus che contiene tutte le applicazioni sia di environment che di applications
        let temp = root.ApplicationMenu.AppMenu.Application;
        for (var a = 0; a < temp.length; a++) {
            if (temp[a].name.toLowerCase() == "erp")
                orderedMenus.push(temp[a]);
            else if (temp[a].name.toLowerCase() == "tbs")
                orderedMenus.push(temp[a]);
            else
                tempMenus.push(temp[a])
        }

        temp = root.EnvironmentMenu.AppMenu.Application;
        for (var a = 0; a < temp.length; a++) {
            if (temp[a].name.toLowerCase() == "framework")
                orderedMenus.push(temp[a]);
            else
                tempMenus.push(temp[a]);
        }

        for (var a = 0; a < tempMenus.length; a++) {
            orderedMenus.push(tempMenus[a]);
        }

        this.sanitizeAllMenus(orderedMenus);
        this.initApplicationAndGroup();
        this.loadFavoritesAndMostUsed();
        this.loadSearchObjects();
        this.loadHiddenTiles();
    }

    //---------------------------------------------------------------------------------------------
    sanitizeAllMenus(allApps) {
        if (allApps) {
            allApps.forEach(app => {

                app.Group = this.utilsService.toArray(app.Group);
                if (app.Group) {
                    app.Group.forEach(menu => {
                        menu.Menu = this.utilsService.toArray(menu.Menu).filter(
                            currentMenu => {
                                return this.utilsService.toArray(currentMenu.Menu).length > 0 || this.utilsService.toArray(currentMenu.Object).length > 0;
                            });

                        //menu orfani a tre livelli
                        if (menu.Object) {
                            menu.Object = this.utilsService.toArray(menu.Object);
                        }

                        if (menu.Menu) {
                            menu.Menu.forEach(subMenu => {
                                subMenu.Menu = this.utilsService.toArray(subMenu.Menu);

                                if (subMenu.Menu) {
                                    subMenu.Menu.forEach(object => {
                                        object.Object = this.utilsService.toArray(object.Object);
                                    });
                                }
                            });
                        }
                    });
                }
            });
        }

        this.allMenus = allApps;
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
            var allGroupsArray = applications[a].Group;
            for (var d = 0; d < allGroupsArray.length; d++) {
                this.getFavoritesAndMostUsedObjectsFromMenu(allGroupsArray[d]);
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    getFavoritesAndMostUsedObjectsFromMenu(menu) {

        if (menu.Object) {
            for (var i = 0; i < menu.Object.length; i++) {


                if (menu.Object[i].isFavorite) {
                    menu.Object[i].position = parseInt(menu.Object[i].position);
                    this.favoritesCount++;
                    this.favorites.push(menu.Object[i]);
                }

                if (menu.Object[i].isMostUsed) {
                    menu.Object[i].lastModified = parseInt(menu.Object[i].lastModified);
                    this.mostUsed.push(menu.Object[i]);
                    this.mostUsedCount++;
                }
            }
        }

        if (menu.Menu) {
            //cerca gli object dentro il menu
            for (var j = 0; j < menu.Menu.length; j++) {
                this.getFavoritesAndMostUsedObjectsFromMenu(menu.Menu[j]);
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    addToMostUsed(object) {
        this.addToMostUsedArray(object);
        let subs = this.httpMenuService.updateMostUsed(this.mostUsed).subscribe(() => {
            subs.unsubscribe()
        });
    }

    //---------------------------------------------------------------------------------------------
    removeFromMostUsed(object) {
        this.removeFromMostUsedArray(object);
        let subs = this.httpMenuService.updateMostUsed(this.mostUsed).subscribe(() => {
            subs.unsubscribe()
        });
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


    addToHiddenTiles(menu, tile) {

        let sub = this.httpMenuService.addToHiddenTiles(this.selectedApplication.name, this.selectedGroup.name, menu.name, tile.name)
            .subscribe(() => {
                tile.currentApp = this.selectedApplication.name;
                tile.currentGroup = this.selectedGroup.name;
                tile.currentMenu = menu.name;

                tile.currentAppTitle = this.selectedApplication.title;
                tile.currentGroupTitle = this.selectedGroup.title;
                tile.currentMenuTitle = menu.title;

                tile.tooltip = tile.currentAppTitle + " | " + tile.currentGroupTitle + " | " + tile.currentMenuTitle;

                this.addToHiddenTilesArray(tile);
                this.selectedMenuChanged.emit(); //stuzzico la rigenerazione delle tiles
                sub.unsubscribe();
            });
    }

    //---------------------------------------------------------------------------------------------
    removeFromHiddenTiles(tile) {

        let sub = this.httpMenuService.removeFromHiddenTiles(tile.currentApp, tile.currentGroup, tile.currentMenu, tile.name)
            .subscribe(() => {
                this.removeFromHiddenTilesArray(tile);
                this.selectedMenuChanged.emit();//stuzzico la rigenerazione delle tiles
                sub.unsubscribe();
            });
    }

    removeAllHiddenTiles() {
        let sub = this.httpMenuService.removeAllHiddenTiles()
            .subscribe(() => {
                this.hiddenTiles.forEach((tile) => tile.hiddenTile = false);
                this.hiddenTiles = [];
                this.hiddenTilesCount = 0;
                this.selectedMenuChanged.emit();//stuzzico la rigenerazione delle tiles
                sub.unsubscribe();
            });
    }



    //---------------------------------------------------------------------------------------------
    addToHiddenTilesArray(tile: any) {
        tile.hiddenTile = true;

        for (var i = 0; i < this.hiddenTiles.length; i++) {
            if (this.hiddenTiles[i] == tile) {
                return;
            }
        }

        this.hiddenTiles.push(tile);
        this.hiddenTilesCount++;
        // this.showOthers();
    }

    //---------------------------------------------------------------------------------------------
    removeFromHiddenTilesArray(tile) {
        var index = -1;

        for (var i = 0; i < this.hiddenTiles.length; i++) {
            if (this.hiddenTiles[i] == tile) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            tile.hiddenTile = false;
            this.hiddenTiles.splice(index, 1);
            this.hiddenTilesCount--;
        }
    };

    //---------------------------------------------------------------------------------------------
    loadHiddenTiles() {

        this.hiddenTiles = [];

        this.findHiddenTilesInApplication();
    };

    findHiddenTilesInApplication() {

        for (var a = 0; a < this.allMenus.length; a++) {
            var allGroupsArray = this.allMenus[a].Group;
            for (var d = 0; d < allGroupsArray.length; d++) {

                var allMenusArray = allGroupsArray[d].Menu;
                for (var m = 0; m < allMenusArray.length; m++) {

                    var allTiles = allMenusArray[m].Menu;
                    for (var t = 0; t < allTiles.length; t++) {
                        if (this.utilsService.parseBool(allTiles[t].hiddenTile) == true) {
                            allTiles[t].currentApp = this.allMenus[a].name;
                            allTiles[t].currentGroup = allGroupsArray[d].name;
                            allTiles[t].currentMenu = allMenusArray[m].name;

                            allTiles[t].currentAppTitle = this.allMenus[a].title;
                            allTiles[t].currentGroupTitle = allGroupsArray[d].title;
                            allTiles[t].currentMenuTitle = allMenusArray[m].title;

                            this.hiddenTiles.push(allTiles[t]);
                            this.hiddenTilesCount++;
                        }
                    }
                }
            }
        }
    };

    /*controlla se ci sono dei tile nascosti nel menu corrente  --  credo non venga usata*/
    //---------------------------------------------------------------------------------------------
    ifMenuExistInHiddenTiles() {

        if (this.selectedMenu == undefined || this.selectedApplication == undefined)
            return false;

        for (var i = 0; i < this.hiddenTiles.length; i++) {
            if ((this.hiddenTiles[i].currentMenuTitle == this.selectedMenu.title) && (this.hiddenTiles[i].currentAppTitle == this.selectedApplication.title))
                return true;
        }
        return false;
    }

    //---------------------------------------------------------------------------------------------
    ifOtherTilesAreHidden() {
        if (this.selectedMenu == undefined)
            return true;

        for (var i = 0; i < this.hiddenTiles.length; i++) {
            if (this.hiddenTiles[i].currentMenuTitle != this.selectedMenu.title)
                return true;
        }
        return false;
    }

    //     //---------------------------------------------------------------------------------------------

    //     $scope.showOthers = function () {
    //         var display = $(".othersHiddenContainer").css("display");
    //         if (display == 'none')
    //             $(".othersHiddenContainer").css("display", "block");

    //     }


    //     //---------------------------------------------------------------------------------------------

    //     $scope.hideOthers = function () {
    //         var display = $(".othersHiddenContainer").css("display");
    //         if (display == 'none')
    //             $(".othersHiddenContainer").css("display", "block");
    //         else
    //             $(".othersHiddenContainer").css("display", "none");

    //     }
    // }
}