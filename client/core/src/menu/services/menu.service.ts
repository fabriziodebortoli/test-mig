import { Response } from '@angular/http';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Router } from '@angular/router';
import { Injectable, EventEmitter, ComponentFactoryResolver, Input } from '@angular/core';

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

    private isMenuCacheActive: boolean = true;
    private _selectedApplication: any;
    private _selectedGroup: any;
    private _selectedMenu: any;

    public applicationMenu: any;
    public environmentMenu: any;
    public favoritesCount: number = 0;
    public mostUsedCount: number = 0;

    private favorites: Array<any> = [];
    private mostUsed: Array<any> = [];

    public searchSources: Array<any> = [];
    private ifMoreAppsExist: boolean;

    public showDescription: boolean = false;

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
        private httpService: HttpService,
        private webSocketService: WebSocketService,
        private httpMenuService: HttpMenuService,
        private logger: Logger,
        private utilsService: UtilsService,
        private imageService: ImageService,
        private settingsService: SettingsService,
        private componentService: ComponentService
    ) {
        this.logger.debug('MenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    //---------------------------------------------------------------------------------------------
    initApplicationAndGroup(applications) {

        var queryStringLastApplicationName = this.utilsService.getApplicationFromQueryString();
        if (queryStringLastApplicationName != '')
            this.settingsService.LastApplicationName = queryStringLastApplicationName;

        this.ifMoreAppsExist = applications.length > 1;

        if (this.settingsService.LastApplicationName != '' && this.settingsService.LastApplicationName != undefined) {
            for (var i = 0; i < applications.length; i++) {
                if (applications[i].name.toLowerCase() == this.settingsService.LastApplicationName.toLowerCase()) {
                    //this.setSelectedApplication(tempAppArray[i]);
                    this.selectedApplication = applications[i];
                    this.selectedApplication.isSelected = true;
                    this.settingsService.LastApplicationName = applications[i].name;
                    break;
                }
            }
        }

        if (this.selectedApplication == undefined)
            this.setSelectedApplication(applications[0]);

        if (this.settingsService.LastGroupName != '' && this.settingsService.LastGroupName != undefined) {

            for (var i = 0; i < this.selectedApplication.Group.length; i++) {
                if (this.selectedApplication.Group[i].name.toLowerCase() == this.settingsService.LastGroupName.toLowerCase()) {

                    this.selectedGroup = this.selectedApplication.Group[i];
                    this.selectedGroup.isSelected = true;
                    this.settingsService.LastGroupName = this.selectedApplication.Group[i].name;
                    break;
                }
            }
        }

        if (this.selectedGroup == undefined) {
            this.setSelectedGroup(this.selectedApplication.Group[0]);
        }

        if (this.selectedGroup == undefined) {
            return;
        }

        let found = false;
        for (let i = 0; i < this.selectedGroup.Menu.length; i++) {
            if (this.selectedGroup.Menu[i].name.toLowerCase() == this.settingsService.LastMenuName.toLowerCase()) {
                this.setSelectedMenu(this.selectedGroup.Menu[i]);
                return;
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

        if (this.selectedApplication.Group[0] != undefined)
            this.setSelectedGroup(this.selectedApplication.Group[0]);
    }

    //---------------------------------------------------------------------------------------------
    setSelectedGroup(group) {
        if (this.selectedGroup != undefined && this.selectedGroup == group)
            return;

        if (this.selectedGroup != undefined)
            this.selectedGroup.isSelected = false;

        this.selectedGroup = group;
        this.selectedGroup.isSelected = true;

        if (this.selectedGroup.Menu[0] != undefined)
            this.setSelectedMenu(this.selectedGroup.Menu[0]);

        this.setSelectedMenu(undefined);
        // $location.path("/MenuTemplate");
        // $route.reload();
    }

    //---------------------------------------------------------------------------------------------
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
        this.selectedMenu.visible = true;

        // this.eventData.model.Title.value = "Menu > " + menu.name;
    }

    //---------------------------------------------------------------------------------------------
    runFunction = function (object) {
        if (object === undefined)
            return;

        if (object.objectType.toLowerCase() == 'report') {
            this.componentService.createReportComponent(object.target, true);
        }
        else {
            this.webSocketService.runDocument(object.target, object.args)
                .catch(() => { object.isLoading = false; });
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

    //---------------------------------------------------------------------------------------------
    clearMostUsed() {
        this.mostUsed.splice(0, this.mostUsed.length);
        this.mostUsedCount = 0;
    }

    //---------------------------------------------------------------------------------------------
    loadSearchObjects() {
        this.getSearchObjects();
    }

    //---------------------------------------------------------------------------------------------
    getSearchObjects() {
        if (this.applicationMenu != undefined) {
            this.findSearchesInApplication(this.applicationMenu.Application);
        }

        if (this.environmentMenu != undefined)
            this.findSearchesInApplication(this.environmentMenu.Application);

        this.searchSources = this.searchSources.sort(this.compareTitle);
    }

    //---------------------------------------------------------------------------------------------
    findSearchesInApplication(application) {

        for (var a = 0; a < application.length; a++) {
            for (var d = 0; d < application[a].Group.length; d++) {
                this.getSearchesObjectsFromMenu(application[a].Group[d], application[a].title, application[a].Group[d].title, undefined, undefined);
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    getSearchesObjectsFromMenu(menu, applicationTitle, groupTitle, menuTitle, tileTitle) {


        if (menu.Object != undefined) {

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

        if (menu.Menu != undefined) {

            //cerca gli object dentro il menu
            for (var j = 0; j < menu.Menu.length; j++) {

                this.getSearchesObjectsFromMenu(menu.Menu[j], applicationTitle, groupTitle, menu.title, menu.Menu[j].title);
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

    updateAllFavoritesAndMostUsed(): Observable<Response> {
        return this.httpMenuService.updateAllFavoritesAndMostUsed(this.favorites, this.mostUsed).map((res: Response) => {
            return res;
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

        let menuItems = localStorage.getItem("_menuElements");

        if (this.isMenuCacheActive && menuItems) {
            let parsedMenu = JSON.parse(menuItems);
            this.onAfterGetMenuElements(parsedMenu.Root);
            return;
        }

        this.httpMenuService.getMenuElements().subscribe((result) => {
            this.onAfterGetMenuElements(result.Root);
            localStorage.setItem("_menuElements", JSON.stringify(result));
        });
    }

    invalidateCache() {
        localStorage.setItem("_menuElements", "");
        this.httpMenuService.clearCachedData().subscribe(result => {
            location.reload();
        });
    }

    //---------------------------------------------------------------------------------------------
    onAfterGetMenuElements(root) {
        this.applicationMenu = root.ApplicationMenu.AppMenu;
        this.environmentMenu = root.EnvironmentMenu.AppMenu;

        //TODOLUCA
        this.initApplicationAndGroup(this.applicationMenu.Application);//qui bisogna differenziare le app da caricare, potrebbero essere app o environment
        this.loadFavoritesAndMostUsed();
        this.loadSearchObjects();
    }

    //---------------------------------------------------------------------------------------------
    loadFavoritesAndMostUsed() {
        this.favorites.splice(0, this.favorites.length);
        this.mostUsed.splice(0, this.mostUsed.length);

        if (this.applicationMenu != undefined)
            this.findFavoritesAndMostUsedInApplication(this.applicationMenu.Application);
        if (this.environmentMenu != undefined)
            this.findFavoritesAndMostUsedInApplication(this.environmentMenu.Application);


        this.favorites = this.favorites.sort(this.compareFavorites);
        this.mostUsed = this.mostUsed.sort(this.compareMostUsed);
    }

    //---------------------------------------------------------------------------------------------
    findFavoritesAndMostUsedInApplication(application) {

        for (var a = 0; a < application.length; a++) {
            for (var d = 0; d < application[a].Group.length; d++) {
                this.getFavoritesAndMostUsedObjectsFromMenu(application[a].Group[d]);
            }
        }
    }

    //---------------------------------------------------------------------------------------------
    getFavoritesAndMostUsedObjectsFromMenu(menu) {


        if (menu.Object != undefined) {

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
        //cerca gli object dentro il menu

        if (menu.Menu != undefined) {
            for (var j = 0; j < menu.Menu.length; j++) {
                this.getFavoritesAndMostUsedObjectsFromMenu(menu.Menu[j]);
            }

        }
    };

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