import { HttpMenuService } from './http-menu.service';
import { UtilsService } from 'tb-core';
import { Logger } from 'libclient';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';
import { ImageService } from './image.service';
import { SettingsService } from './settings.service';
import { DocumentInfo } from 'tb-shared';

@Injectable()
export class MenuService {

    public selectedApplication: any;
    public selectedGroup: any;
    public selectedMenu: any;

    public applicationMenu: any;
    public environmentMenu: any;
    public favoritesCount: number = 0;
    public mostUsedCount: number = 0;
    public hiddenTilesCount: number = 0;

    private favorites: Array<any> = [];
    private mostUsed: Array<any> = [];
    public hiddenTiles: Array<any> = [];


    private ifMoreAppsExist: boolean;

    constructor(
        private httpMenuService: HttpMenuService,
        private logger: Logger,
        private utilsService: UtilsService,
        private imageService: ImageService,
        private settingsService: SettingsService
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

    setSelectedApplication(application) {
        if (this.selectedApplication != undefined && this.selectedApplication.title == application.title)
            return;

        if (this.selectedApplication != undefined)
            this.selectedApplication.isSelected = false;

        this.selectedApplication = application;
        this.selectedApplication.isSelected = true;

        this.settingsService.lastApplicationName = application.name;
        this.settingsService.setPreference('LastApplicationName', encodeURIComponent(this.settingsService.lastApplicationName), undefined);

        var tempGroupArray = this.utilsService.toArray(this.selectedApplication.Group);
        if (tempGroupArray[0] != undefined)
            this.setSelectedGroup(tempGroupArray[0]);
    }



    getSelectedApplication(app) {
        return this.selectedApplication;
    }

    setSelectedGroup(group) {
        if (this.selectedGroup != undefined && this.selectedGroup == group)
            return;

        if (this.selectedGroup != undefined)
            this.selectedGroup.isSelected = false;

        this.selectedGroup = group;
        this.selectedGroup.isSelected = true;
        this.settingsService.lastGroupName = group.name;
        this.settingsService.setPreference('LastGroupName', encodeURIComponent(this.settingsService.lastGroupName), undefined);

        var tempMenuArray = this.utilsService.toArray(this.selectedGroup.Menu);
        if (tempMenuArray[0] != undefined)
            this.setSelectedMenu(tempMenuArray[0]);

        this.setSelectedMenu(undefined);
        // $location.path("/MenuTemplate");
        // $route.reload();
    }

    getSelectedGroup() {
        return this.selectedGroup;
    }

    setSelectedMenu(menu) {
        if (this.selectedMenu != undefined && this.selectedMenu == menu)
            return;

        //deseleziono il vecchio se presente
        //if (this.selectedMenu != undefined)
        //    this.selectedMenu.active = false;

        if (menu == undefined) {
            this.selectedMenu = undefined;
            this.settingsService.lastMenuName = '';
            this.settingsService.setPreference('LastMenuName', encodeURIComponent(this.settingsService.lastMenuName), undefined);
            return;
        }

        this.selectedMenu = menu;
        this.settingsService.lastMenuName = menu.name;
        this.settingsService.setPreference('LastMenuName', encodeURIComponent(this.settingsService.lastMenuName), undefined);
        this.selectedMenu.active = true;
        menu.visible = true
    }

    getSelectedMenu() {
        return this.selectedMenu;
    }

    getApplicationIcon(application) {
        return this.imageService.getStaticImage(application);
    }

    runFunction = function (object) {
        this.httpMenuService.runObject(new DocumentInfo(0, object.target, this.utilsService.generateGUID()));
        this.addToMostUsed(object);
    }

    clearMostUsed() {
        this.mostUsed.splice(0, this.mostUsed.length);
        this.mostUsedCount = 0;
    }

    //---------------------------------------------------------------------------------------------
    hideTile(tile) {
        //$rootScope.$emit('hiddenTileAdded', this.selectedMenu, tile);
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

            if (this.favorites[i].target == object.target && this.favorites[i].objectType == object.objectType) {

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

            if (this.mostUsed[i].target == object.target && this.mostUsed[i].objectType == object.objectType) {
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
            if (this.mostUsed[i].target == object.target && this.mostUsed[i].objectType == object.objectType) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            this.mostUsed.splice(index, 1);
            this.mostUsedCount--;
        }
    };

    loadHiddenTiles() {
        if (this.applicationMenu != undefined)
            this.findHiddenTilesInApplication(this.applicationMenu.Application);
        if (this.environmentMenu != undefined)
            this.findHiddenTilesInApplication(this.environmentMenu.Application);
    }


    //---------------------------------------------------------------------------------------------
    findHiddenTilesInApplication(application) {

        var tempAppArray = this.utilsService.toArray(application);
        for (var a = 0; a < tempAppArray.length; a++) {
            var allGroupsArray = this.utilsService.toArray(tempAppArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {

                var allMenusArray = this.utilsService.toArray(allGroupsArray[d].Menu);
                for (var m = 0; m < allMenusArray.length; m++) {

                    var allTiles = this.utilsService.toArray(allMenusArray[m].Menu);
                    for (var t = 0; t < allTiles.length; t++) {
                        if (this.utilsService.parseBool(allTiles[t].hiddenTile) == true) {
                            allTiles[t].currentApp = tempAppArray[a].name;
                            allTiles[t].currentGroup = allGroupsArray[d].name;
                            allTiles[t].currentMenu = allMenusArray[m].name;

                            allTiles[t].currentAppTitle = tempAppArray[a].title;
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

}