import { Component, OnInit, ChangeDetectorRef } from '@angular/core';

import { ComponentInfoService } from './../../core/services/component-info.service';
import { EventDataService } from './../../core/services/eventdata.service';
import { DocumentService } from './../../core/services/document.service';

import { ViewModeType } from "../models/view-mode-type.model";
import { TbComponent } from "../components/tb.component";

@Component({
    selector: 'tb-document',
    template: '',
    styles: []
})
export class DocumentComponent extends TbComponent implements OnInit {

    viewModeType: ViewModeType;
    protected _title ='';
    args: any;//used tu pass initialization arguments to the component
    public set title(val: string) {
        this._title = val;
    }
    public get title(): string {
        return this._title;
    }
    constructor(
        public document: DocumentService,
        public eventData: EventDataService,
        public ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
        super(document, changeDetectorRef);

    }

    ngOnInit() {
        this.viewModeType = this.document.getViewModeType();
        this._title = this.document.getHeader();
        super.ngOnInit();
    }


}