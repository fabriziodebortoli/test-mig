import { OnInit } from '@angular/core';
import { ViewModeType } from '../../shared/models/view-mode-type.model';
import { TbComponent } from './tb.component';
import { DocumentService } from '../services/document.service';
import { EventDataService } from '../services/eventdata.service';
export declare abstract class DocumentComponent extends TbComponent implements OnInit {
    document: DocumentService;
    eventData: EventDataService;
    viewModeType: ViewModeType;
    title: string;
    args: any;
    constructor(document: DocumentService, eventData: EventDataService);
    ngOnInit(): void;
}
