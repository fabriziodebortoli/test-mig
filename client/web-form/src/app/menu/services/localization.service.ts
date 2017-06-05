import { Injectable, EventEmitter } from '@angular/core';
import { Http } from '@angular/http';

import { UtilsService } from './../../core/utils.service';
import { HttpMenuService } from './http-menu.service';
import { ImageService } from './image.service';

import { Logger } from './../../core/logger.service';

@Injectable()
export class LocalizationService {

    public localizedElements: any = undefined;
    public localizationsLoaded: EventEmitter<any> = new EventEmitter();

    constructor(
        private httpMenuService: HttpMenuService,
        private utils: UtilsService,
        private logger: Logger
    ) {
        this.logger.debug('LocalizationService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    //---------------------------------------------------------------------------------------------
    loadLocalizedElements(needLoginThread) {

        if (this.localizedElements != undefined)
            return this.localizedElements;

        this.httpMenuService.loadLocalizedElements(needLoginThread).subscribe(result => {
            this.localizedElements = result.LocalizedElements;
            this.localizationsLoaded.emit();
        })
    }

    // //---------------------------------------------------------------------------------------------
    // getLocalizedElement(key) {
    //     if (this.localizedElements == undefined || this.localizedElements.LocalizedElement == undefined)
    //         return undefined;

    //     var allElements = this.localizedElements.LocalizedElement;
    //     for (var i = 0; i < allElements.length; i++) {
    //         if (allElements[i].key == key) {

    //             return allElements[i].value;
    //         }
    //     };

    //     return key;
    // }

};
