import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';

import { BOCommonComponent } from './bo-common.component';

import { EventDataService } from './../../../core/services/eventdata.service';
import { ComponentInfoService } from './../../../core/services/component-info.service';

@Component({
    selector: 'tb-bo-slave',
    template: '',
    styles: []
})
export class BOSlaveComponent extends BOCommonComponent implements OnInit, OnDestroy {

    constructor(
        eventData: EventDataService,
        ciService: ComponentInfoService,
        changeDetectorRef : ChangeDetectorRef
    ) {
        super(null, eventData, ciService, changeDetectorRef);
    }

    ngOnInit() {
        super.ngOnInit();
    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}