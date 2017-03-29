import { ComponentService } from './../../core/component.service';
import { HttpService } from './../../core/http.service';
import { UtilsService } from './../../core/utils.service';
import { Injectable, EventEmitter, ComponentFactoryResolver, Input } from '@angular/core';
import { Router } from '@angular/router';

import { HttpMenuService } from './http-menu.service';
import { ImageService } from './image.service';
import { SettingsService } from './settings.service';


import { Logger } from 'libclient';

@Injectable()
export class MenuService {

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

    get selectedMenu(): any {
        return this._selectedMenu;
    }

    @Input()
    set selectedMenu(menu: any) {
        this._selectedMenu = menu;
        if (menu != undefined) {
            this.settingsService.lastMenuName = menu.name;
            this.settingsService.setPreference('LastMenuName', encodeURIComponent(this.settingsService.lastMenuName), undefined);
        }
        this.selectedMenuChanged.emit();
    }

    get selectedGroup(): any {
        return this._selectedGroup;
    }

    @Input()
    set selectedGroup(group: any) {
        this._selectedGroup = group;
        if (group != undefined) {
            this.settingsService.lastGroupName = group.name;
            this.settingsService.setPreference('LastGroupName', encodeURIComponent(this.settingsService.lastGroupName), undefined);
        }
        this.selectedGroupChanged.emit();
    }

    get selectedApplication(): any {
        return this._selectedApplication;
    }

    @Input()
    set selectedApplication(application: any) {
        this._selectedApplication = application;
        if (application != undefined) {
            this.settingsService.lastApplicationName = application.name;
            this.settingsService.setPreference('LastApplicationName', encodeURIComponent(this.settingsService.lastApplicationName), undefined);
        }
        this.selectedApplicationChanged.emit();
    }

    selectedMenuChanged: EventEmitter<any> = new EventEmitter(true);
    selectedApplicationChanged: EventEmitter<any> = new EventEmitter(true);
    selectedGroupChanged: EventEmitter<any> = new EventEmitter(true);

    constructor(
        private httpService: HttpService,
        private httpMenuService: HttpMenuService,
        private logger: Logger,
        private utilsService: UtilsService,
        private imageService: ImageService,
        private settingsService: SettingsService,
        private router: Router,
        private componentService: ComponentService,
        private resolver: ComponentFactoryResolver
    ) {
        this.logger.debug('MenuService instantiated - ' + Math.round(new Date().getTime() / 1000));

    }

    //---------------------------------------------------------------------------------------------
    initApplicationAndGroup(applications) {

        var queryStringLastApplicationName = this.utilsService.getApplicationFromQueryString();
        if (queryStringLastApplicationName != '')
            this.settingsService.lastApplicationName = queryStringLastApplicationName;

        var tempAppArray = this.utilsService.toArray(applications);
        this.ifMoreAppsExist = tempAppArray.length > 1;

        if (this.settingsService.lastApplicationName != '' && this.settingsService.lastApplicationName != undefined) {
            for (var i = 0; i < tempAppArray.length; i++) {
                if (tempAppArray[i].name.toLowerCase() == this.settingsService.lastApplicationName.toLowerCase()) {
                    this.selectedApplication = tempAppArray[i];
                    this.selectedApplication.isSelected = true;
                    this.settingsService.lastApplicationName = tempAppArray[i].name;
                    break;
                }
            }
        }

        if (this.selectedApplication == undefined)
            this.setSelectedApplication(tempAppArray[0]);

        if (this.settingsService.lastGroupName != '' && this.settingsService.lastGroupName != undefined) {
            var tempGroupArray = this.utilsService.toArray(this.selectedApplication.Group);
            for (var i = 0; i < tempGroupArray.length; i++) {
                if (tempGroupArray[i].name.toLowerCase() == this.settingsService.lastGroupName.toLowerCase()) {
                    this.selectedGroup = tempGroupArray[i];
                    this.selectedGroup.isSelected = true;
                    this.settingsService.lastGroupName = tempGroupArray[i].name;
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
            this.settingsService.lastMenuName = '';
            this.settingsService.setPreference('LastMenuName', encodeURIComponent(this.settingsService.lastMenuName), undefined);
            return;
        }

        this.selectedMenu = menu;
        this.selectedMenu.active = true;
        menu.visible = true;

        // this.eventData.model.Title.value = "Menu > " + menu.name;
    }

    //---------------------------------------------------------------------------------------------
    getApplicationIcon(application) {
        return this.imageService.getStaticImage(application);
    }

    //---------------------------------------------------------------------------------------------
    runFunction = function (object) {
        if (object === undefined)
            return;

        if (object.objectType.toLowerCase() == 'report') {
            let obs = this.httpService.runReport(object.target).subscribe((jsonObj) => {
                if (!jsonObj.desktop) {
                    this.componentService.createComponentFromUrl('rs/reportingstudio/' + object.target);
                }
                obs.unsubscribe();
            });
        }
        else {
            this.httpService.runDocument(object.target);
        }

        this.addToMostUsed(object);
        object.isLoading = true;
        let subs = this.componentService.componentCreated.subscribe(info => {
            object.isLoading = false;
            subs.unsubscribe();
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

                var temp = allSubObjects[i];
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
            return -1;
        if (a.lastModified > b.lastModified)
            return 1;
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

    //---------------------------------------------------------------------------------------------
    onAfterGetMenuElements(root) {
        this.applicationMenu = root.ApplicationMenu.AppMenu;
        this.environmentMenu = root.EnvironmentMenu.AppMenu;
        this.loadFavoritesAndMostUsed();
        this.loadSearchObjects();
    }

    //---------------------------------------------------------------------------------------------
    loadFavoritesAndMostUsed() {
        if (this.applicationMenu != undefined)
            this.findFavoritesAndMostUsedInApplication(this.applicationMenu.Application);
        if (this.environmentMenu != undefined)
            this.findFavoritesAndMostUsedInApplication(this.environmentMenu.Application);

        this.favorites = this.favorites.sort(this.compareFavorites);
        this.mostUsed = this.mostUsed.sort(this.compareMostUsed);
    }

    //---------------------------------------------------------------------------------------------
    findFavoritesAndMostUsedInApplication(application) {

        var tempMenuArray = this.utilsService.toArray(application);
        for (var a = 0; a < tempMenuArray.length; a++) {
            var allGroupsArray = this.utilsService.toArray(tempMenuArray[a].Group);
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
    };

    getFavorites() {
        return this.favorites;
    }


    // //---------------------------------------------------------------------------------------------
    // this.listenToMostUsed = function () {
    //     $rootScope.$on('runFunctionCompleted', function (event, object) { this.addToMostUsed(object); });
    // };

    //---------------------------------------------------------------------------------------------
    addToMostUsed(object) {

        // if (!settingsService.showMostUsedOptions)
        //     return;
        this.httpMenuService.addToMostUsed(object).subscribe(result => {
            this.addToMostUsedArray(object);
        })
    }


    //---------------------------------------------------------------------------------------------
    removeFromMostUsed = function (object) {

        // if (!settingsService.showMostUsedOptions)
        //     return;
        this.httpMenuService.removeFromMostUsed(object).subscribe(result => {
            this.removeFromMostUsedArray(object);
        })
    };

    //---------------------------------------------------------------------------------------------
    addToMostUsedArray(object) {

        var now = this.utilsService.getCurrentDate();
        for (var i = 0; i < this.mostUsed.length; i++) {

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


}