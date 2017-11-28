import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';

import { BOCommonComponent } from './bo-common.component';

import { EventDataService } from './../../../core/services/eventdata.service';
import { ComponentInfoService } from './../../../core/services/component-info.service';
import { BOService } from '../../../core/services/bo.service';

@Component({
    selector: 'tb-bo-slave',
    template: '',
    styles: []
})
export class BOSlaveComponent extends BOCommonComponent implements OnInit, OnDestroy {
    protected bo: BOService;
    constructor(
        eventData: EventDataService,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(null, eventData, ciService, changeDetectorRef);
    }

    ngOnInit() {
        super.ngOnInit();
        this.bo = this.document as BOService;
        }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}