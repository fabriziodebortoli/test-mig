import { Component, OnInit } from '@angular/core';

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
    title: string;
    args: any;//used tu pass initialization arguments to the component

    constructor(public document: DocumentService, public eventData: EventDataService) {
        super();
    }

    ngOnInit() {
        this.viewModeType = this.document.getViewModeType();
        this.title = this.document.getTitle();
    }

}