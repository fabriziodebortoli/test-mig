import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { ComponentInfoService } from './../../../core/services/component-info.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { BOService } from './../../../core/services/bo.service';

import { DocumentComponent } from './../document.component';

@Component({
    selector: 'tb-bo',
    template: '',
    styles: []
})
export class BOCommonComponent extends DocumentComponent implements OnInit, OnDestroy {
    subscriptions: Subscription[] = [];

    constructor(
        document: BOService,
        eventData: EventDataService,
        ciService: ComponentInfoService
    ) {
        super(document, eventData, ciService);

        let me = this;
        this.subscriptions.push(document.windowStrings.subscribe((args: any) => {
            if (me.cmpId === args.id) {
                me.translations = args.strings;
                let jItem = { translations: me.translations, installationVersion: this.installationVersion };
                localStorage.setItem(this.dictionaryId, JSON.stringify(jItem));
            }
        }));
    }


    readTranslationsFromServer() {
        let s = this.document as BOService;
        s.getWindowStrings(this.cmpId, this.ciService.globalInfoService.culture.value);
    }
    ngOnInit() {
        const ci = this.ciService.componentInfo;
        this.dictionaryId = ci.app.toLowerCase() + '/' + ci.mod.toLowerCase() + '/' + ci.name + '/' + this.ciService.globalInfoService.culture.value;
        super.ngOnInit();
    }
    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }
}