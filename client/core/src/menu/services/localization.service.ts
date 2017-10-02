import { Injectable, EventEmitter } from '@angular/core';
import { Http } from '@angular/http';

import { Logger } from './../../core/services/logger.service';
import { HttpMenuService } from './http-menu.service';

@Injectable()
export class LocalizationService {

    public localizedElements: any = undefined;
    public localizationsLoaded: EventEmitter<any> = new EventEmitter();

    constructor(
        public httpMenuService: HttpMenuService,
        public logger: Logger
    ) {
        this.logger.debug('LocalizationService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    //---------------------------------------------------------------------------------------------
    loadLocalizedElements() {

        if (this.localizedElements != undefined)
            return this.localizedElements;

        let subs = this.httpMenuService.loadLocalizedElements().subscribe(result => {
            this.localizedElements = result.LocalizedElements;
            this.localizationsLoaded.emit();
            subs.unsubscribe();
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
