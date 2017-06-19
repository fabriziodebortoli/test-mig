import { EventEmitter } from '@angular/core';
import { UtilsService } from '../../core/services/utils.service';
import { HttpMenuService } from './http-menu.service';
import { Logger } from '../../core/services/logger.service';
export declare class LocalizationService {
    private httpMenuService;
    private utils;
    private logger;
    localizedElements: any;
    localizationsLoaded: EventEmitter<any>;
    constructor(httpMenuService: HttpMenuService, utils: UtilsService, logger: Logger);
    loadLocalizedElements(needLoginThread: any): any;
}
