import { Http } from '@angular/http';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { UtilsService } from '../../core/services/utils.service';
import { HttpService } from '../../core/services/http.service';
import { Logger } from '../../core/services/logger.service';
export declare class ImageService {
    protected http: Http;
    protected utils: UtilsService;
    protected logger: Logger;
    protected cookieService: CookieService;
    private httpService;
    constructor(http: Http, utils: UtilsService, logger: Logger, cookieService: CookieService, httpService: HttpService);
    getApplicationIcon(application: any): string;
    getStaticImage(item: any): string;
    getObjectIcon: (object: any) => any;
    getObjectIconForMaterial(target: any): "description" | "print" | "brightness_low" | "color_lens" | "close";
    isCustomImage: (object: any) => boolean;
    getWorkerImage: (item: any) => string;
}
