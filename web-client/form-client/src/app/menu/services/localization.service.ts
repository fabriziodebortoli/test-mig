import { Injectable } from '@angular/core';
import { Http } from '@angular/http';

import { UtilsService } from './../../core/utils.service';
import { HttpMenuService } from './http-menu.service';
import { ImageService } from './image.service';

import { Logger } from 'libclient';

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
