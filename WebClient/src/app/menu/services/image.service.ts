import { CookieService } from 'angular2-cookie/services/cookies.service';
import { UtilsService, HttpService } from 'tb-core';
import { Logger } from 'libclient';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';

@Injectable()
export class ImageService extends HttpService {


    constructor(protected http: Http,
        protected utils: UtilsService,
        protected logger: Logger,
        protected cookieService: CookieService) {
            super(http, utils, logger, cookieService);
        this.logger.debug('ImageService instantiated - ' + Math.round(new Date().getTime() / 1000));
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
        return imageFile === undefined ? 'Images/Default.png' : super.getMenuBaseUrl(false) + '/staticimage/' + imageFile;
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
    getWorkerImage = function (item) {

        if (item == undefined)
            return;

        var imageFile = item['image_file'];
        if (imageFile == undefined || imageFile == '')
            return undefined;

        return 'staticimage/' + imageFile;
    }
}