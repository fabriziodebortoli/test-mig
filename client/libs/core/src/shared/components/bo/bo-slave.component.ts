import { FrameComponent } from './../../containers/frame/frame.component';
import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild } from '@angular/core';

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
    @ViewChild(FrameComponent) frame: FrameComponent;
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
    public get title(): string {
        if (this.frame && this.frame.title) {
            this._title = this.frame.title;
        }
        return this._title;
    }
    public set title(val: string) {
        this._title = val;
    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }
}