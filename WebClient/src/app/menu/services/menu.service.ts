import { UtilsService } from 'tb-core';
import { Logger } from 'libclient';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';

@Injectable()
export class MenuService {

    private baseUrl = 'http://localhost:10000/';

    private selectedApplication: any;
    private selectedGroup: any;
    private selectedMenu: any;

    public applicationMenu: any;
    public environmentMenu: any;

    constructor(private http: Http, private logger: Logger, private utils: UtilsService) {
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
        return this.getStaticImage(application);
    }

    getStaticImage(item) {

        if (item == undefined) {
            return undefined;
        }

        if (Object.prototype.toString.call(item) === '[object String]') {
            return 'staticimage/' + item;
        }

        let imageFile = item['image_file'];
        return imageFile === undefined ? 'Images/Default.png' : this.baseUrl + 'tb/menu/staticimage/' + imageFile;
    }




    //---------------------------------------------------------------------------------------------
    getObjectIcon = function (object) {
        if (object.sub_type != undefined) {
            if (object.application == undefined)
                return object.sub_type;

            return object.sub_type + object.application;
        }
        return object.objectType;
    }

    //---------------------------------------------------------------------------------------------
    isCustomImage = function (object) {
        return object.isCustomImage == undefined || object.isCustomImage == 'Images/Default.png';
    }


    //---------------------------------------------------------------------------------------------
    getStaticThumbnail = function (document) {

        var urlToRun = 'staticThumbnail/?ns=' + document.target;
        return urlToRun;
    }

    //---------------------------------------------------------------------------------------------
    getWorkerImage = function (item) {

        if (item == undefined)
            return;

        var imageFile = item['image_file'];
        if (imageFile == undefined || imageFile == '')
            return undefined;

        return 'staticimage/' + imageFile;
    }






}