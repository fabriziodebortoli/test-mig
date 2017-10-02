import { Component, OnInit, OnDestroy } from '@angular/core';

import { BOCommonComponent } from './bo-common.component';

import { EventDataService } from './../../../core/services/eventdata.service';
import { ComponentInfoService } from './../../../core/services/component-info.service';

@Component({
    selector: 'tb-bo-slave',
    template: '',
    styles: []
})
export abstract class BOSlaveComponent extends BOCommonComponent implements OnInit, OnDestroy {

    constructor(
        eventData: EventDataService,
        ciService: ComponentInfoService
    ) {
        super(null, eventData, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}