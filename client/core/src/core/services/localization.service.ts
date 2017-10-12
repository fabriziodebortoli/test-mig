import { HttpService } from './http.service';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Injectable, EventEmitter } from '@angular/core';
import { Http } from '@angular/http';

import { Logger } from './../../core/services/logger.service';

@Injectable()
export class LocalizationService {

    public localizedElements: any = undefined;
    public localizationsLoaded: BehaviorSubject<boolean> = new BehaviorSubject(false);

    constructor(
        public httpService: HttpService,
        public logger: Logger
    ) {
        this.logger.debug('LocalizationService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    //---------------------------------------------------------------------------------------------
    loadLocalizedElements(reset: boolean = false) {
        if (reset)
            this.localizedElements = undefined;

        if (this.localizedElements != undefined)
            return this.localizedElements;

        let subs = this.httpService.loadLocalizedElements().subscribe(result => {
            this.localizedElements = result.LocalizedElements;
            this.localizationsLoaded.next(true);
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
