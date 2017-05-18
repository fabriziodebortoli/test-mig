import { UtilsService } from './../../core/utils.service';
import { HttpService } from './../../core/http.service';
import { Injectable } from '@angular/core';
import { Http } from '@angular/http';

import { CookieService } from 'angular2-cookie/services/cookies.service';
import { Logger } from './../../core/logger.service';


@Injectable()
export class ImageService {
      
    constructor(
        protected http: Http,
        protected utils: UtilsService,
        protected logger: Logger,
        protected cookieService: CookieService,
        private httpService: HttpService) {
        this.logger.debug('ImageService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    getApplicationIcon(application) {

        let url = 'assets/images/';

        switch(application.name){
            case 'TBS':
                url += 'LogoTBSSmall.png';
                break;
            case 'ERP':
                url += 'LogoMagoNetSmall.png';
                break;
            default:
                url = this.getStaticImage(application);
        }

        return url;
    }

    getStaticImage(item) {

        if (item == undefined) {
            return "";
        }

        if (Object.prototype.toString.call(item) === '[object String]') {
            return 'staticimage/' + item;
        }

        let imageFile = item['image_file'];
        return imageFile === undefined ? 'Images/Default.png' : this.httpService.getMenuBaseUrl() + '/staticimage/' + imageFile;
    }

    //---------------------------------------------------------------------------------------------

    getObjectIcon = function (object) {

        if (object.sub_type != undefined) {
            if (object.application == undefined)
                return this.getObjectIconForMaterial(object.sub_type);

            return this.getObjectIconForMaterial(object.sub_type + object.application);
        }

        return this.getObjectIconForMaterial(object.objectType);
    }

    getObjectIconForMaterial(target) {
        if (target == undefined)
            return;
        if (target.toLowerCase() == "document")
            return 'description';
        if (target.toLowerCase() == "report")
            return 'print';
        if (target.toLowerCase() == "batch")
            return 'brightness_low';
        if (target.toLowerCase() == "wizard")
            return 'color_lens';
        return 'close';
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