import { HttpService } from './http.service';
import { BehaviorSubject } from '../../rxjs.imports';
import { Injectable, EventEmitter } from '@angular/core';
import { Http } from '@angular/http';

import { Logger } from './../../core/services/logger.service';

@Injectable()
export class OldLocalizationService {

    public localizedElements: any = undefined;
    public localizationsLoaded: BehaviorSubject<boolean> = new BehaviorSubject(false);

    constructor(
        public httpService: HttpService,
        public logger: Logger
    )
    {
    
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

};
