import { HttpMenuService } from './http-menu.service';
import { UtilsService } from 'tb-core';
import { Logger } from 'libclient';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';
import { ImageService } from './image.service';

@Injectable()
export class MenuService {

    private selectedApplication: any;
    private selectedGroup: any;
    private selectedMenu: any;

    public applicationMenu: any;
    public environmentMenu: any;
    public favoritesCount: number = 0;

    private favorites: Array<any>;
    private utilsService: UtilsService;
    constructor(private httpMenuService: HttpMenuService, private logger: Logger, private utils: UtilsService, private imageService: ImageService) {
        this.utilsService = utils;
        this.logger.debug('MenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    setSelectedApplication(app) {
        this.selectedApplication = app;
        this.selectedGroup = undefined;
        this.selectedMenu = undefined;
    }

    getSelectedApplication(app) {
        return this.selectedApplication;
    }

    setSelectedGroup(group) {
        this.selectedGroup = group;
        this.selectedMenu = undefined;
    }

    getSelectedGroup() {
        return this.selectedGroup;
    }

    setSelectedMenu(menu) {
        this.selectedMenu = menu;
    }

    getSelectedMenu() {
        return this.selectedMenu;
    }

    getApplicationIcon(application) {
        return this.imageService.getStaticImage(application);
    }

    //---------------------------------------------------------------------------------------------
    toggleFavorites(object) {

        var isFavorite = object.isFavorite;
        if (object.isFavorite == undefined || !object.isFavorite) {
            object.isFavorite = true;
            this.httpMenuService.favoriteObject(object);
            this.addToFavoritesInternal(object);
            //this.favoriteObject(object);
            // $rootScope.$emit('favoritesAdded', object);
        }
        else {
            object.isFavorite = false;
            this.httpMenuService.unFavoriteObject(object);
            this.removeFromFavoritesInternal(object);
            //this.unFavoriteObject(object);
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

        // $rootScope.favoritesIsOpened = $scope.favoritesIsOpened;
    }

    //---------------------------------------------------------------------------------------------
    rearrangePositions() {
        for (var a = 0; a < this.favorites.length; a++) {
            this.favorites[a].position = a;
        }
    }


    //---------------------------------------------------------------------------------------------
    compare(a, b) {
        if (a.position < b.position)
            return -1;
        if (a.position > b.position)
            return 1;
        return 0;
    }


    //---------------------------------------------------------------------------------------------
    loadFavoriteObjects() {
        // if ($rootScope.menu == undefined || $scope.isDragging)
        //    return   this.favorites;
        this.favorites = this.getFavoriteObjectsInternal().sort(this.compare);
    }

    //---------------------------------------------------------------------------------------------
    getFavoriteObjectsInternal() {
        var filtered = [];

        if (this.applicationMenu != undefined)
            this.findFavoritesInApplication(this.applicationMenu.Application, filtered);
        if (this.environmentMenu != undefined)
            this.findFavoritesInApplication(this.environmentMenu.Application, filtered);

        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    findFavoritesInApplication(application, filtered) {

        var tempMenuArray = this.utilsService.toArray(application);
        for (var a = 0; a < tempMenuArray.length; a++) {
            var allGroupsArray = this.utilsService.toArray(tempMenuArray[a].Group);
            for (var d = 0; d < allGroupsArray.length; d++) {
                this.getFavoritesObjectsFromMenu(allGroupsArray[d], filtered);
            }
        }
        return filtered;
    }

    //---------------------------------------------------------------------------------------------
    getFavoritesObjectsFromMenu(menu, filtered) {

        var allSubObjects = this.utilsService.toArray(menu.Object);
        for (var i = 0; i < allSubObjects.length; i++) {

            if (allSubObjects[i].isFavorite) {
                allSubObjects[i].position = parseInt(allSubObjects[i].position);
                {
                    this.favoritesCount++;
                    filtered.push(allSubObjects[i]);
                }
            }
        }

        var allSubMenus = this.utilsService.toArray(menu.Menu);
        //cerca gli object dentro il menu
        for (var j = 0; j < allSubMenus.length; j++) {

            this.getFavoritesObjectsFromMenu(allSubMenus[j], filtered);
        }

        return filtered;
    };

    //------------
}