import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { UtilsService } from '../../core/services/utils.service';
import { HttpService } from '../../core/services/http.service';
import { Logger } from '../../core/services/logger.service';
export class ImageService {
    /**
     * @param {?} http
     * @param {?} utils
     * @param {?} logger
     * @param {?} cookieService
     * @param {?} httpService
     */
    constructor(http, utils, logger, cookieService, httpService) {
        this.http = http;
        this.utils = utils;
        this.logger = logger;
        this.cookieService = cookieService;
        this.httpService = httpService;
        //---------------------------------------------------------------------------------------------
        this.getObjectIcon = function (object) {
            if (object.sub_type != undefined) {
                if (object.application == undefined)
                    return this.getObjectIconForMaterial(object.sub_type);
                return this.getObjectIconForMaterial(object.sub_type + object.application);
            }
            return this.getObjectIconForMaterial(object.objectType);
        };
        //---------------------------------------------------------------------------------------------
        this.isCustomImage = function (object) {
            return object.isCustomImage == undefined || object.isCustomImage == 'Images/Default.png';
        };
        //---------------------------------------------------------------------------------------------
        this.getWorkerImage = function (item) {
            if (item == undefined)
                return;
            var /** @type {?} */ imageFile = item['image_file'];
            if (imageFile == undefined || imageFile == '')
                return undefined;
            return 'staticimage/' + imageFile;
        };
        this.logger.debug('ImageService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    /**
     * @param {?} application
     * @return {?}
     */
    getApplicationIcon(application) {
        let /** @type {?} */ url = 'assets/images/';
        switch (application.name) {
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
    /**
     * @param {?} item
     * @return {?}
     */
    getStaticImage(item) {
        if (item == undefined) {
            return "";
        }
        if (Object.prototype.toString.call(item) === '[object String]') {
            return 'staticimage/' + item;
        }
        let /** @type {?} */ imageFile = item['image_file'];
        return imageFile === undefined ? 'Images/Default.png' : this.httpService.getMenuServiceUrl() + 'getStaticImage/?imageFile=' + imageFile;
    }
    /**
     * @param {?} target
     * @return {?}
     */
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
}
ImageService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
ImageService.ctorParameters = () => [
    { type: Http, },
    { type: UtilsService, },
    { type: Logger, },
    { type: CookieService, },
    { type: HttpService, },
];
function ImageService_tsickle_Closure_declarations() {
    /** @type {?} */
    ImageService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ImageService.ctorParameters;
    /** @type {?} */
    ImageService.prototype.getObjectIcon;
    /** @type {?} */
    ImageService.prototype.isCustomImage;
    /** @type {?} */
    ImageService.prototype.getWorkerImage;
    /** @type {?} */
    ImageService.prototype.http;
    /** @type {?} */
    ImageService.prototype.utils;
    /** @type {?} */
    ImageService.prototype.logger;
    /** @type {?} */
    ImageService.prototype.cookieService;
    /** @type {?} */
    ImageService.prototype.httpService;
}
