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

    constructor(private httpMenuService: HttpMenuService, private logger: Logger, private utils: UtilsService, private imageService: ImageService) {
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
    toggleFavorites  (object) {

        var isFavorite = object.isFavorite;
        if (object.isFavorite == undefined || !object.isFavorite) {
            object.isFavorite = true;
            this.httpMenuService.favoriteObject(object);
            //this.favoriteObject(object);
           // $rootScope.$emit('favoritesAdded', object);
        }
        else {
             object.isFavorite = false;
             this.httpMenuService.unFavoriteObject(object);
            //this.unFavoriteObject(object);
           // $rootScope.$emit('favoritesRemoved', object);
        }
        object.isFavorite = !isFavorite;
    }

    
   
    //------------
}