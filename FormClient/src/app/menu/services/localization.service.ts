import { HttpMenuService } from './http-menu.service';
import { UtilsService, HttpService } from 'tb-core';
import { Logger } from 'libclient';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';
import { ImageService } from './image.service';

@Injectable()
export class LocalizationService {

    private localizedElements: any = undefined;

    constructor(
        private httpMenuService: HttpMenuService,
        private utils: UtilsService
    ) {
    }

    //---------------------------------------------------------------------------------------------
    loadLocalizedElements(needLoginThread) {

        if (this.localizedElements != undefined)
            return this.localizedElements;

        this.httpMenuService.loadLocalizedElements(needLoginThread).subscribe(result => {
            this.localizedElements = result.LocalizedElements;
        })
    }

    //---------------------------------------------------------------------------------------------
    getLocalizedElement(key) {
        if (this.localizedElements == undefined || this.localizedElements.LocalizedElement == undefined)
            return undefined;

        var allElements = this.localizedElements.LocalizedElement;
        for (var i = 0; i < allElements.length; i++) {
            if (allElements[i].key == key) {

                return allElements[i].value;
            }
        };

        return key;
    }

};
