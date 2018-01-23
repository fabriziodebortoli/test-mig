import { Injectable } from '@angular/core';
import { Http } from '@angular/http';

import { Logger } from './../../core/services/logger.service';
import { InfoService } from './../../core/services/info.service';
import { HttpService } from './../../core/services/http.service';

@Injectable()
export class ImageService {

    constructor(
        public http: Http,
        public logger: Logger,
        public httpService: HttpService,
        public infoService: InfoService
    ) { }

    getApplicationIcon(application) {

        let url = 'assets/images/';

        switch (application.name.toLowerCase()) {
            case 'erp':
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
        return imageFile === undefined ? '' : this.infoService.getMenuServiceUrl() + 'getStaticImage/?imageFile=' + imageFile;
    }

    //---------------------------------------------------------------------------------------------

    getObjectIcon(object) {

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
            return 'tb-document';
        if (target.toLowerCase() == "report")
            return 'tb-printfilled';
        if (target.toLowerCase() == "batch")
            return 'tb-options';
        if (target.toLowerCase() == "wizard")
            return 'tb-colours';
        if (target.toLowerCase() == "function")
            return 'tb-dossier';
        return 'close';
    }

    //---------------------------------------------------------------------------------------------
    isCustomImage(object) {
        return object.isCustomImage == undefined || object.isCustomImage == 'Images/Default.png';
    }

    //---------------------------------------------------------------------------------------------
    getWorkerImage(item) {

        if (item == undefined)
            return;

        var imageFile = item['image_file'];
        if (imageFile == undefined || imageFile == '')
            return undefined;

        return 'staticimage/' + imageFile;
    }
}